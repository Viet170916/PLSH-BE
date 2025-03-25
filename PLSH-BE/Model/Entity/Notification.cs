using System.ComponentModel.DataAnnotations;

namespace Model.Entity;

public class Notification
{
  public int Id { get; set; }

  [MaxLength(100)]
  public string Title { get; set; }

  [MaxLength(255)]
  public string Content { get; set; }

  public DateTime Date { get; set; }
  public int Status { get; set; } = 0; //(chưa đọc, đã đọc)
  public int Reference { get; set; } = 0; //(loại thông báo cho chức năng gì)
  public int AccountId { get; set; }

  //public virtual AccountControllers AccountControllers { get; set; }
}