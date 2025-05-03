using System.Threading.Tasks;
using AutoMapper;
using BU.Models.DTO.Notification;
using Microsoft.AspNetCore.SignalR;

namespace BU.Hubs;

public class ReviewHub(IMapper mapper) : Hub
{
  public async Task ReceiveReview(int bookId, object reviewData)
  {
    await Clients.Group($"get-new-review-with-book-{bookId}").SendAsync("ReceiveReview", reviewData);
  }
  public async Task ReceiveMessage(int bookId, object reviewData)
  {
    await Clients.Group($"get-new-review-with-book-{bookId}").SendAsync("ReceiveMessage", reviewData);
  }

  public async Task JoinBookGroup(int bookId)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, $"get-new-review-with-book-{bookId}");
  }

  public async Task LeaveBookGroup(int bookId)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"get-new-review-with-book-{bookId}");
  }

  public async Task JoinGroup(string group) { await Groups.AddToGroupAsync(Context.ConnectionId, group); }
  public async Task LeaveGroup(string group) { await Groups.RemoveFromGroupAsync(Context.ConnectionId, group); }

  public async Task ReceiveNotification(NotificationDto notification)
  {
    await Clients.Caller.SendAsync("ReceiveNotification", notification);
  }
}
