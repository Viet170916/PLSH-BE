using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Model.Entity.book
{
  [Index(nameof(FullName), nameof(BirthYear), nameof(DeathYear), IsUnique = true)]
  public sealed class Author
  {
    public int Id { get; set; } // ID tác giả (nếu có)

    [MaxLength(255)]
    [Column(TypeName = "VARCHAR(255)")]
    public required string FullName { get; set; } // Tên tác giả (nên là bắt buộc)

    [MaxLength(550)]
    public string? AvatarUrl { get; set; } // URL ảnh đại diện

    [ForeignKey("Resource")]
    public int? AuthorResourceId { get; set; } // Tài nguyên liên quan

    // [ForeignKey("BookAuthor")]
    // public int? BookAuthorId { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();

    public Resource? Resource { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; } // Mô tả tác giả

    [MaxLength(1500)]
    public string? SummaryDescription { get; set; } // Mô tả ngắn gọn

    [MaxLength(50)]
    public string? BirthYear { get; set; } // Năm sinh

    [MaxLength(50)]
    public string? DeathYear { get; set; } // Năm mất
  }
}