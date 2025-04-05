using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Model.Entity.User;

namespace Model.Entity.book;

public class Message
{
  [Key]
  public int Id { get; set; }

  public int SenderId { get; set; }

  [ForeignKey("SenderId")]
  public Account Sender { get; set; }

  public int? RepliedId { get; set; }

  [ForeignKey("RepliedId")]
  public Account? RepliedPerson { get; set; }

  public int ReviewId { get; set; }

  [ForeignKey("ReviewId")]
  public Review Review { get; set; }

  [Required]
  [MaxLength(3000)]
  public string Content { get; set; }

  public int? ResourceId { get; set; }

  [ForeignKey("ResourceId")]
  public Resource? Resource { get; set; }

  [Required]
  public DateTime CreatedDate { get; set; } = DateTime.Now;

  public DateTime? UpdatedDate { get; set; }
}
