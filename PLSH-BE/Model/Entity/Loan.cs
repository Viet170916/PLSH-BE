using System.ComponentModel.DataAnnotations;
using Common.Enums;

namespace Model.Entity;

public class Loan
{
  public int Id { get; set; }

  [MaxLength(255)]
  public string? Note { get; set; }

  public int BorrowerId { get; set; } //Người mượn
  public int LibrarianId { get; set; } //Ai xét duyệt mượn
  public DateTime BorrowingDate { get; set; } // ngày nhận dự kiến 
  public DateTime? ReturnDate { get; set; } // hạn trả dự kiến

  public LoanStatus AprovalStatus { get; set; } =
    LoanStatus.Approved; // trạng thái duyệt (duyệt, không duyệt, chờ duyệt)

  public int ExtensionCount { get; set; } = 0; //Đếm số lần gia hoan của BorrowerId

  //  public List<BookBorrowing> BookBorrowings { get; set; } = new();

  //public Borrower? Borrower { get; set; }  
  //public Librarian? Librarian { get; set; }    
}