using System.Collections.Generic;

namespace API.DTO.Borrower
{
    public class BorrowingRequestDto
    {
        public int BorrowerId { get; set; }        // Mã người mượn
        public List<int> BookDetailIds { get; set; } // Danh sách ID chi tiết sách mượn
        public string? Note { get; set; }           // Ghi chú mượn sách
        public int LibrarianId { get; set; }        // Mã thủ thư (người xét duyệt nhận thông báo)
    }
}
