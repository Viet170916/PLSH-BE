using Model.helper;

namespace Model.Entity.book;

public class BookAvailabilityStatusDto
{
  public Status.BookAvailabilityStatus StatusMetaData { get; set; }
  public long Id { get; set; }
  public string Status { get; set; }
  public int Count { get; set; }
}