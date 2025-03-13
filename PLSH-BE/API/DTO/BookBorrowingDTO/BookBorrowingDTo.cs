using Common.Enums;

namespace API.DTO.BookBorrowingDTO
{
    public class BookBorrowingDTo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã giao dịch là bắt buộc.")]
        public int TransactionId { get; set; }

        [Url(ErrorMessage = "Đường dẫn hình ảnh không hợp lệ.")]
        public string? BookConditionUrl { get; set; } // hình ảnh trước khi mượn

        [Required(ErrorMessage = "Mã phiếu mượn là bắt buộc.")]
        public int LoanId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số trang sách phải lớn hơn 0.")]
        public int? PageCount { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả tình trạng sách không được vượt quá 1000 ký tự.")]
        public string? BookConditionDescription { get; set; }

        public BorrowingStatus BorrowingStatus { get; set; } = BorrowingStatus.Borrowed;

        [Required(ErrorMessage = "Ngày mượn thực tế là bắt buộc.")]
        public DateTime? BorrowingDate { get; set; }

        //[DateGreaterThan(nameof(BorrowingDate), ErrorMessage = "Ngày trả thực tế phải lớn hơn hoặc bằng ngày mượn.")]
        public DateTime? ReturnDate { get; set; }

        public bool isFined { get; set; } = false;
        public FineType? FineType { get; set; } = null; // (trả muộn/hỏng sách/mất sách)

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string? Note { get; set; }
    }
}
