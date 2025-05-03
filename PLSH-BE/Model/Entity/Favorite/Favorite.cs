using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;
using Model.Entity.User;

namespace Model.Entity.Favorite
{
  [Table("favorites")] // Mapping đúng tên bảng trong database
  public class Favorite
  {
    [Key]
    public int Id { get; set; }

    public int BorrowerId { get; set; }

    [ForeignKey("BorrowerId")]
    public Account Borrower { get; set; }

    public int BookId { get; set; }

    [ForeignKey("BookId")]
    public book.Book Book { get; set; }

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
