namespace Model.Entity;

public class Notification
{
  public int Id { get; set; }
  public string Title { get; set; }
  public string Content { get; set; }
  public DateTime Date { get; set; }
  public int Status { get; set; } = 0; //(chưa đọc, đã đọc)
  public int Reference { get; set; } = 0; //(loại thông báo cho chức năng gì)
  public int AccountId { get; set; }
}