using Common.Enums;

namespace Model.Entity;

public class Book
{
  public int Id { get; set; }
  public string? Title { get; set; }
  public string? Description { get; set; }
  public int AuthorId { get; set; }
  
  public AvailabilityKind Kind { get; set; }
  public int? CoverImageResourceId { get; set; }//Anh bia

    //Kiem tra co phai PDF => ebook
  public int? PreviewPdfResourceId { get; set; }


    //Kiem tra co phai mp3, ... => audio
  public int? AudioResourceId { get; set; } 
  public string? Version { get; set; }
  public string? Publisher { get; set; }
  public DateTime? PublishDate { get; set; }
  public string? Language { get; set; }
  public int PageCount { get; set; }
  public BookType? BookType { get; set; }
  public int CategoryId { get; set; }
  public string? ISBNumber { get; set; }
  public float? rating { get; set; }   //Đánh giá sách bằng sao
  public int TotalCopies { get; set; }  // Tổng số lượng sách có sẵn
  public int AvailableCopies { get; set; }  // Số sách còn có thể mượn
  public double? Price { get; set; }
  public string? Thumbnail { get; set; }//Đường dẫn đến ảnh bìa cuốn sách
  public double? Fine { get; set; } //Mức phạt của quyển sách
  public DateTime CreateDate { get; set; }
  public DateTime? UpdateDate { get; set; }
  public DateTime? DeletedAt { get; set; }

  public bool IsChecked { get; set; }

  public int BookReviewId {  get; set; }    
  public int Availability { get; set; } //Các trạng thái sách có sẵn
  public int Availabilities { get; set; } 
}