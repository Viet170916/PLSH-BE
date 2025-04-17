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

namespace API.Controllers.FavatiteControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BorrowerFavoriteController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BorrowerFavoriteController> _logger;

        public BorrowerFavoriteController(AppDbContext context, ILogger<BorrowerFavoriteController> logger)
        {
            _context = context;
            _logger = logger;
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

                var favorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.AccountId == userId && f.BookId == bookId);

                if (favorite == null)
                {
                    favorite = new Favorite
                    {
                        AccountId = userId,
                        BookId = bookId,
                        Status = FavoriteStatus.Favorite,
                        AddedDate = DateTime.UtcNow,
                        Note = note
                    };
                    _context.Favorites.Add(favorite);
                }
                else
                {                 
                    favorite.Status = favorite.Status == FavoriteStatus.Favorite
                        ? FavoriteStatus.WantToRead
                        : FavoriteStatus.Favorite;
                    favorite.AddedDate = DateTime.UtcNow;
                    favorite.Note = note ?? favorite.Note; 
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
                if (userId == 0)                {
                    _logger.LogWarning("Unauthorized access attempt on GetFavorites.");
                    return Unauthorized("User not authenticated.");
                }
                var favorites = await _context.Favorites
                    .Where(f => f.AccountId == userId && f.Status == FavoriteStatus.Favorite)
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
                return Ok(favorites);            }
            catch (Exception ex)
            {            
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
                    .FirstOrDefaultAsync(f => f.AccountId == userId && f.BookId == bookId);

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
    }
}