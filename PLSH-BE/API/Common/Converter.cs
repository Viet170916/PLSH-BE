namespace API.Common;

public class Converter
{
  public static string ToImageUrl(string? pathToImage)
  {
    Environment.GetEnvironmentVariable("BACKEND_HOST");
    return $"https://book-hive-api.spage/static/v1/file{pathToImage ?? "/default"}";
  }
}