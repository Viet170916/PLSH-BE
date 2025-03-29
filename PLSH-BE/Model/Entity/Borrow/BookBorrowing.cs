using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Common.Enums;
using Model.Entity.book;

namespace Model.Entity.Borrow;

public class BookBorrowing
{
  public int Id { get; set; }
  public int TransactionId { get; set; }
  public int BookInstanceId { get; set; }

  [ForeignKey("BookInstanceId")]
  public BookInstance BookInstance { get; set; }

  public ICollection<Resource> BookImagesBeforeBorrow { get; set; } = new List<Resource>();
  public ICollection<Resource> BookImagesAfterBorrow { get; set; } = new List<Resource>();
  public string? NoteBeforeBorrow { get; set; }
  public string? NoteAfterBorrow { get; set; }

  // public int BookDetailId { get; set; }

  // [MaxLength(255)]
  // public string? BookConditionUrl { get; set; }
  public int LoanId { get; set; }
  // public int? PageCount { get; set; }

  // [MaxLength(255)]
  // public string? BookConditionDescription { get; set; }
  public string BorrowingStatus { get; set; } = "on-loan";
  public DateTime BorrowDate { get; set; } = DateTime.Now;
  public DateTime CreatedAt { get; set; } = DateTime.Now;

  [Column(TypeName = "text")]
  public string ReturnDatesJson { get; set; } = "[]";

  [Column(TypeName = "text")]
  public string ExtendDatesJson { get; set; } = "[]";

  [NotMapped]
  public List<DateTime> ReturnDates
  {
    get => JsonSerializer.Deserialize<List<DateTime>>(ReturnDatesJson) ?? [];
    set => ReturnDatesJson = JsonSerializer.Serialize(value);
  }

  [NotMapped]
  public List<DateTime> ExtendDates
  {
    get => JsonSerializer.Deserialize<List<DateTime>>(ExtendDatesJson) ?? [];
    set => ExtendDatesJson = JsonSerializer.Serialize(value);
  }

//Thông tin phạt
  public bool IsFined { get; set; } = false;
  public FineType? FineType { get; set; } = null;

  [MaxLength(500)]
  public string? Note { get; set; }

  [ForeignKey("LoanId")]
  public virtual Loan? Loan { get; set; }
}