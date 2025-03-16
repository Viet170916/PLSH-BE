using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace API.DTO.Favorite
{
    public sealed class FavoriteDTO
    {
        [Key]
        public int Id { get; set; } // ID của bản ghi yêu thích

        [Required]
        public int BorrowerId { get; set; } // ID của người dùng thích sách này

        [Required]
        public int BookId { get; set; } // ID của sách được thích

        [Required]
        public DateTime AddedDate { get; set; } = DateTime.UtcNow; // Ngày thêm vào danh sách yêu thích

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string? Note { get; set; } // Ghi chú của người dùng về cuốn sách

        [Required]
        public FavoriteStatus Status { get; set; } = FavoriteStatus.WantToRead; // Trạng thái yêu thích

        // Navigation properties (nullable)
        //[ForeignKey("BorrowerId")]
        //public virtual Borrower? Borrower { get; set; }

        [ForeignKey("BookId")]
        public Model.Entity.book.Book? Book { get; set; }

    }
}
