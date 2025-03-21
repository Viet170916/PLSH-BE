using System.IO;
using Google.Cloud.Storage.V1;

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
  public async Task DownloadEpubFromGcs(string objectPath, string destinationPath)
  {
    await using var outputFile = File.OpenWrite(destinationPath);
    await storageClient.DownloadObjectAsync(_bucketName, objectPath, outputFile);
  }


}