using System.Text.Json;
using API.Common;
using BU.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.AiControllers;

public partial class AiController
{
  public class Article
  {
    public string Title { get; set; }
    public string Content { get; set; }
    public string Author { get; set; }
  }

  [HttpGet("today-quote")] public async Task<ActionResult<BaseResponse<Article>>> GenerateTodayQuote()
  {
    const string fullPrompt = Const.TODAY_QUOTE_PROMT;
    const int maxRetry = 3;
    for (var attempt = 1; attempt <= maxRetry; attempt++)
    {
      var response = await geminiService.GetFromGeminiPromptAsync(fullPrompt);
      try
      {
        var parsed = JsonSerializer.Deserialize<Article>(response,
          new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (parsed != null) { return Ok(new BaseResponse<Article> { Message = "Thành công", Data = parsed }); }
      }
      catch (JsonException)
      {
        if (attempt == maxRetry)
        {
          return BadRequest(new BaseResponse<Article>
          {
            Message = "Phản hồi từ Gemini không đúng định dạng JSON yêu cầu sau 3 lần thử.", Data = null
          });
        }
      }
    }

    return BadRequest(new BaseResponse<Article> { Message = "Không thể phân tích phản hồi từ Gemini.", Data = null });
  }
}
