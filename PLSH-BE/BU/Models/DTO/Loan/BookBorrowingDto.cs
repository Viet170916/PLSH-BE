#nullable enable
using System;
using System.Collections.Generic;
using Model.Entity;
using Model.Entity.book.Dto;

namespace BU.Models.DTO.Loan;

public class BookBorrowingDto
{
  public int Id { get; set; }

  // public int ?TransactionId { get; set; }
  public int BookInstanceId { get; set; }
  public LibraryRoomDto.BookInstanceDto? BookInstance { get; set; }
  public List<Resource> BookImagesBeforeBorrow { get; set; } = new();
  public List<string>? BookImageUrlsBeforeBorrow { get; set; } = new();
  public List<Resource>? BookImagesAfterBorrow { get; set; } = new();
  public List<string>? BookImageUrlsAfterBorrow { get; set; } = new();
  public string? NoteBeforeBorrow { get; set; }
  public string? NoteAfterBorrow { get; set; }
  public int? LoanId { get; set; }
  public string? BorrowingStatus { get; set; }
  public DateTime BorrowDate { get; set; }
  public DateTime? CreatedAt { get; set; }
  public int? ReferenceId { get; set; }
  public List<DateTime> ReturnDates { get; set; } = new();
  public DateTime? ReturnDate { get; set; }
  public List<DateTime>? ExtendDates { get; set; }
  public DateTime? ExtendDate { get; set; }
  public bool IsFined { get; set; }
  public string? FineType { get; set; }
  public string? Note { get; set; }
  public int overdueDays { get; set; } = 0;
}

public class BookBorrowingDetailDto
{
  public int Id { get; set; }
  public string? BorrowerAvatar { get; set; }
  public string? BorrowerFullName { get; set; }
  public string? BorrowerEmail { get; set; }
  public string? BorrowerPhone { get; set; }
  public string? BorrowerClass { get; set; }
  public string? BorrowerRole { get; set; }
  public int BookInstanceId { get; set; }
  public LibraryRoomDto.BookInstanceDto? BookInstance { get; set; }
  public List<string>? BookImageUrlsBeforeBorrow { get; set; } = new();
  public List<string>? BookImageUrlsAfterBorrow { get; set; } = new();
  public string? NoteBeforeBorrow { get; set; }
  public string? NoteAfterBorrow { get; set; }
  public int? LoanId { get; set; }
  public string? BorrowingStatus { get; set; }
  public DateTime BorrowDate { get; set; }
  public DateTime? CreatedAt { get; set; }
  public int? ReferenceId { get; set; }
  public List<DateTime> ReturnDates { get; set; } = new();
  public DateTime? ReturnDate { get; set; }
  public List<DateTime>? ExtendDates { get; set; }
  public DateTime? ExtendDate { get; set; }
  public bool IsFined { get; set; }
  public string? FineType { get; set; }
  public string? Note { get; set; }
  public int OverdueDays { get; set; } = 0;
  public int? BorrowerId { get; set; }
}
