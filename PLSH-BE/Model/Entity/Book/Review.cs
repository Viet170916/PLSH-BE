using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Model.Entity.User;

namespace Model.Entity.book;

public class Review
{
  [Key]
  public int Id { get; set; }

  public int AccountSenderId { get; set; }

  [ForeignKey("AccountSenderId")]
  public Account AccountSender { get; set; }

  public int BookId { get; set; }

  [ForeignKey("BookId")]
  public Book Book { get; set; }

  public int? ResourceId { get; set; }

  [ForeignKey("ResourceId")]
  public Resource? Resource { get; set; }

  [Range(1, 5)]
  public float Rating { get; set; }

  [Required]
  [MaxLength(3000)]
  public string Content { get; set; }

  [Required]
  public DateTime CreatedDate { get; set; } = DateTime.Now;

  public DateTime? UpdatedDate { get; set; }
  public ICollection<Message> Messages { get; set; } = new List<Message>();
}
