using Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
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