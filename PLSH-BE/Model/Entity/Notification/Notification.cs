using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Model.Entity.User;

namespace Model.Entity.Notification;

public class Notification
{
  public int Id { get; set; }

  [MaxLength(100)]
  public string Title { get; set; } = "Thông báo";

  [MaxLength(255)]
  public string? Content { get; set; } = "Bạn nhận được thông báo";

  public DateTime Date { get; set; }
  public int Status { get; set; } = 0;
  public bool IsRead { get; set; } = false;

  [MaxLength(50)]
  public string Reference { get; set; } = "Review";

  public int ReferenceId { get; set; }
  public int AccountId { get; set; }

  [ForeignKey("AccountId")]
  [Required]
  public Account Account { get; set; }
}
