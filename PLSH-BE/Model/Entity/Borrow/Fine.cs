using Model.Entity.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Model.Entity.User;

namespace Model.Entity.Borrow;

public class Fine
{
  public int Id { get; set; }
  public DateTime FineDate { get; set; }
  public bool IsFined { get; set; } = false;
  public int? FineType { get; set; }
  public double FineByDate = 5000;

  public double? Amount { get; set; }

  [MaxLength(255)]
  public string? Note { get; set; }

  public int? BookBorrowingId { get; set; }

  [ForeignKey("BookBorrowingId")]
  public BookBorrowing? BookBorrowing { get; set; }

  public int BorrowerId { get; set; }

  [ForeignKey("BorrowerId")]
  public Account Borrower { get; set; }

  public string Status { get; set; } = "pending";

  public int? TransactionId { get; set; }

  [ForeignKey("TransactionId")]
  public Transaction? Transaction { get; set; }
}
