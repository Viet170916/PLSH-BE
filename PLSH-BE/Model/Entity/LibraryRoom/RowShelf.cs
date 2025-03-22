using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Model.Entity.book;

namespace Model.Entity.LibraryRoom;

public class RowShelf
{
  [Key]
  public long Id { get; set; }

  public string? Name { get; set; }
  public string? Description { get; set; }
  public long ShelfId { get; set; }

  [ForeignKey("ShelfId")]
  public Shelf? Shelf { get; set; }

  public int? Position { get; set; }
  public int MaxCol { get; set; }

  [JsonIgnore]
  public ICollection<BookInstance>? BookInstances { get; set; }
}