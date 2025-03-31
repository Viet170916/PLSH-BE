using System.Collections.Generic;
using API.DTO.Account.AccountDTO;

namespace API.DTO.Loan;

public class LoanDto
{
  public int Id { get; set; }
  public string? Note { get; set; }
  public int BorrowerId { get; set; }
  public AccountGDto? Borrower { get; set; }
  public int? LibrarianId { get; set; }
  public AccountGDto? Librarian { get; set; }
  public DateTime BorrowingDate { get; set; }
  public DateTime? ReturnDate { get; set; }
  public string? AprovalStatus { get; set; }
  public int? UsageDateCount { get; set; } = 0;
  public int? BookCount { get; set; } = 0;
  public int ExtensionCount { get; set; }
  public bool isReturnAll { get; set; } = false;
  public List<BookBorrowingDto> BookBorrowings { get; set; } = new();
}