namespace API.Common;

public class Converter
{
  public static string ToImageUrl(string? pathToImage)
  {
    ;
    return $"{Environment.GetEnvironmentVariable("BACKEND_HOST")??"https://api.book-hive.space"}/static/v1/file{pathToImage ?? "/default"}";
  }
}
