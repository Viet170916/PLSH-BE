using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Entity.book
{
  public class Author
  {
    public int Id { get; set; } // ID tác giả (nếu có)
    public string FullName { get; set; } // Tên tác giả (nên là bắt buộc)
    public string? AvatarUrl { get; set; } // URL ảnh đại diện
    public int? AuthorResourceId { get; set; } // Tài nguyên liên quan

    [ForeignKey("BookAuthor")]
    public long? BookAuthorId { get; set; }

    public virtual ICollection<Book> Books { get; set; }

    [NotMapped]
    public Resource? Resource { get; set; }

    public string? Description { get; set; } // Mô tả tác giả
    public string? SummaryDescription { get; set; } // Mô tả ngắn gọn
    public string? BirthYear { get; set; } // Năm sinh
    public string? DeathYear { get; set; } // Năm mất
  }
}