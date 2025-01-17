namespace Model.Entity;

public class Book
{
  public int Id { get; set; }
  public string Title { get; set; }
  public string? Description { get; set; }
  public string Author { get; set; }
  public string? Publisher { get; set; }
  public DateTime? PublishDate { get; set; }
  public string? Language { get; set; }
  public string? Position { get; set; }
  public int PageCount { get; set; }
  public string? Category { get; set; }
  public string ISBNumber { get; set; }
  public int Quantity { get; set; }
  public double? Price { get; set; }
  public string? Thumbnail { get; set; }
  public double? Fine { get; set; }
  public DateTime CreateDate { get; set; }
  public DateTime? UpdateDate { get; set; }
  public DateTime? DeletedAt { get; set; }
}