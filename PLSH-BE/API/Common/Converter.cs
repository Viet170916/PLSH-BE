namespace API.Common;

public class Converter
{
  public static string ToImageUrl(string? pathToImage)
  {
    ;
    return $"{Environment.GetEnvironmentVariable("BACKEND_HOST")??"https://book-hive-api.space"}/static/v1/file{pathToImage ?? "/default"}";
  }
}