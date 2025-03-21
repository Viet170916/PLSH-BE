namespace Model.helper;

public static class Status
{
  public abstract class AvailabilityType
  {
    public static string Ebook { get; } = "e-book";
    public static string Audio { get; } = "audio";
    public static string Physical { get; } = "physical";
  }

  public abstract class ResourceType
  {
    public static string Image { get; } = "image";
    public static string Pdf { get; } = "pdf";
    public static string Epub { get; } = "epub";
    public static string Audio { get; } = "audio";
  }

  public abstract class BookAvailabilityStatus
  {
    public static string InShelf { get; } = "in-shelf";
    public static string CheckedOut { get; } = "checked-out";
    public static string Lost { get; } = "lost";
    public static string Processing { get; } = "processing";
    public static string Damaged { get; } = "damaged";
  }
}