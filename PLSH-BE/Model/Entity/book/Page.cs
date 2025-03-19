using System.ComponentModel.DataAnnotations;

namespace Model.Entity.book;

public class Page
{
  public int Id { get; set; }
  public int PageNumber { get; set; }
  [MaxLength(255)]
  public string PageUrl { get; set; }
  public int? BookId { get; set; }
}