using BU.Models.DTO.Book;
using BU.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.AiControllers;

[Route("api/v1/ai")]
[ApiController]
public partial class AiController(IChatGptService chatGptService, IGeminiService geminiService) : Controller
{
  [HttpPost("send-message")] public async Task<IActionResult> SendMessage([FromBody] string message)
  {
    try
    {
      var response = await chatGptService.SendMessageAsync(message);
      return Ok(response);
    }
    catch (Exception ex) { return StatusCode(500, $"Internal server error: {ex.Message}"); }
  }


}
