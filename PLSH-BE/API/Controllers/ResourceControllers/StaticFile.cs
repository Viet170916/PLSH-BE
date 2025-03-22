using System.IO;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using VersOne.Epub;
using System.Text.RegularExpressions;
using API.Common;
using Common.Infrastructure.Extensions;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;
using VersOne.Epub;
using System.IO.Compression;
using Data.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using VersOne.Epub.Options;

namespace API.Controllers.ResourceControllers;

[ApiController]
[Route("static/v1")]
public class FileController(StorageClient storageClient, AppDbContext context) : ControllerBase
{
  private readonly string? _bucketName = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_BUCKET");

  [HttpGet("file/{*path}")] public async Task<IActionResult> GetFile(string path)
  {
    if (string.IsNullOrEmpty(path)) return BadRequest("File path is required.");
    try
    {
      var filePath = path.Replace("%2F", "/");
      if (!string.IsNullOrEmpty(filePath) && !filePath.StartsWith("/")) { filePath = $"/{filePath}"; }

      var stream = new MemoryStream();
      await storageClient.DownloadObjectAsync(_bucketName, filePath, stream);
      stream.Position = 0;
      var contentType = GetContentType(filePath);

      // Đặt header Content-Disposition thành "inline"
      Response.Headers["Content-Disposition"] = $"inline; filename={Path.GetFileName(filePath)}";
      return File(stream, contentType);
    }
    catch { return NotFound("File not found."); }
  }

  private static string GetContentType(string filePath)
  {
    var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
    if (!provider.TryGetContentType(filePath, out var contentType)) { contentType = "application/octet-stream"; }

    return contentType;
  }

  [HttpGet("preview/{bookId}")]
  public async Task<IActionResult> PreviewEpub([FromRoute] int bookId, [FromQuery] int chapter = 1)
  {
    var book = await context.Books.Include(b => b.EpubResource).FirstOrDefaultAsync(b => bookId == b.Id);
    if (book is null) return NotFound(new { message = "Book not found." });
    if (book.EpubResource is null) return NotFound(new { message = "Book Preview Resource not found.", });
    if (book.EpubResource?.LocalUrl is null) return NotFound(new { message = "Book Preview Resource not found.", });
    try
    {
      var tempEpubPath = Path.Combine(Path.GetTempPath(), "book.epub");
      var path = book.EpubResource.LocalUrl;
      await DownloadEpubFromGcs(path, tempEpubPath);
      var epubBook = await EpubReader.ReadBookAsync(tempEpubPath);
      var allChapters = epubBook.ReadingOrder.ToList();
      if (chapter < 1 || chapter > allChapters.Count) { return BadRequest(new { message = "Chapter không hợp lệ." }); }

      var selectedChapter = allChapters[chapter - 1];
      var chapterHtml = selectedChapter.Content;
      return Content(chapterHtml, "text/html");
    }
    catch (Exception ex) { return StatusCode(500, new { message = "Lỗi khi xử lý EPUB", error = ex.Message }); }
  }

  private async Task DownloadEpubFromGcs(string objectPath, string destinationPath)
  {
    await using var outputFile = System.IO.File.OpenWrite(destinationPath);
    await storageClient.DownloadObjectAsync(_bucketName, objectPath, outputFile);
  }

  private async Task CreateEpubPreview(
    EpubBook epubBook,
    EpubLocalTextContentFile selectedChapter,
    string outputFilePath
  )
  {
    var tempExtractPath = Path.Combine(Path.GetTempPath(), "epub_extract");
    if (Directory.Exists(tempExtractPath)) Directory.Delete(tempExtractPath, true);
    Directory.CreateDirectory(tempExtractPath);

    // 1️⃣ Giải nén EPUB gốc
    if (epubBook.FilePath != null) ZipFile.ExtractToDirectory(epubBook.FilePath, tempExtractPath);

    // 2️⃣ Xóa các chapter không cần thiết
    string contentFolder = Path.Combine(tempExtractPath, "OEBPS");
    var files = Directory.GetFiles(contentFolder, "*.xhtml", SearchOption.AllDirectories);
    foreach (var file in files)
    {
      if (!file.EndsWith(selectedChapter.GetDisplayName())) { System.IO.File.Delete(file); }
    }

    // 3️⃣ Tạo EPUB mới chỉ chứa 1 chapter
    if (System.IO.File.Exists(outputFilePath)) System.IO.File.Delete(outputFilePath);
    ZipFile.CreateFromDirectory(tempExtractPath, outputFilePath);
    await Task.CompletedTask;
  }
}