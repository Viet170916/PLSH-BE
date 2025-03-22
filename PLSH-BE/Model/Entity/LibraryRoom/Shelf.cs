using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Entity.LibraryRoom
{
  public class Shelf
  {
    [Key]
    public long Id { get; set; }

    public int RoomId { get; set; }

    [ForeignKey("RoomId")]
    public LibraryRoom? LibraryRoom { get; set; }

    public string? Name { get; set; }
    public string? Label { get; set; }
    public string? Column { get; set; }
    public string? Row { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public ICollection<RowShelf> RowShelves { get; set; } = [];
  }
}