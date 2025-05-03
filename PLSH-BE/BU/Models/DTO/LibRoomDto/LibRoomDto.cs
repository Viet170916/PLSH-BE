using System.Collections.Generic;

namespace BU.Models.DTO.LibRoomDto;

public class LibraryRoomDto
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public int ColumnSize { get; set; }
  public int RowSize { get; set; }
  public List<Model.Entity.book.Dto.LibraryRoomDto.ShelfDto>? Shelves { get; set; }
}
