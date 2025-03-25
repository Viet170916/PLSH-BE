using System.ComponentModel.DataAnnotations;

namespace Model.Entity.User
{
  public class CardMember
  {
    public int CardId { get; set; }
    public int AccountId { get; set; }
    public string? CardNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    [MaxLength(255)]
    public string? QrCode { get; set; }

    //public CardStatus? CardStatus { get; set; } 
  }
}