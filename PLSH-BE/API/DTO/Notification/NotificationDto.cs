using API.DTO.Account.AccountDTO;

namespace API.DTO.Notification;

public class NotificationDto
{
  public int Id { get; set; }
  public string Title { get; set; } = "Thông báo";
  public string? Content { get; set; } = "Bạn nhận được thông báo";
  public DateTime Date { get; set; }
  public int Status { get; set; } = 0;
  public bool IsRead { get; set; } = false;
  public string? Reference { get; set; } = "Review";
  public int? ReferenceId { get; set; }
  public object? ReferenceData { get; set; }
  public int AccountId { get; set; }
  public AccountGDto Account { get; set; }
}
