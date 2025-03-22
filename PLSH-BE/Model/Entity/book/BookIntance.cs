using Model.Entity.LibraryRoom;

namespace Model.Entity.book;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public sealed class BookInstance
{
  [Key]
  public int Id { get; set; }

  [MaxLength(10)]
  public string? Code { get; set; }

  public int? RowShelfId { get; set; }
  public int BookId { get; set; }
  public int Position { get; set; }

  [ForeignKey("BookId")]
  public Book Book { get; set; } = null!;

  [ForeignKey("ShelfId")]
  public RowShelf? RowShelf { get; set; }
}