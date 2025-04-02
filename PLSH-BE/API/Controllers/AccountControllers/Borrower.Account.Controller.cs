using API.DTO;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.User;

namespace API.Controllers.AccountControllers;

public partial class AccountController
{
  [HttpPost("b-login")] public async Task<IActionResult> BorrowerLogin([FromBody] LoginRequest request)
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

  [HttpPost("b-login/google")] public async Task<IActionResult> BorrowerLoginWithGoogle([FromBody] GoogleLoginRequest_ request)
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
}
