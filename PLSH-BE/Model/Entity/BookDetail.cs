namespace Model.Entity;

public class BookDetail
{
  public int Id { get; set; }
  public int BookId { get; set; }
  public int Status { get; set; } = 1;// 1 is available, 0 is unavailable 
  public string? BookConditionUrl {get; set;}
  public string? BookConditionDescription { get; set; }
  public string? StatusDescription { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public DateTime? DeletedAt { get; set; }

    //public virtual Book? Book { get; set; }
}