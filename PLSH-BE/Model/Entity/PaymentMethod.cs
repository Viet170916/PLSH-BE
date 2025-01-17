namespace Model.Entity;

public class PaymentMethod
{
  public int Id { get; set; }
  public int AccountId { get; set; }
  public string CardLastFour { get; set; }
  public string CardToken { get; set; }
  public string BankAccount { get; set; }
  public string BankName { get; set; }
  
  public string PaymentType { get; set; }
  public DateTime CreateAt { get; set; }
  public DateTime UpdateAt { get; set; }
  public DateTime DeleteAt { get; set; }
  
}