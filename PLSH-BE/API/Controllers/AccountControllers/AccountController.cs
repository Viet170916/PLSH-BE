using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using API.DTO;
using AutoMapper;
using BU.Models.DTO;
using BU.Models.DTO.Account.AccountDTO;
using BU.Services.Interface;
using Common.Library;
using Data.DatabaseContext;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Model.Entity.User;

namespace API.Controllers.AccountControllers;

[ApiController]
[Route("api/v1/account")]
// [Authorize(Roles = "Admin")] // Chỉ Admin
public partial class AccountController(AppDbContext context, IEmailService emailService, IMapper mapper) : Controller
{
  [Authorize] [HttpGet()] public async Task<IActionResult> GetAccountAsync()
  {
    var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
    var account = await context.Accounts
                               .Include(a => a.Role)
                               .FirstOrDefaultAsync(m => m.Id == accountId);
    if (account == null)
    {
      return BadRequest(new BaseResponse<AccountGDto> { Message = "Account not found", Status = "error" });
    }

    return Ok(new BaseResponse<AccountGDto>
    {
      Message = "Account retrieved successfully", Data = mapper.Map<AccountGDto>(account), Status = "success"
    });
  }

  public class VerifiedResponse
  {
    public bool EmailExisted { get; set; }
    public bool IsVerified { get; set; }
    public string Email { get; set; }
    public string? FullName { get; set; }
  }

  [HttpGet("is-verified")]
  public async Task<IActionResult> IsVerified([FromQuery] string? Email, [FromQuery] string? GgToken)
  {
    string? email;
    string? fullName = null;
    if (Email is not null) { email = Email; }
    else
      if (GgToken is not null)
      {
        var payload = await GoogleJsonWebSignature.ValidateAsync(GgToken);
        if (payload is null)
        {
          return Unauthorized(new BaseResponse<string> { Message = "Token Google không hợp lệ." });
        }

        email = payload.Email;
        fullName = payload.Name;
      }
      else { return BadRequest(new BaseResponse<string> { Message = "No field required found is incorrect." }); }

    var user = await context.Accounts.FirstOrDefaultAsync(acc => acc.Email == email);
    return Ok(new BaseResponse<VerifiedResponse>
    {
      Message = "Request success",
      Data = new VerifiedResponse
      {
        IsVerified = user is not null && user.IsVerified,
        EmailExisted = user is not null,
        Email = email,
        FullName = fullName,
      },
    });
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginRequest request)
  {
    var account = await context.Accounts
                               .Include(a => a.Role)
                               .FirstOrDefaultAsync(a => a.Email == request.Email);
    if (account == null || !BCrypt.Net.BCrypt.Verify(request.Password, account.Password))
    {
      return Unauthorized(new BaseResponse<string> { Message = "Email hoặc mật khẩu không đúng." });
    }

    if (account.Role?.Name != "librarian")
    {
      return Unauthorized(new BaseResponse<string> { Message = "Chỉ thủ thư mới được phép đăng nhập." });
    }

    var token = string.Empty;
    if (!account.IsVerified)
    {
      token = GenerateJwtToken(account, "notVerifiedUser", TimeSpan.FromDays(10));
      return Ok(new BaseResponse<string>
      {
        Data = token,
        Message = "Vui lòng đổi mật khẩu để hoàn tất xác minh tài khoản."
      });
    }

    var tokenExpiration = TimeSpan.FromDays(7);
    token = GenerateJwtToken(account, account.Role.Name, tokenExpiration);
    return Ok(new BaseResponse<string> { Data = token, Message = "Đăng nhập thành công." });
  }


 [HttpPost("login/google")]
public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginRequest_ request)
{
    try
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(request.GoogleToken);
        var email = payload.Email;
        var fullName = payload.Name;
        var googleUserId = payload.Subject;
        var avatarUrl = payload.Picture;

        var account = await context.Accounts
                                   .Include(a => a.Role)
                                   .FirstOrDefaultAsync(a => a.Email == email);

        if (account != null)
        {
            if (account.Role?.Name != "librarian")
            {
                return Unauthorized(new BaseResponse<string> { Message = "Chỉ thủ thư mới được phép đăng nhập." });
            }
        }
        else
        {
            var librarianRoleId = await context.Roles
                                               .Where(r => r.Name == "librarian")
                                               .Select(r => r.Id)
                                               .FirstOrDefaultAsync();
            if (librarianRoleId == 0)
            {
                return BadRequest(new BaseResponse<string> { Message = "Không tìm thấy role thủ thư trong hệ thống." });
            }

            account = new Account
            {
                FullName = request.FullName ?? fullName,
                Email = email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                GoogleUserId = googleUserId,
                ClassRoom = request.ClassRoom,
                IdentityCardNumber = request.IdentityCardNumber,
                RoleId = librarianRoleId,
                IsVerified = true,
                AvatarUrl = avatarUrl,
                CreatedAt = DateTime.UtcNow
            };
            context.Accounts.Add(account);
            await context.SaveChangesAsync();
        }

        var token = GenerateJwtToken(account, account.Role?.Name ?? "librarian", TimeSpan.FromDays(1));
        return Ok(new BaseResponse<string> { Data = token, Message = "Đăng nhập bằng Google thành công." });
    }
    catch (InvalidJwtException)
    {
        return Unauthorized(new BaseResponse<string> { Message = "Token Google không hợp lệ." });
    }
}

  private static string GenerateJwtToken(Account account, string role, TimeSpan expiration)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable(Constants.JWT_SECRET) ?? string.Empty);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(new[]
      {
        new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()), new Claim(ClaimTypes.Email, account.Email),
        new Claim(ClaimTypes.Role, role)
      }),
      Expires = DateTime.UtcNow.Add(expiration),
      Issuer = Constants.Issuer,
      Audience = Constants.Audience,
      SigningCredentials =
        new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
  }

  private static async Task<bool> ValidateGoogleTokenAsync(string googleToken)
  {
    var httpClient = new HttpClient();
    var response = await httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={googleToken}");
    return response.IsSuccessStatusCode;
  }
  [Authorize] [HttpGet("validate")]
  public async Task<ActionResult<BaseResponse<string>>> ValidateThisAccount()
  {
    var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
    var account = await context.Accounts
                               .Include(a => a.Role)
                               .FirstOrDefaultAsync(a => a.Id == accountId);
    if (account == null)
    {
      return NotFound(new BaseResponse<string>
      {
        Message = "Không tìm thấy tài khoản", Data = "invalid", Status = "error"
      });
    }

    var errorMessages = new List<string>();
    if (account.CardMemberExpiredDate < DateTime.UtcNow)
    {
      return Ok(new BaseResponse<string> { Message = "Thẻ đã hết hạn", Data = "expired", Status = "error" });
    }

    if (string.IsNullOrWhiteSpace(account.FullName)) errorMessages.Add("Thiếu họ tên");
    if (string.IsNullOrWhiteSpace(account.PhoneNumber)) errorMessages.Add("Thiếu số điện thoại");
    if (string.IsNullOrWhiteSpace(account.Email)) errorMessages.Add("Thiếu email");
    if (!account.IsVerified) errorMessages.Add("Tài khoản chưa được xác minh");
    if (account.CardMemberStatus == 0) errorMessages.Add("Thẻ không ở trạng thái hợp lệ");
    if (account.Role.Name != "student" && account.Role.Name != "teacher") errorMessages.Add("Vai trò không hợp lệ");
    switch (account.Role.Name)
    {
      case "student":
        if (string.IsNullOrWhiteSpace(account.ClassRoom)) errorMessages.Add("Sinh viên phải có thông tin lớp học");
        break;
      case "teacher":
        if (string.IsNullOrWhiteSpace(account.IdentityCardNumber)) errorMessages.Add("Giảng viên phải có số căn cước công dân");
        break;
    }

    if (errorMessages.Count != 0)
    {
      return Ok(new BaseResponse<string>
      {
        Message = "Thông tin không hợp lệ: " + string.Join("; ", errorMessages), Data = "invalid", Status = "error"
      });
    }

    return Ok(new BaseResponse<string> { Message = "Tài khoản hợp lệ", Data = "valid", Status = "success" });
  }

  public class GoogleLoginRequest_
  {
    [Required]
    public string GoogleToken { get; set; } = string.Empty;

    public string? Email { get; set; } = string.Empty;
    public string? FullName { get; set; } = string.Empty;
    public string? IdentityCardNumber { get; set; }
    public string? Role { get; set; } = string.Empty;
    public string? GoogleUserId { get; set; }
    public string? ClassRoom { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
  }
}
