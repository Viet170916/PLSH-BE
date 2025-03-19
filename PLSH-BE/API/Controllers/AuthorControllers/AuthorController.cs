using API.Common;
using API.DTO.Book;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using Model.Entity.book;
using Model.helper;

namespace API.Controllers.AuthorControllers;

[Route("api/v1/author")]
public partial class AuthorController(AppDbContext context, GoogleCloudStorageHelper fileHelper) : Controller
{
  [HttpPost("add")] public async Task<IActionResult> AddAuthor([FromForm] AuthorDto request)
  {
    if (string.IsNullOrEmpty(request.FullName) || request.AuthorImageResource == null)
    {
      return BadRequest("Invalid author data.");
    }

    var uniqueFileName = $"{Guid.NewGuid()}_{request.AuthorImageResource.FileName}";
    var uploadsFolder = await fileHelper.UploadFileAsync(request.AuthorImageResource.OpenReadStream(),
      uniqueFileName, StaticFolder.DIRPath_AUTHOR, request.AuthorImageResource.ContentType);
    var resource = await context.Resources.AddAsync(new Resource()
    {
      Name = request.AuthorImageResource?.FileName,
      FileType = request.AuthorImageResource?.ContentType,
      LocalUrl = uploadsFolder,
      SizeByte = request.AuthorImageResource?.Length,
      Type = Status.ResourceType.Image,
    });
    await context.SaveChangesAsync();
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

  [HttpGet] public async Task<IActionResult> SearchAuthors([FromQuery] string? keyword)
  {
    var query = context.Authors.AsQueryable();
    if (!string.IsNullOrWhiteSpace(keyword))
    {
      query = query.Where(a =>
        a.FullName.Contains(keyword) ||
        (a.BirthYear != null && a.BirthYear.Contains(keyword)) ||
        (a.BirthYear != null && a.BirthYear.Contains(keyword)));
    }

    query = query.Select(author => new Author()
    {
      Id = author.Id,
      FullName = author.FullName,
      BirthYear = author.BirthYear,
      DeathYear = author.DeathYear,
      Description = author.Description,
      SummaryDescription = author.SummaryDescription,
      AvatarUrl = Converter.ToImageUrl(context.Resources.FirstOrDefault(re => re.Id == author.AuthorResourceId)!
                                              .LocalUrl),
    });
    var authors = await query.Take(100).ToListAsync();
    return Ok(authors);
  }

  
}