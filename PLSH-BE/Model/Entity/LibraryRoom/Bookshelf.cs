using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Entity.LibraryRoom
{
  public class Bookshelf
  {
    [Key]
    public long Id { get; set; }

    public string? ColName { get; set; }
    public int Position { get; set; }
    public long? BookId { get; set; }
    public long? ShelfId { get; set; }
    public long RowShelfId { get; set; }

    [NotMapped]
    public Book? Book { get; set; }
  }
}