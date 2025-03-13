using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Enums
{
    public enum BorrowingStatus
    {
        Borrowed,  // Đang mượn
        Returned,  // Đã trả
        Overdue,   // Quá hạn
        Lost       // Mất sách
    }
}
