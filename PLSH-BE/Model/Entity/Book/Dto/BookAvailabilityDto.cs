namespace Model.Entity.book.Dto;

public class BookAvailabilityDto
{
  public long BookId { get; set; }
  public Book? Book { get; set; } = null!;
  public string BookName { get; set; } = null!;
  public string Position { get; set; } = null!;
  public List<BookAvailabilityStatusDto> BookAvailabilityStatuses { get; set; } = new();
}