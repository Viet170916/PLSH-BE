using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
    public class BookReservation
    {
        public int Id { get; set; }
        public int BookDetailId { get; set; } // Thay BookId thành BookDetailId
        public int AccountId { get; set; }
        public DateTime ReservationDate { get; set; }
        public int Status { get; set; } // 1: Đang đặt | 0: Đã hủy
    }
}
