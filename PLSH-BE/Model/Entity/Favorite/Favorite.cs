using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace Model.Entity
{
    [Table("favorites")] // Mapping đúng tên bảng trong database
    public class Favorite
    {
        [Key]
        public int Id { get; set; }

        [Column("BorrowerId")]
        public int BorrowerId { get; set; }

        [Column("BookId")]
        public int BookId { get; set; }

        [Column("AddedDate")]
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        [Column("Note")]
        public string? Note { get; set; }

        [Column("Status")]
        public FavoriteStatus Status { get; set; } = FavoriteStatus.WantToRead;

        // Nếu sau này cần liên kết bảng (navigation properties) thì mở ra
        // public virtual Borrower? Borrower { get; set; }
        // public virtual Book? Book { get; set; }
    }
}
