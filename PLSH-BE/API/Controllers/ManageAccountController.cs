using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Data.DatabaseContext;
using BU.Models.DTO.Account.AccountDTO;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ManageAccountController(AppDbContext context, ILogger<ManageAccountController> logger)
    : ControllerBase
  {
    // GET: api/ManageAccount/{id}
    [HttpGet("{id}")] public async Task<IActionResult> GetAccount(int id)
    {
      var account = await context.Accounts.FindAsync(id);
      if (account == null)
      {
        logger.LogWarning($"AccountControllers with ID {id} not found.");
        return NotFound(new { message = "Tài khoản không tồn tại" });
      }

      return Ok(account);
    }

    // POST: api/ManageAccount
    //[HttpPost]
    //public async Task<IActionResult> CreateAccount([FromBody] AccountDTO accountDto)
    //{
    //    if (!ModelState.IsValid)
    //    {
    //        return BadRequest(ModelState);
    //    }

    //    var account = new AccountControllers
    //    {
    //        Email = accountDto.Email,
    //        FullName = accountDto.FullName,
    //        Password = BCrypt.Net.BCrypt.HashPassword(accountDto.Password),
    //        IdentityCardNumber = accountDto.IdentityCardNumber,
    //        RoleId = accountDto.RoleId,
    //        PhoneNumber = accountDto.PhoneNumber,
    //        Address = accountDto.Address,
    //        AvataUrl = accountDto.AvataUrl,
    //        isVerified = accountDto.isVerified,
    //        Status = accountDto.Status,
    //        CardMemberNumber = AccountDTO.GenerateUniqueId(),
    //        CardMemberStatus = accountDto.CardMemberStatus,
    //        CardMemberExpiredDate = accountDto.CardMemberExpiredDate,
    //        CreatedAt = DateTime.Now,
    //        UpdatedAt = DateTime.Now
    //    };

    //    await _context.Accounts.AddAsync(account);
    //    await _context.SaveChangesAsync();

    //    return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
    //}

    // PUT: api/ManageAccount/{id}
    [HttpPut("{id}")] public async Task<IActionResult> UpdateAccount(int id, [FromBody] AccountDto accountDto)
    {
      var account = await context.Accounts.FindAsync(id);
      if (account == null) { return NotFound(new { message = "Tài khoản không tồn tại" }); }

      //account.FullName = accountDto.FullName;
      //account.PhoneNumber = accountDto.PhoneNumber;
      //account.Address = accountDto.Address;
      //account.AvataUrl = accountDto.AvataUrl;
      account.Status = accountDto.Status;
      account.UpdatedAt = DateTime.Now;
      context.Accounts.Update(account);
      await context.SaveChangesAsync();
      return Ok(account);
    }

    [HttpDelete("{id}")] public async Task<IActionResult> DeleteAccount(int id)
    {
      var account = await context.Accounts.FindAsync(id);
      if (account == null) { return NotFound(new { message = "Tài khoản không tồn tại" }); }

      account.Status = "inactive";
      account.DeletedAt = DateTime.Now;
      context.Accounts.Update(account);
      await context.SaveChangesAsync();
      return Ok(new { message = "Xóa tài khoản thành công" });
    }

    [HttpGet("all")] public async Task<IActionResult> GetAllAccounts(
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 10
    )
    {
      if (page < 1 || pageSize < 1)
      {
        return BadRequest(new { message = "Số trang và kích thước trang phải lớn hơn 0." });
      }

      var totalAccounts = await context.Accounts.CountAsync();
      var totalPages = (int)Math.Ceiling(totalAccounts / (double)pageSize);
      var accounts = await context.Accounts
                                  .OrderBy(a => a.Id)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();
      var response = new
      {
        totalAccounts,
        totalPages,
        currentPage = page,
        pageSize,
        accounts
      };
      return Ok(response);
    }

    // ✅ Tìm kiếm tài khoản theo tên (không phân biệt hoa thường)
    [HttpGet("search")] public async Task<IActionResult> SearchByName([FromQuery] string name)
    {
      if (string.IsNullOrWhiteSpace(name)) { return BadRequest(new { message = "Vui lòng nhập tên cần tìm kiếm." }); }

      var accounts = await context.Accounts
                                  // .Where(a => a.FullName.ToLower().Contains(name.ToLower()))
                                  .ToListAsync();
      if (!accounts.Any()) { return NotFound(new { message = "Không tìm thấy tài khoản nào." }); }

      return Ok(accounts);
    }

    // ✅ Lọc tài khoản theo tên + trạng thái + vai trò
    [HttpGet("filter")] public async Task<IActionResult> FilterByName(
      [FromQuery] string? name,
      [FromQuery] string? status,
      [FromQuery] int? roleId
    )
    {
      var query = context.Accounts.AsQueryable();
      if (!string.IsNullOrWhiteSpace(name))
      {
        //query = query.Where(a => a.FullName.ToLower().Contains(name.ToLower()));
      }

      if (status is not null) { query = query.Where(a => a.Status == status); }

      if (roleId.HasValue) { query = query.Where(a => a.RoleId == roleId); }

      var accounts = await query.ToListAsync();
      if (!accounts.Any()) { return NotFound(new { message = "Không tìm thấy tài khoản phù hợp." }); }

      return Ok(accounts);
    }

    [HttpPut("status/{id}")] public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
    {
      var account = await context.Accounts.FindAsync(id);
      if (account == null) { return NotFound(new { message = "Tài khoản không tồn tại." }); }

      account.Status = newStatus;
      account.UpdatedAt = DateTime.Now;
      context.Accounts.Update(account);
      await context.SaveChangesAsync();
      return Ok(new { message = "Cập nhật trạng thái thành công.", newStatus });
    }
  }
}
