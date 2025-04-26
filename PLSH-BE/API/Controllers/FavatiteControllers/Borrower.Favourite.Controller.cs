using API.Common;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Model.Entity;
using Common.Enums;
using Microsoft.AspNetCore.Authorization;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.Security.Claims;
using BU.Services.Interface;
using BU.Models.DTO.Notification;

namespace API.Controllers.FavatiteControllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class BorrowerFavoriteController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BorrowerFavoriteController> _logger;
        private readonly INotificationService _notificationService;

        public BorrowerFavoriteController(AppDbContext context, ILogger<BorrowerFavoriteController> logger, INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleFavorite(int bookId, string note = null)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int userId = int.Parse(userIdClaim);
                if (userId == 0)
                {
                    _logger.LogWarning("Unauthorized access attempt on ToggleFavorite.");
                    return Unauthorized("User not authenticated.");
                }

                var book = await _context.Books.FindAsync(bookId);
                if (book == null)
                {
                    return NotFound("Book not found");
                }

                var favorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.BorrowerId == userId && f.BookId == bookId);

                if (favorite == null)
                {
                    favorite = new Favorite
                    {
                        BorrowerId = userId,
                        BookId = bookId,
                        Status = FavoriteStatus.Favorite,
                        AddedDate = DateTime.UtcNow,
                        Note = note
                    };
                    _context.Favorites.Add(favorite);

                    // Gửi thông báo khi tạo mới favorite
                    await _notificationService.SendNotificationToUserAsync(
                        userId,
                        new NotificationDto
                        {
                            Title = "Sách yêu thích",
                            Content = $"Bạn đã thêm '{book.Title}' vào danh sách yêu thích",
                            Reference = "Favorite",
                            ReferenceId = bookId,
                            ReferenceData = new { BookId = bookId, BookTitle = book.Title }
                        });
                }
                else
                {
                    var previousStatus = favorite.Status;
                    favorite.Status = favorite.Status == FavoriteStatus.Favorite
                        ? FavoriteStatus.WantToRead
                        : FavoriteStatus.Favorite;
                    favorite.AddedDate = DateTime.UtcNow;
                    favorite.Note = note ?? favorite.Note;

                    // Gửi thông báo khi thay đổi trạng thái
                    if (previousStatus != favorite.Status)
                    {
                        var action = favorite.Status == FavoriteStatus.Favorite
                            ? "thêm vào"
                            : "bỏ khỏi";

                        await _notificationService.SendNotificationToUserAsync(
                            userId,
                            new NotificationDto
                            {
                                Title = "Thay đổi yêu thích",
                                Content = $"Bạn đã {action} danh sách yêu thích sách '{book.Title}'",
                                Reference = "Favorite",
                                ReferenceId = bookId,
                                ReferenceData = new { BookId = bookId, BookTitle = book.Title }
                            });
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { isFavorite = favorite.Status == FavoriteStatus.Favorite });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling favorite for BookId: {BookId}", bookId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetFavorites()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int userId = int.Parse(userIdClaim);
                if (userId == 0)
                {
                    _logger.LogWarning("Unauthorized access attempt on GetFavorites.");
                    return Unauthorized("User not authenticated.");
                }

                var favorites = await _context.Favorites
                    .Where(f => f.BorrowerId == userId && f.Status == FavoriteStatus.Favorite)
                    .Join(_context.Books,
                          f => f.BookId,
                          b => b.Id,
                          (f, b) => new
                          {
                              f.BookId,
                              BookTitle = b.Title ?? "Unknown Title",
                              f.Status,
                              f.Note,
                              f.AddedDate
                          })
                    .OrderByDescending(f => f.AddedDate)
                    .ToListAsync();

                return Ok(favorites);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting favorites");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // Check Favorite Status
        [HttpGet("check/{bookId}")]
        public async Task<IActionResult> CheckFavoriteStatus(int bookId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int userId = int.Parse(userIdClaim);
                if (userId == 0)
                {
                    _logger.LogWarning("Unauthorized access attempt on CheckFavoriteStatus.");
                    return Unauthorized("User not authenticated.");
                }

                var favorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.BorrowerId == userId && f.BookId == bookId);

                if (favorite == null || favorite.Status != FavoriteStatus.Favorite)
                {
                    return Ok(new { isFavorite = false });
                }

                return Ok(new { isFavorite = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking favorite status for BookId: {BookId}", bookId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("delete/{bookId}")]
        public async Task<IActionResult> DeleteFavorite(int bookId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int userId = int.Parse(userIdClaim);
                if (userId == 0)
                {
                    _logger.LogWarning("Unauthorized access attempt on DeleteFavorite.");
                    return Unauthorized("User not authenticated.");
                }

                var book = await _context.Books.FindAsync(bookId);
                if (book == null)
                {
                    return NotFound("Book not found");
                }

                var favorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.BorrowerId == userId && f.BookId == bookId);

                if (favorite == null)
                {
                    return NotFound("Favorite not found");
                }

                _context.Favorites.Remove(favorite);

                // Gửi thông báo khi xóa favorite
                await _notificationService.SendNotificationToUserAsync(
                    userId,
                    new NotificationDto
                    {
                        Title = "Xóa yêu thích",
                        Content = $"Bạn đã xóa '{book.Title}' khỏi danh sách yêu thích",
                        Reference = "Favorite",
                        ReferenceId = bookId,
                        ReferenceData = new { BookId = bookId, BookTitle = book.Title }
                    });

                await _context.SaveChangesAsync();
                return Ok(new { message = "Favorite removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting favorite for BookId: {BookId}", bookId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}