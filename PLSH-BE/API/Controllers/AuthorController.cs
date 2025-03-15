using API.Common;
using API.DTO.Book;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Model.Entity;
using Model.helper;

namespace API.Controllers;

[Route("api/v1/author")]
public class AuthorController(AppDbContext context, GoogleCloudStorageHelper fileHelper) : Controller
{
  [HttpPost("add")] public async Task<IActionResult> AddAuthor([FromForm] AuthorDto request)
  {
    if (string.IsNullOrEmpty(request.FullName) || request.AuthorImageResource == null)
    {
      return BadRequest("Invalid author data.");
    }

    var uniqueFileName = $"{Guid.NewGuid()}_{request.AuthorImageResource.FileName}";
    var uploadsFolder = await fileHelper.UploadFileAsync(request.AuthorImageResource.OpenReadStream(),
      uniqueFileName, StaticFolder.DIRPath_AUTHOR, request.AuthorImageResource?.ContentType);
    var resource = await context.Resources.AddAsync(new Resource()
    {
      Name = request.AuthorImageResource?.FileName,
      FileType = request.AuthorImageResource?.ContentType,
      LocalUrl = uploadsFolder,
      SizeByte = request.AuthorImageResource?.Length,
      Type = Status.ResourceType.Image,
    });
    var newAuthor = new Author
    {
      FullName = request.FullName,
      Description = request.Description,
      SummaryDescription = request.SummaryDescription,
      BirthYear = request.BirthYear,
      DeathYear = request.DeathYear,
      AuthorResourceId = resource.Entity.Id,
    };
    context.Authors.Add(newAuthor);
    await context.SaveChangesAsync();
    return Ok(newAuthor);
  }
}