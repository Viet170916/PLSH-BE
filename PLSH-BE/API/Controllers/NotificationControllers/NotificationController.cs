using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
using BU.Models.DTO;
using BU.Models.DTO.Notification;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.NotificationControllers;

[ApiController]
[Route("api/v1/notification")]
public partial class NotificationController(AppDbContext context, IMapper mapper) : Controller
{
  [Authorize] [HttpPost("read/{id}")] public async Task<IActionResult> Read([FromRoute] int id)
  {
    var notification = await context.Notifications.FindAsync(id);
    if (notification == null) { return NotFound(new { message = "Notification not found.", }); }

    if (notification.IsRead) { return Ok(new { message = "Notification already read.", }); }

    notification.IsRead = true;
    notification.ReadDate = DateTime.UtcNow;
    await context.SaveChangesAsync();
    return Ok(new { message = "Notification marked as read.", });
  }

  [Authorize] [HttpGet] public async Task<IActionResult>
    GetNotifications([FromQuery] int page = 1, [FromQuery] int limit = 10)
  {
    var accountId = int.Parse(User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    if (accountId == 0)
    {
      return Unauthorized(new BaseResponse<object>
      {
        message = "Không thể xác thực người dùng.", status = "error", data = null
      });
    }

    var totalNotifications = await context.Notifications
                                          .Include(n => n.Account)
                                          .Where(n => n.AccountId == accountId)
                                          .CountAsync();
    var unreadCount = await context.Notifications
                                   .Where(n => n.AccountId == accountId && !n.IsRead)
                                   .CountAsync();
    var notifications = await context.Notifications
                                     .Where(n => n.AccountId == accountId)
                                     .OrderByDescending(n=>!n.IsRead)
                                     .ThenByDescending(n => n.Date)
                                     .Skip((page - 1) * limit)
                                     .Take(limit)
                                     .ToListAsync();
    var notificationDtos =
      mapper.Map<List<NotificationDto>>(notifications, opts => { opts.Items["ReferenceData"] = null; });
    var response = new
    {
      message = "Lấy danh sách thông báo thành công.",
      status = "success",
      data = notificationDtos,
      unreadCount,
      count = totalNotifications,
      page,
      limit,
      currenPage = page,
      pageCount = (int)Math.Ceiling(totalNotifications / (double)limit)
    };
    return Ok(response);
  }
}
