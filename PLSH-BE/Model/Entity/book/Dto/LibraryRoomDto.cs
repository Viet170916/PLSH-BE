namespace Model.Entity.book.Dto;

public class LibraryRoomDto
{
  public class BookInstanceDto
  {
    public int Id { get; set; }
    public string? Code { get; set; }
    public int? RowShelfId { get; set; }
    public string? BookName { get; set; }
    public string? BookThumbnail { get; set; }
    public string? BookVersion { get; set; }
    public string? BookAuthor { get; set; }
    public string? BookCategory { get; set; }
    public int? BookId { get; set; }
    public int? Position { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
  }

  public class RowShelfDto
  {
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Position { get; set; }
    public int MaxCol { get; set; }
    public int? Count { get; set; }
    public IList<BookInstanceDto>? BookInstances { get; set; }
  }

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
    public IList<RowShelfDto> RowShelves { get; set; } = [];
  }
}