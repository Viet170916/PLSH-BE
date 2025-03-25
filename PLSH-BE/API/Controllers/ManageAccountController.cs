using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Data.DatabaseContext;
using API.DTO.Account.AccountDTO;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageAccountController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ManageAccountController> _logger;

        public ManageAccountController(AppDbContext context, ILogger<ManageAccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ManageAccount/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                _logger.LogWarning($"AccountControllers with ID {id} not found.");
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
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] AccountDto accountDto)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound(new { message = "Tài khoản không tồn tại" });
            }

            //account.FullName = accountDto.FullName;
            //account.PhoneNumber = accountDto.PhoneNumber;
            //account.Address = accountDto.Address;
            //account.AvataUrl = accountDto.AvataUrl;
            account.Status = accountDto.Status;
            account.UpdatedAt = DateTime.Now;

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return Ok(account);
        }

        // DELETE: api/ManageAccount/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound(new { message = "Tài khoản không tồn tại" });
            }

            account.Status = 2; // Cập nhật trạng thái thành "đã xóa"
            account.DeletedAt = DateTime.Now;

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa tài khoản thành công" });
        }



        // ✅ Lấy tất cả tài khoản với phân trang
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAccounts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new { message = "Số trang và kích thước trang phải lớn hơn 0." });
            }

            var totalAccounts = await _context.Accounts.CountAsync(); // Tổng số tài khoản
            var totalPages = (int)Math.Ceiling(totalAccounts / (double)pageSize);

            var accounts = await _context.Accounts
                .OrderBy(a => a.Id) // Sắp xếp theo ID tăng dần
                .Skip((page - 1) * pageSize) // Bỏ qua các bản ghi trước đó
                .Take(pageSize) // Lấy số lượng tài khoản theo pageSize
                .ToListAsync();

            var response = new
            {
                totalAccounts,  // Tổng số tài khoản
                totalPages,     // Tổng số trang
                currentPage = page,
                pageSize,
                accounts
            };

            return Ok(response);
        }


        // ✅ Tìm kiếm tài khoản theo tên (không phân biệt hoa thường)
        [HttpGet("search")]
        public async Task<IActionResult> SearchByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "Vui lòng nhập tên cần tìm kiếm." });
            }

            var accounts = await _context.Accounts
               // .Where(a => a.FullName.ToLower().Contains(name.ToLower()))
                .ToListAsync();

            if (!accounts.Any())
            {
                return NotFound(new { message = "Không tìm thấy tài khoản nào." });
            }

            return Ok(accounts);
        }

        // ✅ Lọc tài khoản theo tên + trạng thái + vai trò
        [HttpGet("filter")]
        public async Task<IActionResult> FilterByName(
            [FromQuery] string? name,
            [FromQuery] int? status,
            [FromQuery] int? roleId)
        {
            var query = _context.Accounts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                //query = query.Where(a => a.FullName.ToLower().Contains(name.ToLower()));
            }

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status);
            }

            if (roleId.HasValue)
            {
                query = query.Where(a => a.RoleId == roleId);
            }

            var accounts = await query.ToListAsync();

            if (!accounts.Any())
            {
                return NotFound(new { message = "Không tìm thấy tài khoản phù hợp." });
            }

            return Ok(accounts);
        }

        // ✅ Cập nhật trạng thái tài khoản
        [HttpPut("status/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] int newStatus)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound(new { message = "Tài khoản không tồn tại." });
            }

            account.Status = newStatus;
            account.UpdatedAt = DateTime.Now;

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái thành công.", newStatus });
        }
    }
}
