using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.User;
using Data.DatabaseContext;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Model.Entity;
using System.IO;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace API.Controllers.Profile
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }
        public class UpdateProfileDto
        {
            [MaxLength(255)]
            public string? FullName { get; set; }

            public bool? Gender { get; set; }

            public DateTime? Birthdate { get; set; }

            [MaxLength(255)]
            public string? Address { get; set; }

            [MaxLength(20)]
            public string? PhoneNumber { get; set; }

            [MaxLength(255)]
            [EmailAddress]
            public string? Email { get; set; }

            public int? ResourceId { get; set; } 

            [MaxLength(20)]
            public string? IdentityCardNumber { get; set; }
        }

        [HttpPost("upload-avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Không thể xác thực người dùng" });
            }
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Vui lòng chọn file ảnh" });
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                return BadRequest(new { message = "Chỉ chấp nhận file ảnh (JPEG, PNG)" });
            }
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { message = "Kích thước ảnh không được vượt quá 5MB" });
            }

            // Tạo tên file duy nhất
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var resource = new Resource
            {
                Type = "Image",
                Name = file.FileName,
                SizeByte = file.Length,
                FileType = file.ContentType,
                LocalUrl = $"/uploads/avatars/{uniqueFileName}"
            };
            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == userId);
            if (account != null)
            {
                account.AvatarUrl = resource.LocalUrl;
                account.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                resourceId = resource.Id,
                avatarUrl = resource.LocalUrl,
                message = "Upload ảnh đại diện thành công"
            });
           
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateProfileDto model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Không thể xác thực người dùng" });
            }

            // Find the user in the database
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == userId && a.Status == "active");

            if (account == null)
            {
                return NotFound(new { message = "Tài khoản không tồn tại hoặc đã bị khóa" });
            }

            // Validate input
            var errors = new List<string>();

            if (!string.IsNullOrEmpty(model.PhoneNumber) && !System.Text.RegularExpressions.Regex.IsMatch(model.PhoneNumber, @"^\d{10}$"))
            {
                errors.Add("Số điện thoại phải gồm 10 chữ số");
            }

            if (!string.IsNullOrEmpty(model.Email) && !System.Text.RegularExpressions.Regex.IsMatch(model.Email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            {
                errors.Add("Email không hợp lệ");
            }

            if (!string.IsNullOrEmpty(model.IdentityCardNumber) && !System.Text.RegularExpressions.Regex.IsMatch(model.IdentityCardNumber, @"^\d{9}$|^\d{12}$"))
            {
                errors.Add("CMND/CCCD phải gồm 9 hoặc 12 số");
            }

            if (model.Birthdate.HasValue)
            {
                var age = DateTime.UtcNow.Year - model.Birthdate.Value.Year;
                if (age < 16)
                {
                    errors.Add("Người dùng phải từ 16 tuổi trở lên");
                }
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ", errors });
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(model.FullName))
            {
                account.FullName = model.FullName;
            }

            if (model.Gender.HasValue)
            {
                account.Gender = model.Gender;
            }

            if (model.Birthdate.HasValue)
            {
                account.Birthdate = model.Birthdate;
            }

            if (!string.IsNullOrEmpty(model.Address))
            {
                account.Address = model.Address;
            }

            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                account.PhoneNumber = model.PhoneNumber;
            }

            if (!string.IsNullOrEmpty(model.Email))
            {
                var emailExists = await _context.Accounts
                    .AnyAsync(a => a.Email == model.Email && a.Id != userId);
                if (emailExists)
                {
                    return BadRequest(new { message = "Email đã được sử dụng" });
                }
                account.Email = model.Email;
            }

            if (model.ResourceId.HasValue)
            {
                var resource = await _context.Resources.FindAsync(model.ResourceId.Value);
                if (resource == null)
                {
                    return BadRequest(new { message = "Resource không tồn tại" });
                }
                account.AvatarUrl = resource.LocalUrl;
            }

            if (!string.IsNullOrEmpty(model.IdentityCardNumber))
            {
                var idCardExists = await _context.Accounts
                    .AnyAsync(a => a.IdentityCardNumber == model.IdentityCardNumber && a.Id != userId);
                if (idCardExists)
                {
                    return BadRequest(new { message = "CMND/CCCD đã được sử dụng" });
                }
                account.IdentityCardNumber = model.IdentityCardNumber;
            }

            // Update metadata
            account.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Cập nhật hồ sơ thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật hồ sơ", error = ex.Message });
            }
        }

        [HttpGet]
       // [Authorize]
        public async Task<IActionResult> GetProfileById()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Không thể xác thực người dùng" });
            }

            var account = await _context.Accounts
                .Where(a => a.Id == userId && a.Status == "active")
                .Select(a => new
                {
                    a.FullName,
                    a.Gender,
                    a.Birthdate,
                    a.Address,
                    a.PhoneNumber,
                    a.Email,
                    a.AvatarUrl,
                    a.IdentityCardNumber,
                })
                .FirstOrDefaultAsync();

            if (account == null)
            {
                return NotFound(new { message = "Không tìm thấy hồ sơ người dùng" });
            }

            return Ok(account);
        }
    }
}