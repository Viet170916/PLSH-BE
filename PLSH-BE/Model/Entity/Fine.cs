using System.ComponentModel.DataAnnotations;

namespace Model.Entity;

public class Fine
{
  public int Id { get; set; }
  public DateTime FineDate { get; set; }
  public bool IsFined { get; set; } = false;
  public int? FineType { get; set; } //(trả muộn/hỏng sách/mất sách)

  [MaxLength(255)]
  public string? Note { get; set; }

  public int? BookBorrowingId { get; set; }
  public int BorrowerId { get; set; }
  public int Status { get; set; } //(chờ, đã huỷ, thất bại, hoàn thành)
  public int? TransactionId { get; set; }
}