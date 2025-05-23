using System.Security.Claims;
using API.DTO;
using BU.Models.DTO;
using BU.Models.DTO.Account.AccountDTO;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.User;

namespace API.Controllers.AccountControllers;

public partial class AccountController
{
  [HttpPost("b-login")] public async Task<IActionResult> BorrowerLogin([FromBody] LoginRequest request)
  {
    var account = await context.Accounts
                               .Include(a => a.Role)
                               .FirstOrDefaultAsync(a => a.Email == request.Email);
    if (account == null || !BCrypt.Net.BCrypt.Verify(request.Password, account.Password))
    {
      return Unauthorized(new BaseResponse<string> { Message = "Email hoặc mật khẩu không đúng." });
    }

    if (account.Role == null ||
        (account.Role.Name != "student" && account.Role.Name != "teacher"))
    {
      return Unauthorized(new BaseResponse<string>
      {
        Message = "Chỉ học sinh hoặc giáo viên mới được phép đăng nhập."
      });
    }

    var token = string.Empty;
    if (!account.IsVerified)
    {
      token = GenerateJwtToken(account, "notVerifiedUser", TimeSpan.FromDays(10));
      return Ok(new BaseResponse<string>
      {
        Data = token, Message = "Vui lòng đổi mật khẩu để hoàn tất xác minh tài khoản."
      });
    }

    var tokenExpiration = TimeSpan.FromDays(7);
    token = GenerateJwtToken(account, account.Role.Name, tokenExpiration);
    return Ok(new BaseResponse<string> { Data = token, Message = "Đăng nhập thành công." });
  }

  public class ChangePasswordRequest
  {
    public string? OldPassword { get; set; }
    public required string Password { get; set; }
    public required string ReEnterPassword { get; set; }
  }

  [Authorize] [HttpPost("b-change-password")]
  public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.ReEnterPassword))
    {
      return BadRequest(new BaseResponse<string> { Message = "Mật khẩu không được để trống." });
    }

    if (request.Password != request.ReEnterPassword)
    {
      return BadRequest(new BaseResponse<string> { Message = "Mật khẩu xác nhận không khớp." });
    }

    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");
    var account = await context.Accounts
                               .Include(a => a.Role)
                               .FirstOrDefaultAsync(a => a.Id == userId);
    if (account == null) { return NotFound(new BaseResponse<string> { Message = "Không tìm thấy người dùng." }); }

    if (!account.IsVerified)
    {
      if (request.Password != request.ReEnterPassword)
      {
        return BadRequest(new BaseResponse<string> { Message = "Mật khẩu và mật khẩu nhập lại không khớp." });
      }

      account.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
      account.IsVerified = true;
      context.Accounts.Update(account);
      await context.SaveChangesAsync();
      var token = GenerateJwtToken(account, account.Role.Name, TimeSpan.FromDays(10));
      return Ok(new BaseResponse<string>
      {
        Data = token, Message = "Mật khẩu đã được thay đổi thành công và tài khoản đã được xác minh."
      });
    }
    else
    {
      if (request.OldPassword == null || request.Password == null || request.ReEnterPassword == null)
      {
        return BadRequest(new BaseResponse<string> { Message = "Vui lòng nhập đầy đủ thông tin." });
      }

      var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.OldPassword, account.Password);
      if (!isPasswordValid) { return Unauthorized(new BaseResponse<string> { Message = "Mật khẩu cũ không đúng." }); }

      if (request.Password != request.ReEnterPassword)
      {
        return BadRequest(new BaseResponse<string> { Message = "Mật khẩu và mật khẩu nhập lại không khớp." });
      }

      account.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
      context.Accounts.Update(account);
      await context.SaveChangesAsync();
      var role = context.Roles.FirstOrDefault(r => r.Id == account.RoleId)?.Name;
      var tokenExpiration = TimeSpan.FromDays(7);
      var token = GenerateJwtToken(account, role, tokenExpiration);
      return Ok(new BaseResponse<string> { Data = token, Message = "Mật khẩu đã được thay đổi thành công." });
    }
  }

  [HttpPost("b-login/google")]
  public async Task<IActionResult> BorrowerLoginWithGoogle([FromBody] GoogleLoginRequest_ request)
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
      if (account != null)
      {
        // Tài khoản đã tồn tại, kiểm tra role
        if (account.Role == null ||
            (account.Role.Name != "student" && account.Role.Name != "teacher"))
        {
          return Unauthorized(new BaseResponse<string>
          {
            Message = "Chỉ học sinh hoặc giáo viên mới được phép đăng nhập."
          });
        }
      }
      else
      {
        // Tài khoản chưa có, phải chọn role là student hoặc teacher
        if (request.Role != "student" && request.Role != "teacher")
        {
          return BadRequest(new BaseResponse<string> { Message = "Chỉ hỗ trợ vai trò học sinh hoặc giáo viên." });
        }

        var roleId = await context.Roles
                                  .Where(r => r.Name == request.Role)
                                  .Select(r => r.Id)
                                  .FirstOrDefaultAsync();
        if (roleId == 0)
        {
          return BadRequest(new BaseResponse<string>
          {
            Message = $"Không tìm thấy vai trò {request.Role} trong hệ thống."
          });
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
          RoleId = roleId,
          IsVerified = true,
          AvatarUrl = avatarUrl,
          CreatedAt = DateTime.UtcNow
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
      }

      var token = GenerateJwtToken(account, account.Role?.Name ?? request.Role, TimeSpan.FromDays(1));
      return Ok(new BaseResponse<string> { Data = token, Message = "Đăng nhập bằng Google thành công." });
    }
    catch (InvalidJwtException)
    {
      return Unauthorized(new BaseResponse<string> { Message = "Token Google không hợp lệ." });
    }
  }

  [Authorize] [HttpPut("borrower/update")]
  public async Task<IActionResult> UpdateBorrowerAccountAsync(
    [FromBody] AccountGDto updateDto
  )
  {
    var account = await context.Accounts.FindAsync(updateDto.Id);
    if (account == null)
    {
      return BadRequest(new BaseResponse<AccountGDto> { Message = "Account not found", Status = "error" });
    }

    if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != account.Email)
    {
      var existingAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Email == updateDto.Email);
      if (existingAccount != null)
      {
        return BadRequest(new BaseResponse<AccountGDto> { Message = "Email is already in use", Status = "error" });
      }

      var generatedPassword = GenerateRandomPassword();
      account.Email = updateDto.Email;
      account.Password = BCrypt.Net.BCrypt.HashPassword(generatedPassword);
      var appLink = "https://book-hive.space/login";
      await emailService.SendWelcomeEmailAsync(account.FullName, account.Email, generatedPassword, appLink);
    }

    mapper.Map(updateDto, account);
    account.UpdatedAt = DateTime.UtcNow;
    context.Accounts.Update(account);
    await context.SaveChangesAsync();
    return Ok(new BaseResponse<AccountGDto>
    {
      Message = "Account updated successfully", Data = mapper.Map<AccountGDto>(account), Status = "success",
    });
  }
}
