using System.Collections.Generic;
using Model.Entity;

namespace API.DTO.Loan;

public class BookBorrowingDto
{
  public int Id { get; set; }

  // public int ?TransactionId { get; set; }
  public int BookInstanceId { get; set; }
  public List<Resource> BookImagesBeforeBorrow { get; set; } = new();
  public List<Resource>? BookImagesAfterBorrow { get; set; } = new();
  public string? NoteBeforeBorrow { get; set; }
  public string? NoteAfterBorrow { get; set; }
  public int? LoanId { get; set; }
  public string? BorrowingStatus { get; set; }
  public DateTime BorrowDate { get; set; }
  public DateTime? CreatedAt { get; set; }
  public int? ReferenceId { get; set; }
  public List<DateTime> ReturnDates { get; set; } = new();
  public List<DateTime>? ExtendDates { get; set; }
  public bool IsFined { get; set; }
  public string? FineType { get; set; }
  public string? Note { get; set; }
}