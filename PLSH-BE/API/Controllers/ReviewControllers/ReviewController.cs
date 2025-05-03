using System.Collections.Generic;
using System.Security.Claims;
using API.Common;
using API.DTO;
using AutoMapper;
using BU.Hubs;
using BU.Models.DTO;
using BU.Models.DTO.Book;
using BU.Models.DTO.Notification;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using Model.Entity.book;
using Model.Entity.Notification;

namespace API.Controllers.ReviewControllers;

[Authorize]
[Route("api/v1/review")]
[ApiController]
public partial class ReviewController(
  AppDbContext context,
  IMapper mapper,
  IHubContext<ReviewHub> hubContext,
  GoogleCloudStorageHelper cloudStorageHelper
)
  : Controller
{
  public class ReviewResponse : BaseResponse<List<ReviewDto>>
  {
    public bool? isAccountHasReviewForThisBook { get; set; }
  }

  [HttpGet("book/{bookId}")] public async Task<IActionResult> GetReviewsByBookId(
    [FromRoute] int bookId,
    [FromQuery] int? senderId,
    [FromQuery] int page = 1,
    [FromQuery] int limit = 10
  )
  {
    var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
    var currentAccountId = senderId ?? accountId;
    var totalReviewCount = await context.Reviews
                                        .Where(r => r.BookId == bookId)
                                        .CountAsync();
    var totalPage = (int)Math.Ceiling((double)totalReviewCount / limit);
    var reviews = await context.Reviews
                               .Include(r => r.Resource)
                               .Include(r => r.AccountSender)
                               .Where(r => r.BookId == bookId)
                               .OrderByDescending(r => r.AccountSenderId == currentAccountId)
                               .ThenByDescending(r => r.UpdatedDate)
                               .ThenByDescending(r => r.CreatedDate)
                               .Skip((page - 1) * limit)
                               .Take(limit)
                               .ToListAsync();
    if (reviews == null || reviews.Count == 0)
    {
      return NotFound(new BaseResponse<string> { Message = "No reviews found for this book.", Status = "error" });
    }

    var reviewDtos = mapper.Map<List<ReviewDto>>(reviews, opt => { opt.Items["CurrentUserId"] = accountId; });
    return Ok(new
      ReviewResponse()
      {
        Message = "Reviews retrieved successfully.",
        Status = "success",
        isAccountHasReviewForThisBook = await context.Reviews
                                                     .AnyAsync(
                                                       r => r.BookId == bookId && r.AccountSenderId == accountId),
        Data = reviewDtos,
        Page = page,
        Limit = limit,
        PageCount = totalPage,
        Count = totalReviewCount
      });
  }

  [HttpPost("send")] public async Task<IActionResult> PostReview([FromForm] ReviewDto request)
  {
    if (request.BookId is null)
    {
      return BadRequest(new BaseResponse<string> { Message = "Không tìm thấy sách, id trống", Status = "error" });
    }

    var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
    if (string.IsNullOrWhiteSpace(request.Content))
    {
      return BadRequest(new BaseResponse<string>
      {
        Message = "Nội dung đánh giá không được để trống.", Status = "error"
      });
    }

    var review = new Review
    {
      AccountSenderId = accountId,
      BookId = (int)request.BookId,
      Rating = (float)request.Rating,
      Content = request.Content,
      CreatedDate = DateTime.UtcNow,
      UpdatedDate = DateTime.UtcNow
    };
    if (request.ResourceFile is not null)
    {
      var url = await cloudStorageHelper.UploadFileAsync(request.ResourceFile.OpenReadStream(),
        request.ResourceFile.FileName,
        request.ResourceFile.ContentType);
      review.Resource = new Resource
      {
        LocalUrl = url,
        FileType = request.ResourceFile.ContentType,
        Type = "image",
        Name = request.ResourceFile.FileName,
        SizeByte = request.ResourceFile.Length,
      };
    }

    context.Reviews.Add(review);
    await context.SaveChangesAsync();
    var reviewWithDetails = await context.Reviews
                                         .Include(r => r.AccountSender)
                                         .Include(r => r.Book)
                                         .FirstOrDefaultAsync(r => r.Id == review.Id);
    var reviewDto = mapper.Map<ReviewDto>(reviewWithDetails, opt => { opt.Items["CurrentUserId"] = accountId; });
    var librarians = await context.Accounts
                                  .Where(a => a.Role.Name == "librarian")
                                  .ToListAsync();
    var notifications = librarians.Select(lib => new Notification
                                  {
                                    Title = "Đánh giá mới",
                                    Content = $"Có một đánh giá mới cho sách \"{reviewWithDetails?.Book.Title}\".",
                                    Date = DateTime.UtcNow,
                                    Reference = "Review",
                                    ReferenceId = review.Id,
                                    AccountId = lib.Id,
                                    IsRead = false,
                                  })
                                  .ToList();
    context.Notifications.AddRange(notifications);
    await context.SaveChangesAsync();
    var notificationDtos = notifications.Select(n =>
                                          mapper.Map<NotificationDto>(n, opt => opt.Items["ReferenceData"] = reviewDto))
                                        .ToList();
    foreach (var librarian in librarians)
    {
      var userNotification = notificationDtos.FirstOrDefault(n => n.AccountId == librarian.Id);
      await hubContext.Clients.User(librarian.Id.ToString())
                      .SendAsync("ReceiveNotification", userNotification);
    }

    await hubContext.Clients.Group($"get-new-review-with-book-{review.BookId}")
                    .SendAsync("ReceiveReview", reviewDto);
    return Ok(new BaseResponse<ReviewDto>
    {
      Message = "Gửi đánh giá thành công và đã thông báo đến thủ thư.", Data = reviewDto, Status = "success"
    });
  }
}
