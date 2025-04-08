using System.Collections.Generic;
using System.Threading.Tasks;
using BU.Models.DTO.Notification;

namespace BU.Services.Interface;

public interface INotificationService
{
  Task SendNotificationToUserAsync(int userId, NotificationDto notificationDto);
  Task SendNotificationToUsersAsync(IEnumerable<int> userIds, NotificationDto notificationDto);
  Task SendNotificationToGroupAsync(string groupName, NotificationDto notificationDto);
}
