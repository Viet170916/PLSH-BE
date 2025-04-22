using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BU.Services.Interface;

public interface IEpubParserService
{
  Task ParseAndSaveEpubFromFileAsync(IFormFile epubFile, int bookId);

  Task<int> GetTotalChapter(
    int bookId
  );

  Task<List<TDto>> GetChaptersAsync<TDto>(
    int bookId,
    int userId,
    bool fullAccess = false,
    int? chapterIndex = null
  );
}
