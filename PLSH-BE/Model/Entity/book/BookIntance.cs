using Model.Entity.LibraryRoom;

namespace Model.Entity.book;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class BookInstance
{
  [Key]
  public int Id { get; set; }

  [MaxLength(50)]
  public string? Code { get; set; }

  public int? BookOnRowShelfId { get; set; }
  public int BookId { get; set; }

  [ForeignKey("BookId")]
  public Book Book { get; set; } = null!;

  [ForeignKey("BookShelfId")]
  public virtual Bookshelf? BookOnRowShelf { get; set; }
}