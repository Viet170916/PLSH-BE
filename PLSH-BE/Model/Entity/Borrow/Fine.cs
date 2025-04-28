using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Model.Entity.User;

namespace Model.Entity.Borrow;

public class Fine
{
  public int Id { get; set; }
  public DateTime FineDate { get; set; }
  public bool IsFined { get; set; } = false;
  public int? FineType { get; set; } //(trả muộn/hỏng sách/mất sách)

  [MaxLength(255)]
  public string? Note { get; set; }

  public int? BookBorrowingId { get; set; }

  [ForeignKey("BookBorrowingId")]
  public BookBorrowing? BookBorrowing { get; set; }

  public int BorrowerId { get; set; }

  [ForeignKey("BorrowerId")]
  public Account Borrower { get; set; }

  public string Status { get; set; } = "pending"; //(chờ, đã huỷ, thất bại, hoàn thành)

  public int? TransactionId { get; set; }

  [ForeignKey("TransactionId")]
  public Transaction? Transaction { get; set; }
}
