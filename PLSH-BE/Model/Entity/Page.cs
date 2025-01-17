namespace Model.Entity;

public class Page
{
  public int Id { get; set; }
  public int PageNumber { get; set; }
  public string PageUrl { get; set; }
  public int? BookId { get; set; }
}