using AutoMapper;
using BU.Services.Interface;
using Castle.Core.Logging;
using Data.DatabaseContext;
using Google.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Model.Entity;
using Model.Entity.book;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace API.Controllers.ShareLinkControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class ShareLinksController(AppDbContext _context) : Controller
    {
        [Authorize]
        [HttpPost("GenerateLink")]
        public async Task<IActionResult> GenerateShareLink([FromBody] GenerateLinkRequest request)
        {
            try
            {
                var accountIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
                {
                    return Unauthorized("Token không hợp lệ");
                }
                var book = await _context.Books.FindAsync(request.BookId);
                if (book == null)
                {
                    return NotFound("Sách không tồn tại");
                }
                var existingLink = await _context.ShareLinks
                .FirstOrDefaultAsync(s =>
                    s.BookId == request.BookId &&
                    s.AccountId == accountId 
                );
                if (existingLink != null)
                {
                    return Ok(new { ShareUrl = existingLink.ShareUrl });
                }
                string title = Uri.EscapeDataString(book.Title ?? "");
                string description = Uri.EscapeDataString(book.Description ?? "");
                string thumbnail = Uri.EscapeDataString(book.Thumbnail ?? "");
                string encodedBookId = Convert.ToBase64String(BitConverter.GetBytes(request.BookId))
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .TrimEnd('=');
                string shareUrl = $"https://librarian.book-hive.space/sharelinks/{encodedBookId}" +
                            $"?title={title}" + "axy" +
                            $"&description={description}" + "byz" +
                            $"&thumbnail={thumbnail}";
                var shareLink = new ShareLink
                {
                    BookId = request.BookId,
                    AccountId = accountId,
                    ShareUrl = shareUrl,
                    EnablePlatform = Platform.Facebook,
                    CreatedAt = DateTime.UtcNow
                };
               _context.ShareLinks.Add(shareLink);
                await _context.SaveChangesAsync();
                return Ok(new { ShareUrl = shareUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server khi tạo liên kết");
            }
        }
       
        public class GenerateLinkRequest
        {
            public int BookId { get; set; }
        }

        [HttpGet("{shareId}")]
        public async Task<IActionResult> GetShareLinkDetails(string shareId)
        {
            var link = await (from shareLink in _context.ShareLinks
                              join book in _context.Books
                              on shareLink.BookId equals book.Id
                              where shareLink.ShareUrl.Contains(shareId)
                              select new
                              {
                                  ShareUrl = shareLink.ShareUrl,
                                  Title = book.Title,
                                  Thumnail = book.Thumbnail,
                                  Description = book.Description
                              })
                 .FirstOrDefaultAsync();
            if (link == null) {return NotFound(); }
            return Ok(link);
        }
    }
}