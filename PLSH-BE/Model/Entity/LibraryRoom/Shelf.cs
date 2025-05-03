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
    public string Type { get; set; }
    public int? X1 { get; set; }
    public int? X2 { get; set; }
    public int? Y1 { get; set; }
    public int? Y2 { get; set; }
    public int Angle { get; set; }
    public int RootX { get; set; }
    public int RootY { get; set; }
    public ICollection<RowShelf> RowShelves { get; set; } = [];
  }
}
