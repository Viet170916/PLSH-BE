using System.Collections.Generic;
using System.Text.Json;
using API.Common;
using BU.Models.DTO;
using BU.Models.DTO.Book;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.AiControllers;

public partial class AiController
{
  public class BookGenRequest
  {
    public required string BookName { get; set; }
    public required string Prompt { get; set; }
  }

  public class KeyValue
  {
    public string key { get; set; }
    public object value { get; set; }
  }

  public class ChatGptResponseFormat
  {
    public string message { get; set; }
    public List<KeyValue> data { get; set; }
  }

  [HttpPost("fields-generate")]
  public async Task<ActionResult<BaseResponse<List<KeyValue>>>> GenerateBookFields([FromBody] BookGenRequest request)
  {
    var fullPrompt = Const.GEN_BOOK_FIELS_PROMT
                          .Replace("{{bookName}}", request.BookName)
                          .Replace("{{prompt}}", request.Prompt);
    var maxRetries = 5;
    var attempt = 0;
    while (attempt < maxRetries)
    {
      var response = await geminiService.GetFromGeminiPromptAsync(fullPrompt);
      try
      {
        var parsed = JsonSerializer.Deserialize<BaseResponse<BookNewDtoAiRes>>(response,
          new JsonSerializerOptions { PropertyNameCaseInsensitive = true, });
        if (parsed?.Data == null)
        {
          return Ok(new BaseResponse<BookNewDtoAiRes?> { Message = "Phản hồi từ AI không hợp lệ.", Data = null });
        }

        return Ok(new BaseResponse<BookNewDtoAiRes> { Message = parsed.Message, Data = parsed.Data });
      }
      catch (JsonException)
      {
        attempt++;
        if (attempt >= maxRetries)
        {
          return Ok(new BaseResponse<BookNewDtoAiRes> { Message = "Lỗi khi cố gắng phản hồi.", Data = null });
        }
      }
    }

    return Ok(new BaseResponse<BookNewDtoAiRes> { Message = "Lỗi không xác định.", Data = null });
  }
}
