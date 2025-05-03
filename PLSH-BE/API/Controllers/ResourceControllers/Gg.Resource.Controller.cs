using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.ResourceControllers;

public partial class ResourceController
{
  [HttpGet("gg/images")] public async Task<IActionResult> Get([FromQuery] string query)
  {
    if (string.IsNullOrWhiteSpace(query)) return BadRequest("Query is required.");
    var images = await ggServices.SearchImageUrlsAsync(  query);
    return Ok(images);
  }
}
