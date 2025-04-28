using Model.Entity.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Entity.Borrow;

public class Fine
{
  public int Id { get; set; }
  public DateTime FineDate { get; set; }
  public bool IsFined { get; set; } = false;
  public int? FineType { get; set; } //(trả muộn/hỏng sách/mất sách)

  public double FineByDate { get; set; } //Phí phạt 1 ngày (giả sử 5000đ/ 1 ngày)
  [MaxLength(255)]
  public string? Note { get; set; }

  public int? BookBorrowingId { get; set; }
  public int BorrowerId { get; set; }
    [ForeignKey("BorrowerId")]
    public virtual Borrower? Borrower { get; set; }
    public int Status { get; set; } //(chờ, đã huỷ, thất bại, hoàn thành)
  public int? TransactionId { get; set; }
}