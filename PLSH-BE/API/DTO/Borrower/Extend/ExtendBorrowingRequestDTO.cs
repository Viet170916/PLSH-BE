namespace API.DTO.Borrower.Extend
{
    public class ExtendBorrowingRequestDto
    {
        public int LoanId { get; set; } // ID của lần mượn sách cần gia hạn
        public int LibrarianId { get; set; } // ID của thủ thư xác nhận gia hạn
        public int BorrowerId { get; set; } // ID của người mượn
    }
}
