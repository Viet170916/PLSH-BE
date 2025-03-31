using System.IO;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;

namespace API.Common;

public class GoogleCloudStorageHelper(StorageClient storageClient)
{
  private readonly string? _bucketName = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_BUCKET");

  public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderPath, string contentType)
  {
    if (fileStream == null || string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(folderPath))
      throw new ArgumentException("Invalid input parameters.");
    var objectName = $"{folderPath.TrimEnd('/')}/{Guid.NewGuid()}_{fileName}";
    await storageClient.UploadObjectAsync(_bucketName, objectName, contentType, fileStream);
    return $"{objectName}";
  }

  public async Task<string> UploadFileAsync(Stream fileStream, string pathTofFle, string contentType)
  {
    if (fileStream == null || string.IsNullOrEmpty(pathTofFle))
      throw new ArgumentException("Invalid input parameters.");
    var objectName = $"{pathTofFle}";
    await storageClient.UploadObjectAsync(_bucketName, objectName, contentType, fileStream);
    return $"{objectName}";
  }

  public async Task DownloadEpubFromGcs(string objectPath, string destinationPath)
  {
    await using var outputFile = File.OpenWrite(destinationPath);
    await storageClient.DownloadObjectAsync(_bucketName, objectPath, outputFile);
  }

  public async Task<bool> FileExistsAsync(string filePath)
  {
    try
    {
      var obj = await storageClient.GetObjectAsync(_bucketName, filePath);
      return obj != null;
    }
    catch (Google.GoogleApiException e) when (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound) { return false; }
  }

  public async Task<FileStreamResult> GetFileStreamAsync(string filePath)
  {
    var stream = new MemoryStream();
    await storageClient.DownloadObjectAsync(_bucketName, filePath, stream);
    stream.Position = 0;
    return new FileStreamResult(stream, "audio/mpeg");
  }
}
