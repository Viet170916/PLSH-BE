using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;
using Model.Entity.User;

namespace Model.Entity.Borrow;

public class Loan
{
  public int Id { get; set; }

  [MaxLength(255)]
  public string? Note { get; set; }

  public int BorrowerId { get; set; }
  public int? LibrarianId { get; set; }
  public DateTime BorrowingDate { get; set; }
  public DateTime? ReturnDate { get; set; }
  public bool IsCart { get; set; } = false;
  public string? AprovalStatus { get; set; }

  // public int ExtensionCount { get; set; } = 0;
  public ICollection<BookBorrowing> BookBorrowings { get; set; } = new List<BookBorrowing>();

  [ForeignKey("BorrowerId")]
  public Account? Borrower { get; set; }

  [ForeignKey("LibrarianId")]
  public Account? Librarian { get; set; }
}
