using System.ComponentModel.DataAnnotations;
using Common.Enums;

namespace Model.Entity;

public class BookBorrowing
{
  public int Id { get; set; }
  public int TransactionId { get; set; }
  public int BookDetailId { get; set; }

  [MaxLength(255)]
  public string? BookConditionUrl { get; set; } // hình ảnh lúc trước khi mượn sách

  public int LoanId { get; set; } //Lien ket voi bang Loan(Phieu muon sach)
  public int? PageCount { get; set; }

  [MaxLength(255)]
  public string? BookConditionDescription { get; set; } // mô tả tình trạng lúc trước khi mượn 

  public BorrowingStatus BorrowingStatus { get; set; } =
    BorrowingStatus.Borrowed; // trạng thái mượn (đã trả, đang mượn, quá hạn, mất)

  public DateTime? BorrowingDate { get; set; } // ngày mượn thực tế
  public DateTime? ReturnDate { get; set; } // ngày trả thực tế

  //Thông tin phạt
  public bool IsFined { get; set; } = false;
  public FineType? FineType { get; set; } = null; //(trả muộn/hỏng sách/mất sách)

  [MaxLength(500)]
  public string? Note { get; set; }
  //public virtual Loan? Loan { get; set; }
  //public virtual BookDetail? BookDetail { get; set; }
}