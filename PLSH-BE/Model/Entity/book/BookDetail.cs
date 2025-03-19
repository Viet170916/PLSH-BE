using System.ComponentModel.DataAnnotations;

namespace Model.Entity.book;

public class BookDetail
{
  public int Id { get; set; }
  public int BookId { get; set; }
  public int Status { get; set; } = 1;// 1 is available, 0 is unavailable 
  [MaxLength(255)]
  public string? BookConditionUrl {get; set;}
  [MaxLength(1000)]
  public string? BookConditionDescription { get; set; }
  [MaxLength(255)]
  public string? StatusDescription { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public DateTime? DeletedAt { get; set; }

    //public virtual Book? Book { get; set; }
}