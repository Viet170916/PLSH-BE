using Model.helper;

namespace Model.Entity.book;

public class AvailabilityDto
{
  public long? Id { get; set; }
  public Status.AvailabilityType TypeMetaData { get; set; }
  public string Kind { get; set; }
  public bool? IsChecked { get; set; }
  public string? Title { get; set; }
  public string? Position { get; set; }
  public long? ResourceId { get; set; }
  public Resource? Resource { get; set; }
}