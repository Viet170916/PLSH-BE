using BU.Models.DTO;
using BU.Models.DTO.Book;
using BU.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.AiControllers;

[Route("api/v1/ai")]
[ApiController]
public partial class AiController(IChatGptService chatGptService, IGeminiService geminiService) : Controller
{
  public class Request
  {
    public required string Message { get; set; }
  }


  [HttpPost("send-message")] public async Task<IActionResult> SendMessage([FromBody] Request request)
  {
    try
    {
      var response = await geminiService.GetFromGeminiPromptAsync(request.Message);
      return Ok(new BaseResponse<string> { data = response, message = "Generate thành công", });
    }
    catch (Exception ex) { return StatusCode(500, $"Internal server error: {ex.Message}"); }
  }
}
