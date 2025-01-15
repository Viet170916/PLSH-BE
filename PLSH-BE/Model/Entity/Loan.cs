namespace Model.Entity;

public class Loan
{
  public int Id { get; set; }
  public string? Note { get; set; }
  public int BorrowerId { get; set; }
  public int LibrarianId { get; set; }
  public DateTime BorrowingDate { get; set; } // ngày nhận dự kiến 
  public DateTime ReturnDate { get; set; } // hạn trả dự kiến
  public int AprovalStatus { get; set; } = 0; // trạng thái duyệt (duyệt, không duyệt, chờ duyệt)
}