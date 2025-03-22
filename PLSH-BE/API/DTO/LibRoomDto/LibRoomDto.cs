using System.Collections.Generic;

namespace API.DTO.LibRoomDto;

public class LibRoomDto
{
  public class ShelfDto
  {
    public long Id { get; set; }
    public int RoomId { get; set; }
    public string? Name { get; set; }
    public string? Label { get; set; }
    public string? Column { get; set; }
    public string? Row { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public List<RowShelfDto>? RowShelves { get; set; }
  }

  public class LibraryRoomDto
  {
    public int Id { get; set; }
    public string? Name { get; set; }
    public int ColumnSize { get; set; }
    public int RowSize { get; set; }
    public List<ShelfDto>? Shelves { get; set; }
  }

  public class RowShelfDto
  {
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Position { get; set; }
    public int MaxCol { get; set; }
  }
}