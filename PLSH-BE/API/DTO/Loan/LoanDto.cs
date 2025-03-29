using System.Collections.Generic;
using Common.Enums;

namespace API.DTO.Loan;

public class LoanDto
{
  public int Id { get; set; }
  public string? Note { get; set; }
  public int BorrowerId { get; set; }
  public int LibrarianId { get; set; }
  public DateTime BorrowingDate { get; set; }
  public DateTime? ReturnDate { get; set; }
  public string? AprovalStatus { get; set; }
  public int ExtensionCount { get; set; }
  public List<BookBorrowingDto> BookBorrowings { get; set; } = new();
}