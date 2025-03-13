using Common.Enums;
using System;
using System.Collections.Generic;
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

        public string? QRCode { get; set; }

        //public CardStatus? CardStatus { get; set; } 
    }
}
