using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Entity.LibraryRoom
{
  public class LibraryRoom
  {
    public int Id { get; set; }
    public string? Name { get; set; }
    public int ColumnSize { get; set; } = 6;
    public int RowSize { get; set; } = 3;
    public List<Shelf> Shelves { get; set; } = [];
  }
}