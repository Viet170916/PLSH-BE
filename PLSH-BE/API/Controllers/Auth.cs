using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.DTO.Account;
using API.DTO.Account.AccountDTO;
using BU.Services.Implementation;
using BU.Services.Interface;
using Common.Enums;
using Common.Helper;
using Common.Library;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Model.Entity;
using Model.Entity.User;
using Model.ResponseModel;
using Newtonsoft.Json;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/auth")]
public class AuthController(
  IAccountService accountService,
  ILogger<AuthController> logger,
  AppDbContext context,
  IEmailService emailService,
  IOTPService otpService,
  RedisService redisService,
  IConfiguration configuration
) : Controller
{
  private readonly EmailSettings _emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();

  [HttpGet("check-token")] public IActionResult CheckToken()
  {
    var email = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
    var id = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "-1");
    var fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
    return Ok(new OkResponse { Data = new { Id = id, Fullname = fullname, Email = email }, });
  }

  [AllowAnonymous] [HttpPost("sign-in")] public async Task<IActionResult> SignIn([FromBody] GoogleLoginRequest request)
  {
    try
    {
      logger.LogInformation($"/api/v1/auth/sign-in...............");
      var googleUser = await GetGoogleUserInfo(request.GoogleToken);
      if (googleUser == null)
      {
        return Ok(new ErrorResponse
        {
          Status = HttpStatus.UNAUTHORIZED.GetDescription(),
          StatusCode = HttpStatus.UNAUTHORIZED,
          Message = "Unauthorized access.",
        });
      }

      var user = await accountService.GetOrCreateUserAsync(googleUser.Email);
      if (user is null)
      {
        return Ok(new ErrorResponse
        {
          Status = HttpStatus.NOT_FOUND.GetDescription(),
          StatusCode = HttpStatus.NOT_FOUND,
          Message = "User not found.",
          Data = new { IsAuthorized = false, account = googleUser.RemoveNullProperties(), },
        });
      }

      var token = GenerateJwt(user);
      return Ok(new OkResponse
      {
        Status = HttpStatus.OK.GetDescription(),
        StatusCode = HttpStatus.OK,
        Message = "Login successful.",
        Data = new { IsAuthenticated = true, Account = user.RemoveNullProperties(), AccessToken = token, },
      });
    }
    catch (HttpRequestException e)
    {
      logger.LogError(e, ">>>>>>>Request: /api/v1/auth/sign-in...............An error occured");
      return Ok(new ErrorResponse
      {
        Status = HttpStatus.UNAUTHORIZED.GetDescription(),
        StatusCode = HttpStatus.UNAUTHORIZED,
        Message = "Unauthorized access.",
      });
    }
    
  }

  [AllowAnonymous] [HttpPost("login")] public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
  {
    try
    {
      logger.LogInformation($">>>>>>>Request: /api/v1/auth/login...............");
      var user = await accountService.GetUserByEmailAsync(request.Email);
      if (user == null)
      {
        return Ok(new ErrorResponse
        {
          Status = HttpStatus.NOT_FOUND.GetDescription(),
          StatusCode = HttpStatus.NOT_FOUND,
          Message = "User not found.",
          Data = new { IsAuthenticated = false }
        });
      }

      if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
      {
        return Ok(new ErrorResponse
        {
          Status = HttpStatus.FORBIDDEN.GetDescription(),
          StatusCode = HttpStatus.FORBIDDEN,
          Message = $"Tài khoản bị khóa đến {user.LockoutEnd:HH:mm dd/MM/yyyy}.",
          Data = new { IsLockedOut = true, LockoutEnd = user.LockoutEnd }
        });
      }

      var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
      if (!isPasswordValid)
      {
        // Tăng số lần đăng nhập sai
        user.FailedLoginAttempts++;

        // Xử lý khóa tài khoản theo số lần đăng nhập sai
        switch (user.FailedLoginAttempts)
        {
          case 3:
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(5); // Khóa 5 phút
            break;
          case 4:
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(20); // Khóa 20 phút
            break;
          case 5:
            user.LockoutEnd = DateTime.MaxValue; // Khóa vĩnh viễn
            break;
        }

        await accountService.UpdateUserAsync(user);

        // Trả về thông báo tùy theo số lần đăng nhập sai
        if (user.FailedLoginAttempts >= 5)
        {
          return Ok(new ErrorResponse
          {
            Status = HttpStatus.FORBIDDEN.GetDescription(),
            StatusCode = HttpStatus.FORBIDDEN,
            Message = "Tài khoản bị khóa vĩnh viễn do 5 lần đăng nhập sai liên tiếp.",
            Data = new { IsLockedOut = true, LockoutEnd = user.LockoutEnd }
          });
        }
        else
          if (user.FailedLoginAttempts >= 3)
          {
            return Ok(new ErrorResponse
            {
              Status = HttpStatus.FORBIDDEN.GetDescription(),
              StatusCode = HttpStatus.FORBIDDEN,
              Message = $"Tài khoản bị khóa đến {user.LockoutEnd:HH:mm dd/MM/yyyy}.",
              Data = new { IsLockedOut = true, LockoutEnd = user.LockoutEnd }
            });
          }
          else
          {
            return Ok(new ErrorResponse
            {
              Status = HttpStatus.UNAUTHORIZED.GetDescription(),
              StatusCode = HttpStatus.UNAUTHORIZED,
              Message = $"Sai mật khẩu. Bạn còn {5 - user.FailedLoginAttempts} lần thử.",
              Data = new { RemainingAttempts = 5 - user.FailedLoginAttempts }
            });
          }
      }

      // 🔥 KIỂM TRA NẾU MẬT KHẨU QUÁ 90 NGÀY 🔥
      if (await accountService.IsPasswordExpiredAsync(user.Id))
      {
        return Ok(new ErrorResponse
        {
          Status = HttpStatus.FORBIDDEN.GetDescription(),
          StatusCode = HttpStatus.FORBIDDEN,
          Message = "Bạn cần đổi mật khẩu vì đã quá 90 ngày.",
          Data = new { IsPasswordExpired = true }
        });
      }

      // Đăng nhập thành công: Reset số lần thử và mở khóa (nếu có)
      user.FailedLoginAttempts = 0;
      user.LockoutEnd = null;
      await accountService.UpdateUserAsync(user);

      // Tạo token và trả về kết quả
      var accessToken = GenerateJwt(user);
      var refreshToken = GenerateRefreshToken();

      //user.Token = accessToken;
      user.RefreshToken = refreshToken;
      user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
      await accountService.UpdateUserAsync(user);
      var otp = otpService.GenerateOtp();
      otpService.StoreOtp(request.Email, otp);
      await emailService.SendOtpEmailAsync(request.Email, otp);
      return Ok(new { RequiresOtp = true, Message = "Vui lòng kiểm tra email để lấy OTP" });
      return Ok(new OkResponse
      {
        Status = HttpStatus.OK.GetDescription(),
        StatusCode = HttpStatus.OK,
        Message = "Login successful.",
        Data = new
        {
          IsAuthenticated = true,
          Account = user.RemoveNullProperties(),
          AccessToken = accessToken,
          RefreshToken = refreshToken,
        }
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, ">>>>>>>Request: /api/v1/auth/login...............An error occurred.");
      return Ok(new ErrorResponse
      {
        Status = HttpStatus.NOT_FOUND.GetDescription(),
        StatusCode = HttpStatus.NOT_FOUND,
        Message = "An unexpected error occurred.",
      });
    }
  }

  //SendOTP
  [AllowAnonymous] [HttpPost("send-otp")]
  public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request, [FromServices] RedisService redisService)
  {
    if (string.IsNullOrEmpty(request.Email)) return BadRequest("Email không được để trống");
    var user = await accountService.GetUserByEmailAsync(request.Email);
    if (user == null) return NotFound("Người dùng không tồn tại");
    var otp = otpService.GenerateOtp();
    await redisService.SetOtpAsync(request.Email, otp);
    await emailService.SendOtpEmailAsync(request.Email, otp);
    Console.WriteLine($"OTP gửi đến {request.Email}: {otp}");
    return Ok(new { Message = "Đã gửi OTP qua email" });
  }

  [AllowAnonymous] [HttpPost("verify-otp")]
  public async Task<IActionResult> VerifyOtp(
    [FromBody] VerifyOtpRequestDto request,
    [FromServices] RedisService redisService
  )
  {
    if (string.IsNullOrEmpty(request.Email)) return Unauthorized("Không tìm thấy email, vui lòng đăng nhập lại.");
    var storedOtp = await redisService.GetOtpAsync(request.Email);
    if (string.IsNullOrEmpty(storedOtp)) return BadRequest("OTP đã hết hạn hoặc không tồn tại.");
    if (storedOtp != request.Otp) return BadRequest("OTP không chính xác.");

    // Xóa OTP sau khi xác minh thành công
    await redisService.RemoveOtpAsync(request.Email);
    var user = await accountService.GetUserByEmailAsync(request.Email);
    if (user == null) return NotFound("Người dùng không tồn tại");
    var token = GenerateJwt(user);
    return Ok(new OkResponse
    {
      Status = HttpStatus.OK.GetDescription(),
      StatusCode = HttpStatus.OK,
      Message = "Login successful.",
      Data = new
      {
        IsAuthenticated = true,
        Account = user.RemoveNullProperties(),
        AccessToken = token,
        RefreshToken = GenerateRefreshToken()
      }
    });
  }

  //Refresh Token

  [AllowAnonymous] [HttpPost("refresh-token")]
  public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO request)
  {
    try
    {
      logger.LogInformation($">>>>>>>Request: /api/v1/auth/refresh-token...............");

      // Find the user by refresh token
      var user = await accountService.GetUserByRefreshTokenAsync(request.RefreshToken);
      if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
      {
        return Ok(new ErrorResponse
        {
          Status = HttpStatus.UNAUTHORIZED.GetDescription(),
          StatusCode = HttpStatus.UNAUTHORIZED,
          Message = "Invalid or expired refresh token.",
        });
      }

      // Generate new tokens
      var accessToken = GenerateJwt(user);
      var newRefreshToken = GenerateRefreshToken();

      // Update the user's refresh token and expiry
      user.RefreshToken = newRefreshToken;
      user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
      await accountService.UpdateUserAsync(user);
      return Ok(new OkResponse
      {
        Status = HttpStatus.OK.GetDescription(),
        StatusCode = HttpStatus.OK,
        Message = "Token refreshed successfully.",
        Data = new { AccessToken = accessToken, RefreshToken = newRefreshToken, }
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An unexpected error occurred");
      return StatusCode(500,
        new ErrorResponse
        {
          Status = HttpStatus.NOT_FOUND.GetDescription(),
          StatusCode = HttpStatus.NOT_FOUND,
          Message = "An unexpected error occurred.",
          Data = new { errorDetails = ex.Message }
        });
    }
  }

  private string GenerateRefreshToken()
  {
    var randomNumber = new byte[32];
    using (var rng = RandomNumberGenerator.Create()) { rng.GetBytes(randomNumber); }

    return Convert.ToBase64String(randomNumber);
  }

  // [Authorize(Policy = "Bearer")] 
  private async Task<GoogleUserInfo> GetGoogleUserInfo(string googleToken)
  {
    var client = new HttpClient();
    var response =
      await client.GetStringAsync($"{SourceConstants.GET_USER_INFO}?access_token={googleToken}");
    return JsonConvert.DeserializeObject<GoogleUserInfo>(response);
  }

  private string GenerateJwt(Account user)
  {
    var claims = new[]
    {
      new Claim(ClaimTypes.Name, user.FullName), new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new Claim(ClaimTypes.Email, user.Email),
    };
    var key = new SymmetricSecurityKey(
      Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable(Constants.JWT_SECRET) ?? string.Empty));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(issuer: Constants.Issuer,
      audience: Constants.Audience,
      claims: claims,
      expires: DateTime.Now.AddDays(1),
      signingCredentials: creds);
    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  [AllowAnonymous] [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
  {
    try
    {
      // Kiểm tra tính hợp lệ của model
      if (!ModelState.IsValid)
      {
        return BadRequest(new { Message = "Thông tin đăng ký không hợp lệ", Errors = ModelState });
      }

      // Kiểm tra email đã tồn tại
      var existingUser = await accountService.GetUserByEmailAsync(request.Email);
      if (existingUser != null) { return Conflict(new { Message = "Email này đã được sử dụng" }); }

      // Tạo tài khoản mới
      var newAccount = new Account
      {
        Email = request.Email,
        Password = BCrypt.Net.BCrypt.HashPassword(request.Password), // Mã hóa mật khẩu
        FullName = string.Empty, // Có thể để người dùng cập nhật sau
        IdentityCardNumber = null,
        RoleId = 2, // Gán quyền mặc định (VD: 2 là người dùng bình thường)
        PhoneNumber = string.Empty,
        Address = string.Empty,
        GoogleToken = null,
        GoogleUserId = null,
        AvatarUrl = null,
        IsVerified = false, // Mặc định chưa xác minh
        Status = 0, // Chờ duyệt
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        DeletedAt = DateTime.MinValue,
        CardMemberExpiredDate = DateTime.UtcNow.AddYears(1) // Hạn thẻ mặc định 1 năm
      };

      // Lưu tài khoản vào cơ sở dữ liệu
      var createdAccount = await accountService.CreateUserAsync(newAccount);
      return Ok(new
      {
        Message = "Tài khoản đăng ký thành công",
        Data = new
        {
          createdAccount.Id,
          createdAccount.Email,
          createdAccount.RoleId,
          createdAccount.Status,
          createdAccount.CreatedAt,
          isVerified = createdAccount.IsVerified
        }
      });
    }
    catch (Exception ex)
    {
      // Log lỗi nếu cần
      return StatusCode(500, new { Message = "Đã xảy ra lỗi trong quá trình đăng ký", Error = ex.Message });
    }
  }
}

public class GoogleLoginRequest
{
  public string GoogleToken { get; set; }
}

public class GoogleUserInfo
{
  public string Sub { get; set; }
  public string Name { get; set; }
  public string Email { get; set; }
  public string Picture { get; set; }
}