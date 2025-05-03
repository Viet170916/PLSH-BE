using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.User;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using API.DTO;
using BU.Models.DTO;
using BU.Models.DTO.Account.AccountDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Controllers.AccountControllers;

public partial class AccountController
{
  [Authorize("LibrarianPolicy")] [HttpGet("member/{id}/validate")]
  public async Task<ActionResult<BaseResponse<string>>> ValidateAccount(int id)
  {
    var account = await context.Accounts
                               .Include(a => a.Role)
                               .FirstOrDefaultAsync(a => a.Id == id);
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
        if (string.IsNullOrWhiteSpace(account.IdentityCardNumber))
          errorMessages.Add("Giảng viên phải có số căn cước công dân");
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

  [Authorize("LibrarianPolicy")] [HttpPost("member/create")]
  public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest account)
  {
    if (await context.Accounts.AnyAsync(a => a.Email == account.Email))
    {
      return BadRequest(new { Message = "Email đã tồn tại." });
    }

    var roleId = context.Roles.FirstOrDefault(r => r.Name == account.Role)?.Id;
    if (roleId is null)
    {
      return BadRequest(new { Message = $"Hệ thống không hỗ trợ người dùng này {account.Role}", });
    }

    var generatedPassword = GenerateRandomPassword();
    var newAccount = new Account()
    {
      FullName = account.FullName,
      RoleId = (int)roleId,
      Email = account.Email,
      IdentityCardNumber = account.IdentityCardNumber,
      Password = BCrypt.Net.BCrypt.HashPassword(generatedPassword),
      CreatedAt = DateTime.UtcNow,
    };
    context.Accounts.Add(newAccount);
    await context.SaveChangesAsync();
    var appLink = "https://book-hive.space/auth/sign-in";
    await emailService.SendWelcomeEmailAsync(account.FullName, account.Email, generatedPassword, appLink);
    return Ok(new
    {
      Message =
        "Tài khoản đã được tạo thành công và email xác nhận đã được gửi. Hãy sử dụng mật khẩu trong email xác nhận để đăng nhập lần đầu.",
      Data = mapper.Map<Account>(newAccount),
    });
  }

  [Authorize("LibrarianPolicy")] [HttpGet("member")]
  public async Task<IActionResult> GetMembers(
    [FromQuery] int page = 1,
    [FromQuery] int limit = 10,
    [FromQuery] string orderBy = "FullName",
    [FromQuery] string orderDirection = "asc",
    [FromQuery] string? keyword = null,
    [FromQuery] string? role = null,
    [FromQuery] string? accountStatus = null,
    [FromQuery] string? approveStatus = null
  )
  {
    var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
    var roleToken = (await context.Accounts.Include(a => a.Role)
                                  .FirstOrDefaultAsync(a => a.Id == accountId))?.Role.Name;
    var trimedKeyword = keyword?.ToLower().Trim();
    if (page < 1) page = 1;
    if (limit < 1) limit = 10;
    if (limit > 100) limit = 100;
    var allowedOrderFields = new List<string> { "FullName", "Email", "CreatedAt" };
    if (!allowedOrderFields.Contains(orderBy))
    {
      return BadRequest(new { Message = $"orderBy phải là một trong: {string.Join(", ", allowedOrderFields)}" });
    }

    var query = context.Accounts.Include(a => a.Role).AsQueryable();
    if (roleToken is not ("librarian" or "admin")) { return Forbid(); }

    if (roleToken == "librarian") { query = query.Where(a => a.Role.Name == "student" || a.Role.Name == "teacher"); }

    if (!string.IsNullOrEmpty(trimedKeyword))
    {
      query = query.Where(a =>
        (a.FullName != null && a.FullName.Contains(trimedKeyword)) ||
        (a.Email != null && a.Email.Contains(trimedKeyword)) ||
        a.IdentityCardNumber == trimedKeyword ||
        a.CardMemberNumber.Contains(trimedKeyword) ||
        (a.PhoneNumber != null && a.PhoneNumber.Contains(trimedKeyword)));
    }

    if (!string.IsNullOrEmpty(role)) { query = query.Where(a => a.Role.Name == role); }

    if (!string.IsNullOrEmpty(accountStatus))
    {
      query = accountStatus switch
      {
        "isVerify" => query.Where(a => a.IsVerified),
        "notVerify" => query.Where(a => !a.IsVerified),
        _ => query
      };
    }

    // if (!string.IsNullOrEmpty(approveStatus)) { query = query.Where(a => a.Status == approveStatus); }
    query = query.OrderBy($"{orderBy} {orderDirection}");
    var totalCount = await query.CountAsync();
    var pageCount = (int)Math.Ceiling((double)totalCount / limit);

    if (page > pageCount && pageCount > 0) { page = 1; }

    var members = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();
    var membersDto = mapper.Map<List<AccountGDto>>(members);
    var result = new BaseResponse<List<AccountGDto>>
    {
      Data = membersDto,
      Count = totalCount,
      Page = page,
      Limit = limit,
      PageCount = pageCount
    };
    return Ok(result);
  }

  [Authorize] [HttpPut("member/update")] public async Task<IActionResult> UpdateAccountAsync(
    [FromBody] AccountGDto updateDto
  )
  {
    var account = await context.Accounts.FindAsync(updateDto.Id);
    if (account == null)
    {
      return BadRequest(new BaseResponse<AccountGDto> { Message = "Account not found", Status = "error" });
    }

    var roleId = context.Roles.FirstOrDefault(r => r.Name == updateDto.Role)?.Id;
    if (roleId is null)
    {
      return BadRequest(new { Message = $"Hệ thống không hỗ trợ người dùng này {updateDto.Role}", });
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
      var appLink = "https://Book-hive.space/login";
      await emailService.SendWelcomeEmailAsync(account.FullName, account.Email, generatedPassword, appLink);
    }

    mapper.Map(updateDto, account);
    account.UpdatedAt = DateTime.UtcNow;
    account.RoleId = (int)roleId;
    context.Accounts.Update(account);
    await context.SaveChangesAsync();
    return Ok(new BaseResponse<AccountGDto>
    {
      Message = "Account updated successfully", Data = mapper.Map<AccountGDto>(account), Status = "success",
    });
  }

  [Authorize] [HttpGet("member/{accountId:int}")]
  public async Task<IActionResult> GetAccountByIdAsync([FromRoute] int accountId)
  {
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

  private static string GenerateRandomPassword() { return Guid.NewGuid().ToString()[..8]; }
}

public class CreateAccountRequest
{
  public required string FullName { get; set; }
  public required string Email { get; set; }
  public required string Role { get; set; }
  public required string IdentityCardNumber { get; set; }
}
