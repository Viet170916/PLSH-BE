using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.User;
using System.Linq.Dynamic.Core;
using API.DTO;
using API.DTO.Account.AccountDTO;

namespace API.Controllers.AccountControllers;

public partial class AccountController
{
  [HttpPost("member/create")] public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest account)
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
    var appLink = "https://book-hive.space/login";
    await emailService.SendWelcomeEmailAsync(account.FullName, account.Email, generatedPassword, appLink);
    return Ok(new
    {
      Message =
        "Tài khoản đã được tạo thành công và email xác nhận đã được gửi. Hãy sử dụng mật khẩu trong email xác nhận để đăng nhập lần đầu.",
      Data = mapper.Map<Account>(newAccount),
    });
  }

  [HttpGet("member")] public async Task<IActionResult> GetMembers(
    [FromQuery] int page = 1,
    [FromQuery] int limit = 10,
    [FromQuery] string orderBy = "FullName",
    [FromQuery] string orderDirection = "asc"
  )
  {
    if (page < 1) page = 1;
    if (limit < 1) limit = 10;
    if (limit > 100) limit = 100;
    var allowedOrderFields = new List<string> { "FullName", "Email", "CreatedAt", };
    if (!allowedOrderFields.Contains(orderBy))
    {
      return BadRequest(new { Message = $"orderBy phải là một trong: {string.Join(", ", allowedOrderFields)}" });
    }

    var query = context.Accounts
                       .Include(a => a.Role)
                       .AsQueryable();
    query = query.OrderBy($"{orderBy} {orderDirection}");
    var totalCount = await query.CountAsync();
    var members = await query
                        .Skip((page - 1) * limit)
                        .Take(limit)
                        .ToListAsync();
    var membersDto = mapper.Map<List<AccountGDto>>(members);
    var result = new { Data = membersDto, Count = totalCount, Page = page, Limit = limit };
    return Ok(result);
  }

  [HttpPut("member/update/{accountId:int}")]
  public async Task<BaseResponse<AccountDto>> UpdateAccountAsync(
    [FromRoute] int accountId,
    [FromBody] AccountDto updateDto
  )
  {
    var account = await context.Accounts.FindAsync(accountId);
    if (account == null) { return new BaseResponse<AccountDto> { message = "Account not found", status = "error" }; }

    if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != account.Email)
    {
      var existingAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Email == updateDto.Email);
      if (existingAccount != null)
      {
        return new BaseResponse<AccountDto> { message = "Email is already in use", status = "error" };
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
    return new BaseResponse<AccountDto>
    {
      message = "Account updated successfully", data = mapper.Map<AccountDto>(account), status = "success",
    };
  }

  [HttpPut("member/{accountId:int}")]
  public async Task<BaseResponse<AccountDto>> GetAccountByIdAsync([FromRoute] int accountId)
  {
    var account = await context.Accounts.FindAsync(accountId);
    return account == null ?
      new BaseResponse<AccountDto> { message = "Account not found", status = "error" } :
      new BaseResponse<AccountDto>
      {
        message = "Account retrieved successfully", data = mapper.Map<AccountDto>(account), status = "success"
      };
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