using API.Common;
using AutoMapper;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model.Entity;
using Model.helper;

namespace API.Controllers.ResourceControllers;

[Route("api/v1/resource")]
[ApiController]
public class ResourceController(
  AppDbContext context,
  ILogger<ResourceController> logger,
  IMapper mapper,
  GoogleCloudStorageHelper googleCloudStorageHelper
) : Controller
{
  [HttpPost("book/upload/{bookId}")]
  public async Task<IActionResult> UploadBookResource([FromForm] ResourceDto resource, [FromRoute] int bookId)
  {
    var book = await context.Books.FindAsync(bookId);
    if (book is null) return NotFound(new { Message = "Book not found", BookId = bookId, });
    var resourceEntity = mapper.Map<Resource>(resource);
    switch (resource.Type)
    {
      case "image":
        var coverImageUrl = await googleCloudStorageHelper.UploadFileAsync(resource.File.OpenReadStream(),
          resource.File.FileName,
          $"{StaticFolder.DIRPath_BOOK_COVER}/book_{bookId}",
          resource.File.ContentType);
        resourceEntity.Type = Status.ResourceType.Image;
        resourceEntity.LocalUrl = coverImageUrl;
        context.Resources.Add(resourceEntity);
        await context.SaveChangesAsync();
        book.CoverImageResourceId = resourceEntity.Id;
        break;
      case "pdf":
        var contentPdfUrl = await googleCloudStorageHelper.UploadFileAsync(resource.File.OpenReadStream(),
          resource.File.FileName,
          $"{StaticFolder.DIRPath_BOOK_PDF}/book_{bookId}",
          resource.File.ContentType);
        resourceEntity.Type = Status.ResourceType.Pdf;
        resourceEntity.LocalUrl = contentPdfUrl;
        context.Resources.Add(resourceEntity);
        await context.SaveChangesAsync();
        book.PreviewPdfResourceId = resourceEntity.Id;
        break;
      case "epub":
        var contentEpubUrl = await googleCloudStorageHelper.UploadFileAsync(resource.File.OpenReadStream(),
          resource.File.FileName,
          $"{StaticFolder.DIRPath_BOOK_EPUB}/book_{bookId}",
          resource.File.ContentType);
        resourceEntity.Type = Status.ResourceType.Epub;
        resourceEntity.LocalUrl = contentEpubUrl;
        context.Resources.Add(resourceEntity);
        await context.SaveChangesAsync();
        book.EpubResourceId = resourceEntity.Id;
        break;
      case "audio":
        var audioFileUrl = await googleCloudStorageHelper.UploadFileAsync(resource.File.OpenReadStream(),
          resource.File.FileName,
          $"{StaticFolder.DIRPath_BOOK_AUDIO}/book_{bookId}",
          resource.File.ContentType);
        resourceEntity.Type = Status.ResourceType.Audio;
        resourceEntity.LocalUrl = audioFileUrl;
        context.Resources.Add(resourceEntity);
        await context.SaveChangesAsync();
        book.AudioResourceId = resourceEntity.Id;
        break;
      default:
        return BadRequest(new { Message = "Resource type not supported(image/pdf/audio/epub)", BookId = bookId, });
    }

    await context.SaveChangesAsync();
    return Ok(new { Message = "Resource uploaded", BookId = bookId, resource = resourceEntity, });
  }
}

public class ResourceDto
{
  public int? Id { get; set; }
  public required string Type { get; set; }
  public string? Name { get; set; }
  public long? SizeByte { get; set; }
  public string? FileType { get; set; }
  public string? LocalUrl { get; set; }
  public required IFormFile File { get; set; }
}