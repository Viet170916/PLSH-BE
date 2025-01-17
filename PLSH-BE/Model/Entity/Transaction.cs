namespace Model.Entity;

public class Transaction
{
  public int Id { get; set; }
  public int AccountId { get; set; }
  public int PaymentMethodId { get; set; }
  public decimal Amount { get; set; }
  public string Currency { get; set; } = "VND"; // Đơn vị tiền tệ
  public int Status { get; set; } // Enum trạng thái giao dịch (thất bại, chờ, thành công, đã huỷ)
  public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
  public string ReferenceId { get; set; } // ID tham chiếu từ Payment Gateway
  public int TransactionType { get; set; } //(nộp phạt)
  public string Note { get; set; }
}