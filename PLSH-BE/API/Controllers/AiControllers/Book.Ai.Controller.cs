using System.Collections.Generic;
using System.Text.Json;
using API.Common;
using BU.Models.DTO;
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

  // [HttpPost("fields-generate")]
  // public async Task<ActionResult<BaseResponse<List<KeyValue>>>> GenerateBookFields([FromBody] BookGenRequest request)
  // {
  //   var fullPrompt = Const.GEN_BOOK_FIELS_PROMT
  //                         .Replace("{{bookName}}", request.BookName)
  //                         .Replace("{{prompt}}", request.Prompt);
  //   var response = await chatGptService.SendMessageAsync(fullPrompt);
  //   try
  //   {
  //     var parsed = JsonSerializer.Deserialize<ChatGptResponseFormat>(response,
  //       new JsonSerializerOptions { PropertyNameCaseInsensitive = true, });
  //     if (parsed?.data == null)
  //     {
  //       return BadRequest(
  //         new BaseResponse<List<KeyValue>> { message = "Phản hồi từ ChatGPT không hợp lệ.", data = [], });
  //     }
  //
  //     return Ok(new BaseResponse<List<KeyValue>> { message = parsed.message, data = parsed.data });
  //   }
  //   catch (JsonException)
  //   {
  //     return BadRequest(new BaseResponse<List<KeyValue>>
  //     {
  //       message = "Phản hồi từ ChatGPT không đúng định dạng JSON yêu cầu.", data = []
  //     });
  //   }
  // }
  [HttpPost("fields-generate")]
  public async Task<ActionResult<BaseResponse<List<KeyValue>>>> GenerateBookFields([FromBody] BookGenRequest request)
  {
    var fullPrompt = Const.GEN_BOOK_FIELS_PROMT
                          .Replace("{{bookName}}", request.BookName)
                          .Replace("{{prompt}}", request.Prompt);
    var response = await geminiService.GetFromGeminiPromptAsync(fullPrompt);
    try
    {
      var parsed = JsonSerializer.Deserialize<ChatGptResponseFormat>(response,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true, });
      if (parsed?.data == null)
      {
        return BadRequest(
          new BaseResponse<List<KeyValue>> { message = "Phản hồi từ ChatGPT không hợp lệ.", data = [], });
      }

      return Ok(new BaseResponse<List<KeyValue>> { message = parsed.message, data = parsed.data });
    }
    catch (JsonException)
    {
      return BadRequest(new BaseResponse<List<KeyValue>>
      {
        message = "Phản hồi từ ChatGPT không đúng định dạng JSON yêu cầu.", data = []
      });
    }
  }
}
