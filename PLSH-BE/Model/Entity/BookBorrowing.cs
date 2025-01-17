namespace Model.Entity;

public class BookBorrowing
{
  public int Id { get; set; }
  public int BookDetailId { get; set; }
  public int TransactionId { get; set; }
  public string? BookConditionUrl { get; set; } // hình ảnh lúc trước khi mượn sách
  public int? PageCount { get; set; }
  public string? BookConditionDescription { get; set; } // mô tả tình trạng lúc trước khi mượn 
  public string? BorrowingStatus { get; set; } // trạng thái mượn (đã trả, đang mượn, quá hạn, mất)
  public DateTime? BorrowingDate { get; set; } // ngày mượn thực tế
  public DateTime? ReturnDate { get; set; } // ngày trả thực tế
  public bool isFined { get; set; } = false;
  public int? FineType { get; set; } = 0;//(trả muộn/hỏng sách/mất sách)
  public string? Note { get; set; }
}