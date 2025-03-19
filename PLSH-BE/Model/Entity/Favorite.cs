using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace Model.Entity
{
  public class Favorite
  {
    public int Id { get; set; }
    public int BorrowerId { get; set; }
    public int BookId { get; set; }
    public DateTime AddedDate { get; set; } = DateTime.UtcNow;

    [MaxLength(255)]
    public string? Note { get; set; }

    public FavoriteStatus Status { get; set; } = FavoriteStatus.WantToRead;

    //public virtual Borrower? Borrower { get; set; }

    //public virtual Book? Book { get; set; }
  }
}