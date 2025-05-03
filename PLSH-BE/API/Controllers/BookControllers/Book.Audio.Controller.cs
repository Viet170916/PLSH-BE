using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using API.Common;
using BU.Models.DTO;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VersOne.Epub;

namespace API.Controllers.BookControllers;

public partial class BookController
{
  // private readonly TextToSpeechClient _client = TextToSpeechClient.Create();
  //
  // public class TtsRequest
  // {
  //   public string Text { get; set; } = string.Empty;
  //   public float Rate { get; set; } = 1.0f;
  // }
  private const string ApiKey = "sk_4078e6e5b0e88f62d784c46580690d0e098f4bc109fa3254";

  [HttpPost("speak")] public async Task<IActionResult> Speak([FromBody] TtsRequestDto request)
  {
    var httpClient = httpClientFactory.CreateClient();
    string apiKey = null;
    try
    {
      // Lấy API Key hợp lệ
      apiKey = await GetValidApiKeyAsync();
    }
    catch (Exception ex)
    {
      return StatusCode(500, new BaseResponse<string> { Status = "fail", Message = ex.Message, Data = null });
    }

    var url =
      $"https://api.elevenlabs.io/v1/text-to-speech/{request.VoiceId ?? "9BWtsMINqrJLrRacOk9x"}/with-timestamps";
    var payload = JsonSerializer.Serialize(new
    {
      text = request.Text, model_id = "eleven_flash_v2_5", output_format = "mp3_44100_128",
    });
    var content = new StringContent(payload, Encoding.UTF8, "application/json");
    httpClient.DefaultRequestHeaders.Clear();
    httpClient.DefaultRequestHeaders.Add("xi-api-key", apiKey);
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    var response = await httpClient.PostAsync(url, content);
    if (!response.IsSuccessStatusCode)
    {
      await RemoveInvalidApiKeyAsync(apiKey);

      try
      {
        apiKey = await GetValidApiKeyAsync();
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("xi-api-key", apiKey);
        response = await httpClient.PostAsync(url, content);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new BaseResponse<string> { Status = "fail", Message = ex.Message, Data = null });
      }

      if (!response.IsSuccessStatusCode)
      {
        return StatusCode((int)response.StatusCode,
          new BaseResponse<string> { Status = "fail", Message = "Failed to call ElevenLabs API", Data = null });
      }
    }

    var json = await response.Content.ReadAsStringAsync();
    var result = Converter.ParseTtsResponse(json);
    return Ok(new BaseResponse<TtsResponseDto>
    {
      Status = "success", Message = "Successfully generated speech", Data = result,
    });
    // var httpClient = httpClientFactory.CreateClient();
    // var url =
    //   $"https://api.elevenlabs.io/v1/text-to-speech/{request.VoiceId ?? "9BWtsMINqrJLrRacOk9x"}/with-timestamps";
    // var payload = JsonSerializer.Serialize(new
    // {
    //   text = request.Text, model_id = "eleven_flash_v2_5", output_format = "mp3_44100_128",
    // });
    // var content = new StringContent(payload, Encoding.UTF8, "application/json");
    // httpClient.DefaultRequestHeaders.Clear();
    // httpClient.DefaultRequestHeaders.Add("xi-api-key", ApiKey);
    // httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    // var response = await httpClient.PostAsync(url, content);
    // if (!response.IsSuccessStatusCode)
    // {
    //   return StatusCode((int)response.StatusCode,
    //     new BaseResponse<string> { Status = "fail", Message = "Failed to call ElevenLabs API", Data = null });
    // }
    //
    // var json = await response.Content.ReadAsStringAsync();
    // var result = Converter.ParseTtsResponse(json);
    // return Ok(new BaseResponse<TtsResponseDto>
    // {
    //   Status = "success", Message = "Successfully generated speech", Data = result,
    // });
  }

  private async Task RemoveInvalidApiKeyAsync(string apiKey)
  {
    var invalidApiKey = await context.ApiKeys.FirstOrDefaultAsync(a => a.Key == apiKey);
    if (invalidApiKey != null)
    {
      context.ApiKeys.Remove(invalidApiKey);
      await context.SaveChangesAsync();
    }
  }

  private async Task<string> GetValidApiKeyAsync()
  {
    var apiKey = await context.ApiKeys.FirstOrDefaultAsync();
    if (apiKey == null) throw new InvalidOperationException("Không có API key hợp lệ.");
    return apiKey.Key;
  }

  [HttpGet("{bookId}/to-text")] public async Task<IActionResult> GetEpubText(
    [FromRoute] int bookId,
    [FromQuery] string? lang,
    [FromQuery] int chapter = 1
  )
  {
    var book = await context.Books.Include(b => b.EpubResource).FirstOrDefaultAsync(b => bookId == b.Id);
    if (book is null) { return NotFound(new BaseResponse<string> { Message = "Book not found.", Data = null }); }

    if (book.EpubResource?.LocalUrl is null)
    {
      return NotFound(new BaseResponse<string> { Message = "Book Preview Resource not found.", Data = null });
    }

    try
    {
      var tempEpubPath = Path.Combine(Path.GetTempPath(), "Book.epub");
      await helper.DownloadEpubFromGcs(book.EpubResource.LocalUrl, tempEpubPath);
      var epubBook = await EpubReader.ReadBookAsync(tempEpubPath);
      var allChapters = epubBook.ReadingOrder.ToList();
      var maxChapterAttempts = 5;
      var chapterAttempts = 0;
      while (chapterAttempts < maxChapterAttempts && chapter + chapterAttempts <= allChapters.Count)
      {
        var currentChapter = chapter + chapterAttempts;
        var selectedChapter = allChapters[currentChapter - 1];
        var chapterHtml = selectedChapter.Content;
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(chapterHtml);
        var textContent = htmlDoc.DocumentNode.InnerText;
        var prompt = Const.ANALIZE_PHASE_OF_BOOK_PROMT
                          .Replace("{{text}}", textContent) +
                     (lang is not null ? $"dịch văn bản sang tiếng \"{lang}\"" : "");
        var maxRetry = 3;
        for (var retry = 0; retry < maxRetry; retry++)
        {
          try
          {
            var messageFromGemini = await geminiService.GetFromGeminiPromptAsync(prompt);
            var data = JsonSerializer.Deserialize<List<TextPhaseChunks>>(
              Regex.Replace(messageFromGemini, @"```[a-zA-Z]+", "").Replace("```", ""),
              new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (data != null && data.Any())
            {
              return Ok(new BaseResponse<List<TextPhaseChunks>>
              {
                PageCount = allChapters.Count,
                Message = "Lấy nội dung chương thành công.",
                Data = data,
                Page = currentChapter
              });
            }

            break;
          }
          catch
          {
            if (retry == maxRetry - 1)
            {
              return StatusCode(500,
                new BaseResponse<string?>
                {
                  PageCount = allChapters.Count,
                  Message = "Có lỗi xảy ra khi xử lý nội dung. Vui lòng thử lại.",
                  Data = null,
                  Status = "error",
                  Page = currentChapter
                });
            }
          }
        }

        chapterAttempts++;
      }

      return NotFound(new BaseResponse<string?>
      {
        PageCount = allChapters.Count,
        Message = "Không tìm thấy nội dung phù hợp.",
        Data = null,
        Status = "error",
        Page = chapter + chapterAttempts - 1
      });
    }
    catch
    {
      return StatusCode(500,
        new BaseResponse<string?> { Message = "Lỗi khi xử lý EPUB", Data = null, Status = "error" });
    }
  }

  public class TextPhaseChunks
  {
    public string text { get; set; } = "";
    public int p { get; set; }
  }

  public class TtsRequestDto
  {
    public string Text { get; set; } = string.Empty;
    public string VoiceId { get; set; } = "21m00Tcm4TlvDq8ikWAM"; // default voice
  }

  public class TtsResponseDto
  {
    public string AudioBase64 { get; set; } = string.Empty;
    public List<TtsTimestamp> Alignment { get; set; } = new();
  }

  public class TtsTimestamp
  {
    public string Word { get; set; } = string.Empty;
    public double Start { get; set; }
    public double End { get; set; }
  }
}
