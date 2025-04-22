using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Entity.Book.e_book;

public class EBookChapter
{
  [Key]
  public int Id { get; set; }

  public int BookId { get; set; }

  [ForeignKey("BookId")]
  public Model.Entity.book.Book Book { get; set; }

  public int ChapterIndex { get; set; }
  public string FileName { get; set; } = string.Empty;
  public string HtmlContent { get; set; } = string.Empty;
  public string PlainText { get; set; } = string.Empty;
  public string? Title { get; set; }
}
