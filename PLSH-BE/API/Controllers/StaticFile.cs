using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Storage.V1;
using System.IO;

namespace API.Controllers;

[Route("file")]
[ApiController]
[Route("static/v1")]
public class FileController(StorageClient storageClient) : ControllerBase
{
  private readonly string? _bucketName = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_BUCKET");

  [HttpGet("file/{*path}")] public async Task<IActionResult> GetFile(string path)
  {
    if (string.IsNullOrEmpty(path)) return BadRequest("File path is required.");
    try
    {
      var filePath = path.Replace("%2F", "/");
      if (!string.IsNullOrEmpty(filePath) && !filePath.StartsWith("/")) { filePath = $"/{filePath}"; }

      MemoryStream stream = new MemoryStream();
      await storageClient.DownloadObjectAsync(_bucketName, filePath, stream);
      stream.Position = 0;
      string contentType = GetContentType(filePath);

      // Đặt header Content-Disposition thành "inline"
      Response.Headers["Content-Disposition"] = $"inline; filename={Path.GetFileName(filePath)}";
      return File(stream, contentType);
    }
    catch { return NotFound("File not found."); }
  }

  // private string GetContentType(string fileName)
  // {
  //   var extension = Path.GetExtension(fileName).ToLower();
  //   return extension switch
  //   {
  //     ".jpg" or ".jpeg" => "image/jpeg",
  //     ".png" => "image/png",
  //     ".pdf" => "application/pdf",
  //     ".txt" => "text/plain",
  //     ".mp4" => "video/mp4",
  //     _ => "application/octet-stream"
  //   };
  // }
  private string GetContentType(string filePath)
  {
    var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
    if (!provider.TryGetContentType(filePath, out var contentType)) { contentType = "application/octet-stream"; }

    return contentType;
  }
}