using BU.Services.Interface;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Model.Entity;
using Model.Entity.Notification;

namespace API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class CardMemberController(AppDbContext context, ILogger<ManageAccountController> logger)
    : ControllerBase
  {
    [HttpPost("CreateCardMember")] public async Task<IActionResult> CreateCardMember([FromBody] int accountId)
    {
      try
      {
        var account = context.Accounts.FirstOrDefault(a => a.Id == accountId);
        if (account == null) { return NotFound("Tài khoản không tồn tại"); }

        if (account.CardMemberNumber != null) { return BadRequest("Tài khoản đã có thẻ thành viên"); }

        account.CardMemberStatus = 1;
        account.CardMemberExpiredDate = DateTime.Now.AddYears(1);
        account.UpdatedAt = DateTime.Now;
        context.Accounts.Update(account);
        await context.SaveChangesAsync();
        logger.LogInformation($"Tạo mới thẻ thành viên cho tài khoản {accountId} thành công");
        return Ok(new { Message = "Tạo mới thẻ thành viên thành công", CardNumber = account.CardMemberNumber });
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Lỗi trong quá trình tạo thẻ thành viên");
        return StatusCode(StatusCodes.Status500InternalServerError, "Có lỗi xảy ra trong quá trình xử lý");
      }
    }

    [HttpPost("RequestNewCard")] public async Task<IActionResult> RequestNewCard([FromBody] int accountId)
    {
      try
      {
        var account = context.Accounts.FirstOrDefault(a => a.Id == accountId);
        if (account == null) { return NotFound("Tài khoản không tồn tại"); }

        var notification = new Notification
        {
          Title = "Yêu cầu cấp lại thẻ",
          Content = $"Yêu cầu cấp lại thẻ thành viên cho tài khoản {accountId}",
          Date = DateTime.Now,
          AccountId = accountId
        };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
        return Ok("Yêu cầu cấp lại thẻ thành viên đã được gửi đi");
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Lỗi trong quá trình gửi yêu cầu cấp lại thẻ");
        return StatusCode(StatusCodes.Status500InternalServerError, "Có lỗi xảy ra trong quá trình xử lý");
      }
    }

    //Chấp nhận yêu cầu tạo thẻ thành viên mới
    [HttpPost("ApproveCardRequest")] public async Task<IActionResult> ApproveCardRequest([FromBody] int notificationId)
    {
      try
      {
        var notification = context.Notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification == null || notification.Status != 0)
        {
          return NotFound("Yêu cầu không hợp lệ hoặc đã được xử lý");
        }

        var account = context.Accounts.FirstOrDefault(a => a.Id == notification.AccountId);
        if (account == null) { return NotFound("Tài khoản không tồn tại"); }

        account.CardMemberStatus = 1; // 1: Hoạt động
        account.CardMemberExpiredDate = DateTime.Now.AddYears(1);
        account.UpdatedAt = DateTime.Now;
        notification.Status = 1; // 1: Đã phê duyệt
        context.Accounts.Update(account);
        context.Notifications.Update(notification);
        await context.SaveChangesAsync();
        return Ok("Yêu cầu cấp lại thẻ đã được phê duyệt và thẻ mới đã được cấp");
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Lỗi trong quá trình phê duyệt yêu cầu cấp lại thẻ");
        return StatusCode(StatusCodes.Status500InternalServerError, "Có lỗi xảy ra trong quá trình xử lý");
      }
    }

    //Từ chối làm thẻ thành viên
    [HttpPost("RejectCardRequest")] public async Task<IActionResult> RejectCardRequest([FromBody] int notificationId)
    {
      try
      {
        var notification = context.Notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification == null || notification.Status != 0)
        {
          return NotFound("Yêu cầu không hợp lệ hoặc đã được xử lý");
        }

        notification.Status = 2; // 2: Từ chối
        context.Notifications.Update(notification);
        await context.SaveChangesAsync();
        return Ok("Yêu cầu cấp lại thẻ đã bị từ chối");
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Lỗi trong quá trình từ chối yêu cầu cấp lại thẻ");
        return StatusCode(StatusCodes.Status500InternalServerError, "Có lỗi xảy ra trong quá trình xử lý");
      }
    }

    [HttpPost("RenewCardMember")] public async Task<IActionResult> RenewCardMember([FromBody] int accountId)
    {
      try
      {
        var account = context.Accounts.FirstOrDefault(a => a.Id == accountId);
        if (account == null) { return NotFound("Tài khoản không tồn tại"); }

        if (account.CardMemberExpiredDate <= DateTime.Now)
        {
          account.CardMemberExpiredDate = DateTime.Now.AddYears(1);
          account.UpdatedAt = DateTime.Now;
          context.Accounts.Update(account);
          await context.SaveChangesAsync();
          return Ok("Gia hạn thẻ thành viên thành công");
        }

        return BadRequest("Thẻ thành viên chưa đến thời hạn gia hạn");
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Lỗi trong quá trình gia hạn thẻ thành viên");
        return StatusCode(StatusCodes.Status500InternalServerError, "Có lỗi xảy ra trong quá trình xử lý");
      }
    }

    [HttpPost("SendRenewNotification")] public async Task<IActionResult> SendRenewNotification()
    {
      try
      {
        var accounts = context.Accounts.Where(a =>
                                a.CardMemberExpiredDate != null && a.CardMemberExpiredDate <= DateTime.Now.AddDays(7) &&
                                a.CardMemberExpiredDate > DateTime.Now)
                              .ToList();
        foreach (var account in accounts)
        {
          var notification = new Notification
          {
            Title = "Thông báo gia hạn thẻ thành viên",
            Content =
              $"Thẻ thành viên của bạn sắp hết hạn vào ngày {account.CardMemberExpiredDate:dd/MM/yyyy}. Vui lòng gia hạn để tiếp tục sử dụng dịch vụ.",
            Date = DateTime.Now,
            Status = 1,
            AccountId = account.Id
          };
          context.Notifications.Add(notification);
        }

        await context.SaveChangesAsync();
        return Ok("Thông báo gia hạn đã được gửi đi thành công");
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Lỗi trong quá trình gửi thông báo gia hạn thẻ thành viên");
        return StatusCode(StatusCodes.Status500InternalServerError, "Có lỗi xảy ra trong quá trình xử lý");
      }
    }
  }
}
