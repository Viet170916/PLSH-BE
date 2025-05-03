using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using AutoMapper;
using BU.Models.DTO;
using BU.Models.DTO.Book;
using BU.Models.DTO.Notification;
using BU.Services.Interface;
using Common.Enums;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Model.Entity;
using Model.Entity.Favorite;

namespace API.Controllers.FavatiteControllers;

[Route("api/v1/favorite")]
[ApiController]
public class BorrowerFavoriteController(
  AppDbContext context,
  ILogger<BorrowerFavoriteController> logger,
  INotificationService notificationService,
  IMapper mapper
) : ControllerBase
{
  public class ToggleRequest
  {
    public int BookId { get; set; }
  }

  [Authorize] [HttpPost("toggle")] public async Task<IActionResult> ToggleFavorite(ToggleRequest request)
  {
    // try
    // {
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userId = int.Parse(userIdClaim ?? "0");
    var user = await context.Accounts.FindAsync(userId);
    if (userId == 0)
    {
      logger.LogWarning("Người dùng chưa xác thực");
      return Unauthorized(new BaseResponse<object> { Message = "Người dùng chưa xác thực.", Data = null });
    }

    var book = await context.Books.FindAsync(request.BookId);
    if (book == null) { return NotFound(new BaseResponse<object> { Message = "Không tìm thấy sách.", Data = null }); }

    var favorite =
      await context.Favorites.FirstOrDefaultAsync(f => f.BorrowerId == userId && f.BookId == request.BookId);
    var libs = context.Accounts.Include(l => l.Role)
                      .Where(a => a.Role.Name == "librarian")
                      .Select(a => a.Id)
                      .ToList();
    if (favorite == null)
    {
      favorite = new Favorite
      {
        BorrowerId = userId, BookId = request.BookId, Status = FavoriteStatus.Favorite, AddedDate = DateTime.UtcNow,
      };
      context.Favorites.Add(favorite);
      await context.SaveChangesAsync();
      await notificationService.SendNotificationToUsersAsync(libs,
        new NotificationDto
        {
          Title = "Sách yêu thích",
          Content = $"Người dùng {user?.FullName ?? "--"} đã thêm '{book.Title}' vào danh sách yêu thích.",
          Reference = "Book",
          ReferenceId = request.BookId,
          ReferenceData = new { BookId = request.BookId, BookTitle = book.Title, },
        });
    }
    else
    {
      var previousStatus = favorite.Status;
      favorite.Status = favorite.Status == FavoriteStatus.Favorite ?
        FavoriteStatus.UnFavorite :
        FavoriteStatus.Favorite;
      favorite.AddedDate = DateTime.UtcNow;
      // favorite.Note = note ?? favorite.Note;
      if (previousStatus == favorite.Status)
        return Ok(new BaseResponse<bool>
        {
          Message = "Cập nhật yêu thích thành công.", Data = favorite.Status == FavoriteStatus.Favorite,
        });
      var action = favorite.Status == FavoriteStatus.Favorite ? "thêm vào" : "bỏ khỏi";
      await notificationService.SendNotificationToUsersAsync(libs,
        new NotificationDto
        {
          Title = "Cập nhật yêu thích",
          Content = $"Người dùng {user?.FullName ?? "--"} đã {action} danh sách yêu thích sách '{book.Title}'.",
          Reference = "Favorite",
          ReferenceId = request.BookId,
          ReferenceData = new { BookId = request.BookId, BookTitle = book.Title }
        });
    }

    return Ok(new BaseResponse<bool>
    {
      Message = "Cập nhật yêu thích thành công.", Data = favorite.Status == FavoriteStatus.Favorite,
    });
  }
  // catch (Exception ex)
  // {
  //   logger.LogError(ex, "Lỗi khi cập nhật yêu thích BookId: {BookId}", request.BookId);
  //   return StatusCode(500, new BaseResponse<object> { Message = "Đã xảy ra lỗi khi xử lý yêu cầu.", Data = null });
  // }

  [HttpGet("list")] [Authorize] public async Task<IActionResult> GetFavorites()
  {
    try
    {
      var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var userId = int.Parse(userIdClaim ?? "0");
      if (userId == 0)
      {
        logger.LogWarning("Người dùng chưa xác thực");
        return Unauthorized(new BaseResponse<object> { Message = "Người dùng chưa xác thực.", Data = null });
      }

      var favorites = await context.Favorites
                                   .Where(f => f.BorrowerId == userId && f.Status == FavoriteStatus.Favorite)
                                   .Join(context.Books,
                                     f => f.BookId,
                                     b => b.Id,
                                     (f, b) => new
                                     {
                                       f.BookId,
                                       BookTitle = b.Title ?? "Không có tiêu đề",
                                       book = b,
                                       f.Status,
                                       f.Note,
                                       f.AddedDate
                                     })
                                   .OrderByDescending(f => f.AddedDate)
                                   .Select(f => f.book)
                                   .ToListAsync();
      var bookDtos = mapper.Map<List<BookNewDto>>(favorites);
      foreach (var bookNewDto in bookDtos) { bookNewDto.IsFavorite = true; }

      return Ok(new BaseResponse<List<BookNewDto>>
      {
        Message = "Lấy danh sách yêu thích thành công.", Data = bookDtos, Count = favorites.Count
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Lỗi khi lấy danh sách yêu thích");
      return StatusCode(500, new BaseResponse<object> { Message = "Đã xảy ra lỗi khi xử lý yêu cầu.", Data = null });
    }
  }

  [HttpGet("check/{bookId}")] [Authorize]
  public async Task<IActionResult> CheckFavoriteStatus(int bookId)
  {
    try
    {
      var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var userId = int.Parse(userIdClaim ?? "0");
      if (userId == 0)
      {
        logger.LogWarning("Người dùng chưa xác thực khi gọi CheckFavoriteStatus");
        return Unauthorized(new BaseResponse<object> { Message = "Người dùng chưa xác thực.", Data = null });
      }

      var favorite = await context.Favorites
                                  .FirstOrDefaultAsync(f => f.BorrowerId == userId && f.BookId == bookId);
      var isFavorite = favorite is { Status: FavoriteStatus.Favorite };
      return Ok(new BaseResponse<object>
      {
        Message = "Kiểm tra trạng thái yêu thích thành công.", Data = new { isFavorite }
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Lỗi khi kiểm tra yêu thích BookId: {BookId}", bookId);
      return StatusCode(500, new BaseResponse<object> { Message = "Đã xảy ra lỗi khi xử lý yêu cầu.", Data = null });
    }
  }

  [HttpDelete("delete/{bookId}")] [Authorize]
  public async Task<IActionResult> DeleteFavorite(int bookId)
  {
    try
    {
      var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var userId = int.Parse(userIdClaim ?? "0");
      if (userId == 0)
      {
        logger.LogWarning("Người dùng chưa xác thực khi gọi DeleteFavorite");
        return Unauthorized(new BaseResponse<object> { Message = "Người dùng chưa xác thực.", Data = null });
      }

      var book = await context.Books.FindAsync(bookId);
      if (book == null) { return NotFound(new BaseResponse<object> { Message = "Không tìm thấy sách.", Data = null }); }

      var favorite = await context.Favorites
                                  .FirstOrDefaultAsync(f => f.BorrowerId == userId && f.BookId == bookId);
      if (favorite == null)
      {
        return NotFound(new BaseResponse<object> { Message = "Không tìm thấy mục yêu thích.", Data = null });
      }

      context.Favorites.Remove(favorite);
      await notificationService.SendNotificationToUserAsync(userId,
        new NotificationDto
        {
          Title = "Xóa yêu thích",
          Content = $"Bạn đã xóa sách '{book.Title}' khỏi danh sách yêu thích.",
          Reference = "Favorite",
          ReferenceId = bookId,
          ReferenceData = new { BookId = bookId, BookTitle = book.Title }
        });
      await context.SaveChangesAsync();
      return Ok(new BaseResponse<object> { Message = "Xóa yêu thích thành công.", Data = null });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Lỗi khi xóa yêu thích BookId: {BookId}", bookId);
      return StatusCode(500, new BaseResponse<object> { Message = "Đã xảy ra lỗi khi xử lý yêu cầu.", Data = null });
    }
  }
}
