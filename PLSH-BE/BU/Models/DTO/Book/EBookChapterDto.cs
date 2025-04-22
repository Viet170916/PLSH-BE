namespace BU.Models.DTO.Book;

public class EBookChapterDto
{
  public class EBookChapterPlainTextDto
  {
    public int Id { get; set; }
    public int BookId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int ChapterIndex { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PlainText { get; set; } = string.Empty;
  }

  public class EBookChapterHtmlContentDto
  {
    public int Id { get; set; }
    public int BookId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int ChapterIndex { get; set; }
    public string Title { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
  }

  public class EBookChapterFulltDto
  {
    public int Id { get; set; }
    public int BookId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int ChapterIndex { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PlainText { get; set; } = string.Empty;
    public BookMinimalDto Book { get; set; }
    public string HtmlContent { get; set; } = string.Empty;
  }
}
