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
using Model.Entity.book;

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

        // File: Controllers/FavoriteController.cs

        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleFavorite(int bookId)
        {
            int borrowerId = 1; // TODO: Replace with actual user context later

            try
            {
                var borrowerExists = await _context.Borrowers.AnyAsync(b => b.Id == borrowerId);
                if (!borrowerExists)
                {
                    _logger.LogWarning("Borrower {BorrowerId} not found.", borrowerId);
                    return NotFound("Borrower not found.");
                }

                var book = await _context.Books.FindAsync(bookId);
                if (book == null)
                {
                    _logger.LogWarning("Book {BookId} not found.", bookId);
                    return NotFound("Book not found.");
                }

                var favorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.BorrowerId == borrowerId && f.BookId == bookId);

                if (favorite == null)
                {
                    favorite = new Favorite
                    {
                        BorrowerId = borrowerId,
                        BookId = bookId,
                        Status = FavoriteStatus.Favorite,
                        AddedDate = DateTime.UtcNow
                    };

                    _context.Favorites.Add(favorite);
                    await _context.SaveChangesAsync();

                    await NotifyUserFavoriteAdded(borrowerId, book);
                }
                else
                {
                    var previousStatus = favorite.Status;
                    favorite.Status = previousStatus == FavoriteStatus.Favorite
                        ? FavoriteStatus.WantToRead
                        : FavoriteStatus.Favorite;
                    favorite.AddedDate = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    if (previousStatus != favorite.Status)
                    {
                        await NotifyUserFavoriteToggled(borrowerId, book, favorite.Status);
                    }
                }

                return Ok(new { isFavorite = favorite.Status == FavoriteStatus.Favorite });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling favorite for BookId: {BookId}, BorrowerId: {BorrowerId}", bookId, borrowerId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        private async Task NotifyUserFavoriteAdded(int borrowerId, Book book)
        {
            await _notificationService.SendNotificationToUserAsync(
                borrowerId,
                new NotificationDto
                {
                    Title = "Sách yêu thích",
                    Content = $"Bạn đã thêm '{book.Title}' vào danh sách yêu thích.",
                    Reference = "Favorite",
                    ReferenceId = book.Id,
                    ReferenceData = new { BookId = book.Id, BookTitle = book.Title }
                });

            var librarianIds = await _context.Accounts
                .Where(a => a.Role.Name == "Librarian")
                .Select(a => a.Id)
                .ToListAsync();

            var librarianNotifications = librarianIds.Select(librarianId =>
                _notificationService.SendNotificationToUserAsync(
                    librarianId,
                    new NotificationDto
                    {
                        Title = "Sách được yêu thích",
                        Content = $"Sách '{book.Title}' vừa được thêm vào danh sách yêu thích.",
                        Reference = "Favorite",
                        ReferenceId = book.Id,
                        ReferenceData = new { BookId = book.Id, BookTitle = book.Title }
                    }));

            await Task.WhenAll(librarianNotifications);
        }

        private async Task NotifyUserFavoriteToggled(int borrowerId, Book book, FavoriteStatus status)
        {
            string action = status == FavoriteStatus.Favorite ? "thêm vào" : "bỏ khỏi";

            await _notificationService.SendNotificationToUserAsync(
                borrowerId,
                new NotificationDto
                {
                    Title = "Thay đổi yêu thích",
                    Content = $"Bạn đã {action} danh sách yêu thích sách '{book.Title}'",
                    Reference = "Favorite",
                    ReferenceId = book.Id,
                    ReferenceData = new { BookId = book.Id, BookTitle = book.Title }
                });
        }



        [HttpGet("list")]
        [Authorize]
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
                var borrower = await _context.Borrowers
                .FirstOrDefaultAsync(b => b.AccountId == userId);
                if (borrower == null)
                {
                    return NotFound("Borrower not found");
                }
                var favorites = await _context.Favorites
                    .Where(f => f.BorrowerId == borrower.Id && f.Status == FavoriteStatus.Favorite)
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

        [HttpGet("check/{bookId}")]
        [Authorize]
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
                var borrower = await _context.Borrowers
                .FirstOrDefaultAsync(b => b.AccountId == userId);
                if (borrower == null)
                {
                    return NotFound("Borrower not found");
                }
                var favorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.BorrowerId == borrower.Id && f.BookId == bookId);

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
        [Authorize]
        public async Task<IActionResult> DeleteFavorite(int bookId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Unauthorized access attempt on DeleteFavorite.");
                    return Unauthorized("User not authenticated.");
                }

                var book = await _context.Books.FindAsync(bookId);
                if (book == null)
                {
                    return NotFound("Book not found");
                }

                // Tìm borrower theo userId
                var borrower = await _context.Borrowers
                    .FirstOrDefaultAsync(b => b.AccountId == userId);

                if (borrower == null)
                {
                    return NotFound("Borrower not found");
                }

                var favorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.BorrowerId == borrower.Id && f.BookId == bookId);

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

        [HttpGet("favorite-books")]
        public async Task<IActionResult> GetFavoriteBooks()
        {
            int borrowerId = 1; // Mặc định borrowerId = 1 như yêu cầu

            try
            {
                var favoriteBooks = await _context.Favorites
                    .Where(f => f.BorrowerId == borrowerId && f.Status == FavoriteStatus.Favorite)
                    .Join(_context.Books,
                        f => f.BookId,
                        b => b.Id,
                        (f, b) => new
                        {
                            BookId = b.Id,
                            Title = b.Title,
                            Thumbnail = b.Thumbnail,
                            Rating = b.Rating,
                            Description = b.Description,
                            AddedDate = f.AddedDate,
                            Authors = b.Authors.Select(a => new
                            {
                                Id = a.Id,
                                FullName = a.FullName
                            }).ToList()
                        })
                    .OrderByDescending(f => f.AddedDate)
                    .ToListAsync();

                if (!favoriteBooks.Any())
                {
                    return NotFound("No favorite books found for this borrower.");
                }

                return Ok(favoriteBooks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting favorite books for BorrowerId: {BorrowerId}", borrowerId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("stats/{bookId}")]
        public async Task<IActionResult> GetBookStats(int bookId)
        {
            try
            {
                var book = await _context.Books
               .Include(b => b.Authors)
               .FirstOrDefaultAsync(b => b.Id == bookId);
                if (book == null)
                {
                    return NotFound("Book not found");
                }
                var favoriteCount = await _context.Favorites
                    .CountAsync(f => f.BookId == bookId && f.Status == FavoriteStatus.Favorite);
                var commentCount = await _context.Reviews
                    .CountAsync(c => c.BookId == bookId);
                var result = new
                {
                    BookId = bookId,
                    BookTitle = book.Title,
                    AuthorNames = book.Authors != null
                    ? string.Join(", ", book.Authors.Select(a => a.FullName))
                    : null,
                    BookThumbnail = book.Thumbnail,
                    BookRating = book.Rating,
                    BookDescription = book.Description,
                    FavoriteCount = favoriteCount,
                    CommentCount = commentCount,
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting stats for BookId: {BookId}", bookId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}