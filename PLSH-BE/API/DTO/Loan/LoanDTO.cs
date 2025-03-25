#nullable enable
using System.Collections.Generic;
using Common.Enums;
using Model.Entity;
using Model.Entity.User;

namespace API.DTO.Loan
{
    public class LoanDto
    {
        public int Id { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string? Note { get; set; }

        [Required(ErrorMessage = "Người mượn là bắt buộc.")]
        public int BorrowerId { get; set; }

        [Required(ErrorMessage = "Thủ thư xét duyệt là bắt buộc.")]
        public int LibrarianId { get; set; }

        [Required(ErrorMessage = "Ngày mượn dự kiến là bắt buộc.")]
        public DateTime BorrowingDate { get; set; }

        [Required(ErrorMessage = "Hạn trả là bắt buộc.")]
        //[DateGreaterThan(nameof(BorrowingDate), ErrorMessage = "Hạn trả phải lớn hơn ngày mượn.")]
        public DateTime ReturnDate { get; set; }

        public LoanStatus AprovalStatus { get; set; } = LoanStatus.Approved;

        public List<BookBorrowing> BookBorrowings { get; set; } = new();

        //public Borrower? Borrower { get; set; }
        public Librarian? Librarian { get; set; }
    }
}
