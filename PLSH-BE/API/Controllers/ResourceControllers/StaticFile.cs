using System.IO;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using VersOne.Epub;
using Common.Infrastructure.Extensions;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using API.Common;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Model.Entity;
using Model.Entity.book;
using Newtonsoft.Json;

namespace API.Controllers.ResourceControllers;

[Authorize("LibrarianPolicy")]
[ApiController]
[Route("static/v1")]
public class FileController(
  StorageClient storageClient,
  AppDbContext context,
  GoogleCloudStorageHelper googleCloudStorageHelper,
  ILogger<FileController> logger
) : ControllerBase
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
      var tempEpubPath = Path.Combine(Path.GetTempPath(), "Book.epub");
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

  [HttpGet("book/{bookId}/text")]
  public async Task<IActionResult> GetEbupText([FromRoute] int bookId, [FromQuery] int chapter = 1)
  {
    var book = await context.Books.Include(b => b.EpubResource).FirstOrDefaultAsync(b => bookId == b.Id);
    if (book is null) return NotFound(new { message = "Book not found." });
    if (book.EpubResource is null) return NotFound(new { message = "Book Preview Resource not found." });
    if (book.EpubResource?.LocalUrl is null) return NotFound(new { message = "Book Preview Resource not found." });
    try
    {
      var tempEpubPath = Path.Combine(Path.GetTempPath(), "Book.epub");
      var path = book.EpubResource.LocalUrl;
      await DownloadEpubFromGcs(path, tempEpubPath);
      var epubBook = await EpubReader.ReadBookAsync(tempEpubPath);
      var allChapters = epubBook.ReadingOrder.ToList();
      if (chapter < 1 || chapter > allChapters.Count) { return BadRequest(new { message = "Chapter không hợp lệ." }); }

      var selectedChapter = allChapters[chapter - 1];
      var chapterHtml = selectedChapter.Content;
      var htmlDoc = new HtmlDocument();
      htmlDoc.LoadHtml(chapterHtml);
      var textContent = htmlDoc.DocumentNode.InnerText;
      return Content(textContent, "text/plain");
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
    var tempExtractPath = Path.Combine(Path.GetTempPath(), "epubextract");
    if (Directory.Exists(tempExtractPath)) Directory.Delete(tempExtractPath, true);
    Directory.CreateDirectory(tempExtractPath);
    if (epubBook.FilePath != null) ZipFile.ExtractToDirectory(epubBook.FilePath, tempExtractPath);
    string contentFolder = Path.Combine(tempExtractPath, "OEBPS");
    var files = Directory.GetFiles(contentFolder, "*.xhtml", SearchOption.AllDirectories);
    foreach (var file in files)
    {
      if (!file.EndsWith(selectedChapter.GetDisplayName())) { System.IO.File.Delete(file); }
    }

    if (System.IO.File.Exists(outputFilePath)) System.IO.File.Delete(outputFilePath);
    ZipFile.CreateFromDirectory(tempExtractPath, outputFilePath);
    await Task.CompletedTask;
  }

  [HttpGet("book/{bookId}/audio")]
  public async Task<IActionResult> GetEpubAudio([FromRoute] int bookId, [FromQuery] int chapter = 1)
  {
    var existingAudio = await context.AudioBooks.Include(a => a.AudioFile)
                                     .FirstOrDefaultAsync(a => a.BookId == bookId && a.Chapter == chapter);
    if (existingAudio != null && existingAudio.AudioFile?.LocalUrl != null)
    {
      return await googleCloudStorageHelper.GetFileStreamAsync(existingAudio.AudioFile.LocalUrl);
    }

    var book = await context.Books.Include(b => b.EpubResource).FirstOrDefaultAsync(b => bookId == b.Id);
    if (book is null || book.EpubResource?.LocalUrl is null)
    {
      return NotFound(new { message = "Book or EPUB resource not found." });
    }

    var tempEpubPath = Path.Combine(Path.GetTempPath(), "Book.epub");
    await DownloadEpubFromGcs(book.EpubResource.LocalUrl, tempEpubPath);
    var epubBook = await EpubReader.ReadBookAsync(tempEpubPath);
    var allChapters = epubBook.ReadingOrder.ToList();
    if (chapter < 1 || chapter > allChapters.Count) { return BadRequest(new { message = "Invalid chapter." }); }

    var selectedChapter = allChapters[chapter - 1];
    var htmlDoc = new HtmlDocument();
    htmlDoc.LoadHtml(selectedChapter.Content);
    var textContent = htmlDoc.DocumentNode.InnerText;
    string gcsAudioUrl = await GenerateLongTtsAudio(textContent, bookId, chapter);
    if (string.IsNullOrEmpty(gcsAudioUrl)) { return StatusCode(500, new { message = "Error generating audio." }); }

    var resource = new Resource
    {
      Type = "audio", Name = $"Book {bookId} - Chapter {chapter}", FileType = "mp3", LocalUrl = gcsAudioUrl
    };
    context.Resources.Add(resource);
    await context.SaveChangesAsync();
    var audioBook = new AudioBook
    {
      BookId = bookId,
      Chapter = chapter,
      IsAvailable = true,
      AudioResourceId = resource.Id,
      AudioFile = resource
    };
    context.AudioBooks.Add(audioBook);
    await context.SaveChangesAsync();
    return Redirect(gcsAudioUrl);
  }

  private async Task<string> GenerateLongTtsAudio(string text, int bookId, int chapter)
  {
    var client = new HttpClient();
    var requestBody = new
    {
      input = new { text },
      voice = new { languageCode = "en-US", ssmlGender = "NEUTRAL" },
      audioConfig = new { audioEncoding = "MP3" },
      outputConfig = new
      {
        gcsUri = $"gs://{_bucketName}/{StaticFolder.DIRPath_BOOK_AUDIO}/book_{bookId}/chapter_{chapter}.mp3"
      }
    };
    var response = await client.PostAsync(
      $"https://texttospeech.googleapis.com/v1beta1/text:synthesizeLongAudio?key={Environment.GetEnvironmentVariable("GOOGLE_API_KEY")}",
      new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));
    logger.LogError(await response?.Content.ReadAsStringAsync());
    if (!response.IsSuccessStatusCode) return null;
    return requestBody.outputConfig.gcsUri;
  }
}
