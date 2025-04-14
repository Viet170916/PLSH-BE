using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using API.Controllers.BookControllers;

namespace API.Common;

public class Converter
{
  public static string ToImageUrl(string? pathToImage)
  {
    ;
    return
      $"{Environment.GetEnvironmentVariable("BACKEND_HOST") ?? "https://api.book-hive.space"}/static/v1/file{pathToImage ?? "/default"}";
  }

  public class RawTtsResponse
  {
    public string audio_base64 { get; set; } = string.Empty;
    public NormalizedAlignment normalized_alignment { get; set; } = new();
  }

  public class NormalizedAlignment
  {
    public List<string> characters { get; set; } = new();
    public List<double> character_start_times_seconds { get; set; } = new();
    public List<double> character_end_times_seconds { get; set; } = new();
  }

  public static BookController.TtsResponseDto ParseTtsResponse(string json)
  {
    var raw = JsonSerializer.Deserialize<RawTtsResponse>(json);
    var result = new BookController.TtsResponseDto
    {
      AudioBase64 = raw?.audio_base64 ?? string.Empty, Alignment = new List<BookController.TtsTimestamp>()
    };
    if (raw?.normalized_alignment?.characters == null || raw.normalized_alignment.characters.Count == 0) return result;
    var chars = raw.normalized_alignment.characters;
    var starts = raw.normalized_alignment.character_start_times_seconds;
    var ends = raw.normalized_alignment.character_end_times_seconds;
    var word = new StringBuilder();
    int startIndex = -1;
    for (int i = 0; i < chars.Count; i++)
    {
      var ch = chars[i];
      if (!string.IsNullOrWhiteSpace(ch))
      {
        if (word.Length == 0) startIndex = i;
        word.Append(ch);
      }

      bool isLastChar = i == chars.Count - 1;
      bool isWordBreak = string.IsNullOrWhiteSpace(ch) || isLastChar;
      if ((isWordBreak && word.Length > 0) || (isLastChar && word.Length > 0))
      {
        int endIndex = i;
        if (isLastChar && !string.IsNullOrWhiteSpace(ch))
          endIndex = i;
        else
          endIndex = i - 1;
        result.Alignment.Add(new BookController.TtsTimestamp
        {
          Word = word.ToString(), Start = starts[startIndex], End = ends[endIndex]
        });
        word.Clear();
        startIndex = -1;
      }
    }

    return result;
  }
}
