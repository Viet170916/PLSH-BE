using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using API.DTO;
using API.DTO.Book;
using BU.Models.DTO;
using BU.Models.DTO.Book;
using BU.Models.DTO.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book;
using Model.Entity.Notification;

namespace API.Controllers.ReviewControllers;

public partial class ReviewController
{
  [HttpPost("message/send")] public async Task<IActionResult> SendMessage([FromForm] MessageDto request)
  {
    if (string.IsNullOrWhiteSpace(request.Content))
    {
      return BadRequest(new BaseResponse<string>
      {
        message = "Nội dung tin nhắn không được để trống.", status = "error",
      });
    }

    var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
    var message = new Message
    {
      SenderId = accountId,
      RepliedId = request.RepliedId,
      ReviewId = request.ReviewId,
      Content = request.Content,
      ResourceId = request.ResourceId,
      CreatedDate = DateTime.UtcNow,
    };
    context.Messages.Add(message);
    await context.SaveChangesAsync();
    var messageDetail = await context.Messages
                                     .Include(m => m.Sender)
                                     .Include(m => m.Review)
                                     .ThenInclude(r => r.AccountSender)
                                     .Include(m => m.RepliedPerson)
                                     .FirstOrDefaultAsync(m => m.Id == message.Id);
    var messageDto = mapper.Map<MessageDto>(messageDetail, opt => { opt.Items["CurrentUserId"] = accountId; });
    var review = messageDetail?.Review;
    var notifications = new List<Notification>();
    var notificationReceivers = new List<(int receiverId, string content)>();
    if (message.RepliedId.HasValue && message.RepliedId != accountId)
    {
      notificationReceivers.Add((message.RepliedId.Value, "Bạn đã được phản hồi trong một đánh giá."));
    }

    if (review != null && review.AccountSenderId != accountId)
    {
      notificationReceivers.Add((review.AccountSenderId, "Đánh giá của bạn vừa nhận được một tin nhắn mới."));
    }

    foreach (var (receiverId, content) in notificationReceivers)
    {
      notifications.Add(new Notification
      {
        Title = "Tin nhắn mới",
        Content = content,
        Date = DateTime.UtcNow,
        Reference = "Message",
        ReferenceId = message.Id,
        AccountId = receiverId,
        IsRead = false,
      });
    }

    context.Notifications.AddRange(notifications);
    await context.SaveChangesAsync();
    foreach (var notification in notifications)
    {
      var notificationDto = mapper.Map<NotificationDto>(notification, opt =>
        opt.Items["ReferenceData"] = messageDto);
      await hubContext.Clients
                      .User(notification.AccountId.ToString())
                      .SendAsync("ReceiveNotification", notificationDto);
    }

    if (review != null)
    {
      await hubContext.Clients
                      .Group($"get-new-review-with-book-{review.BookId}")
                      .SendAsync("ReceiveMessage", messageDto);
    }

    return Ok(new BaseResponse<MessageDto>
    {
      message = "Gửi tin nhắn thành công.", data = messageDto, status = "success"
    });
  }

  [HttpGet("{reviewId}/messages")] public async Task<IActionResult> GetMessagesByReviewId(
    [FromRoute] int reviewId,
    [FromQuery] int page = 1,
    [FromQuery] int limit = 10
  )
  {
    var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
    var totalMessageCount = await context.Messages
                                         .Where(m => m.ReviewId == reviewId)
                                         .CountAsync();
    var totalPage = (int)Math.Ceiling((double)totalMessageCount / limit);
    var messages = await context.Messages
                                .Include(m => m.Sender)
                                .Include(m => m.Review)
                                .ThenInclude(r => r.AccountSender)
                                .Include(m => m.RepliedPerson)
                                .Where(m => m.ReviewId == reviewId)
                                .OrderByDescending(m => m.CreatedDate)
                                .Skip((page - 1) * limit)
                                .Take(limit)
                                .ToListAsync();
    if (messages.Count == 0)
    {
      return NotFound(new BaseResponse<string>
      {
        message = "Không tìm thấy tin nhắn nào cho đánh giá này.", status = "error"
      });
    }

    var messageDtos = mapper.Map<List<MessageDto>>(messages, opt => { opt.Items["CurrentUserId"] = accountId; });
    return Ok(new BaseResponse<List<MessageDto>>
    {
      message = "Lấy danh sách tin nhắn thành công.",
      status = "success",
      page = page,
      currenPage = page,
      limit = limit,
      pageCount = totalPage,
      count = totalMessageCount,
      data = messageDtos,
    });
  }
}
