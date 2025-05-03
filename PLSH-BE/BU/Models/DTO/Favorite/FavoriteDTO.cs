using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace BU.Models.DTO.Favorite
{
    public sealed class FavoriteDTO
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BorrowerId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string? Note { get; set; }

        [Required]
        public FavoriteStatus Status { get; set; } = FavoriteStatus.WantToRead;

        // Navigation properties (nullable)
        //[ForeignKey("BorrowerId")]
        //public virtual Borrower? Borrower { get; set; }

        [ForeignKey("BookId")]
        public Model.Entity.book.Book? Book { get; set; }

    }
}
