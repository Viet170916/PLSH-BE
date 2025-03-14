using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Entity.LibraryRoom;

public class RowShelf
{
  [Key]
  public long Id { get; set; }

  public string? Name { get; set; }
  public string? Description { get; set; }
  public long ShelfId { get; set; }

  [NotMapped]
  public Shelf? Shelf { get; set; }

  public int? Position { get; set; }
  public int MaxCol { get; set; }
}