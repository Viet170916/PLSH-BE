using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using API.DTO;
using AutoMapper;
using BU.Services.Interface;
using Common.Library;
using Data.DatabaseContext;
using Google.Apis.Auth;
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
          return Unauthorized(new BaseResponse<string> { message = "Token Google không hợp lệ." });
        }

        email = payload.Email;
        fullName = payload.Name;
      }
      else { return BadRequest(new BaseResponse<string> { message = "No field required found is incorrect." }); }

    var user = await context.Accounts.FirstOrDefaultAsync(acc => acc.Email == email);
    return Ok(new BaseResponse<VerifiedResponse>
    {
      message = "Request success",
      data = new VerifiedResponse
      {
        IsVerified = user is not null && user.IsVerified,
        EmailExisted = user is not null,
        Email = email,
        FullName = fullName,
      },
    });
  }

  [HttpPost("login")] public async Task<IActionResult> Login([FromBody] LoginRequest request)
  {
    var account = await context.Accounts.FirstOrDefaultAsync(a => a.Email == request.Email);
    if (account == null || !BCrypt.Net.BCrypt.Verify(request.Password, account.Password))
    {
      return Unauthorized(new BaseResponse<string> { message = "Email hoặc mật khẩu không đúng." });
    }

    var token = string.Empty;
    if (!account.IsVerified)
    {
      token = GenerateJwtToken(account, "notVerifiedUser", TimeSpan.FromDays(10));
      return Ok(new BaseResponse<string>
      {
        data = token, message = "Vui lòng đổi mật khẩu để hoàn tất xác minh tài khoản."
      });
    }

    var role = context.Roles.FirstOrDefault(r => r.Id == account.RoleId)?.Name;
    var tokenExpiration = TimeSpan.FromDays(7);
    token = GenerateJwtToken(account, role, tokenExpiration);
    return Ok(new BaseResponse<string> { data = token, message = "Đăng nhập thành công." });
  }

  [HttpPost("login/google")] public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginRequest_ request)
  {
    try
    {
      var payload = await GoogleJsonWebSignature.ValidateAsync(request.GoogleToken);
      var email = payload.Email;
      var fullName = payload.Name;
      var googleUserId = payload.Subject;
      var avatarUrl = payload.Picture;
      var account = await context.Accounts
                                 .Include(ac => ac.Role)
                                 .FirstOrDefaultAsync(a => a.Email == email);
      var roleId = context.Roles.FirstOrDefault(r => r.Name == request.Role)?.Id;
      

      if (account == null)
      {
        if (roleId is null)
        {
          return BadRequest(new BaseResponse<string> { message = $"Không hỗ trợ người dùng: {request.Role}" });
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
          RoleId = (int)roleId,
          IsVerified = true,
          AvatarUrl = avatarUrl,
          CreatedAt = DateTime.UtcNow
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
      }

      var token = GenerateJwtToken(account, account?.Role?.Name ?? request.Role, TimeSpan.FromDays(1));
      return Ok(new BaseResponse<string> { data = token, message = "Đăng nhập bằng Google thành công." });
    }
    catch (InvalidJwtException)
    {
      return Unauthorized(new BaseResponse<string> { message = "Token Google không hợp lệ." });
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
        new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
        new Claim(ClaimTypes.Email, account.Email),
        new Claim(ClaimTypes.Role, role)
      }),
      Expires = DateTime.UtcNow.Add(expiration),
      Issuer = Constants.Issuer,  
      Audience = Constants.Audience, 
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
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