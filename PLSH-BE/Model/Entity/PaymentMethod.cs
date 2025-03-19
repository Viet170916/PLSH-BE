using System.ComponentModel.DataAnnotations;

namespace Model.Entity;

public class PaymentMethod
{
  public int Id { get; set; }
  public int AccountId { get; set; }

  [MaxLength(4)]
  public required string CardLastFour { get; set; }

  [MaxLength(255)]
  public required string CardToken { get; set; }

  [MaxLength(50)]
  public required string BankAccount { get; set; }

  [MaxLength(50)]
  public required string BankName { get; set; }

  [MaxLength(10)]
  public required string PaymentType { get; set; }

  public DateTime CreateAt { get; set; }
  public DateTime UpdateAt { get; set; }
  public DateTime DeleteAt { get; set; }
}