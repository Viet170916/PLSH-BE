using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BU.Models.DTO;
using BU.Services.Interface;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Http;
using Model.Entity.Book.e_book;
using Microsoft.EntityFrameworkCore;

namespace BU.Services.Implementation;

public partial class EpubParserService(AppDbContext dbContext, IMapper mapper) : IEpubParserService
{
  public async Task ParseAndSaveEpubFromFileAsync(IFormFile epubFile, int bookId)
  {
    try
    {
      await using var fileStream = epubFile.OpenReadStream();
      var book = await TryReadBookWithVersOne(fileStream);
      if (book != null) { await SaveChaptersToDatabase(book.ReadingOrder, bookId); }
      else { await ProcessEpubAsZip(epubFile, bookId); }
    }
    catch (Exception ex) { throw new InvalidOperationException($"Error processing EPUB: {ex.Message}", ex); }
  }

  public async Task<int> GetTotalChapter(int bookId)
  {
    var allChaptersCount = await dbContext.EBookChapters
                                          .Where(c => c.BookId == bookId)
                                          .CountAsync();
    return allChaptersCount;
  }

  public async Task<List<TDto>> GetChaptersAsync<TDto>(
    int bookId,
    int userId,
    bool fullAccess = false,
    int? chapterIndex = null
  )
  {
    // var hasAccess = await dbContext.ResourceAccesses
    //                                .AnyAsync(a =>
    //                                  a.BookId == bookId && a.AccountId == userId && a.ExpireDate > DateTime.UtcNow);
    var query = dbContext.EBookChapters
                         .Where(c => c.BookId == bookId)
                         .OrderBy(c => c.ChapterIndex)
                         .AsQueryable();
    // if (hasAccess || fullAccess)
    // {
      if (chapterIndex.HasValue) { query = query.Where(c => c.ChapterIndex == chapterIndex.Value); }

      return await query.ProjectTo<TDto>(mapper.ConfigurationProvider).ToListAsync();
    // }

    // var allChapters = await query.ToListAsync();
    // var result = new List<TDto>();
    // var totalWords = 0;
    // foreach (var chapter in allChapters)
    // {
    //   if (totalWords >= 1000) break;
    //   var words = chapter.PlainText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    //   var remaining = 1000 - totalWords;
    //   var content = string.Join(' ', words.Take(remaining));
    //   totalWords += Math.Min(words.Length, remaining);
    //   var tempChapter = new EBookChapter
    //   {
    //     Id = chapter.Id,
    //     BookId = chapter.BookId,
    //     ChapterIndex = chapter.ChapterIndex,
    //     FileName = chapter.FileName,
    //     Title = chapter.Title,
    //     HtmlContent = chapter.HtmlContent,
    //     PlainText = content,
    //   };
    //   result.Add(mapper.Map<TDto>(tempChapter));
    // }
    //
    // return [result.LastOrDefault(),];
  }
}
