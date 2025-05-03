using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Model.Entity.Book.e_book;
using VersOne.Epub;

namespace BU.Services.Implementation;

public partial class EpubParserService
{

  private static async Task<EpubBook> TryReadBookWithVersOne(Stream fileStream)
  {
    try { return await EpubReader.ReadBookAsync(fileStream); }
    catch (Exception) { return null; }
  }

  private async Task ProcessEpubAsZip(IFormFile epubFile, int bookId)
  {
    using var memoryStream = new MemoryStream();
    await epubFile.CopyToAsync(memoryStream);
    memoryStream.Seek(0, SeekOrigin.Begin);
    using var archive = new ZipArchive(memoryStream);
    var tocEntry = archive.GetEntry("toc.ncx");
    if (tocEntry == null) { throw new InvalidOperationException("TOC file (toc.ncx) not found in EPUB."); }

    await using var tocStream = tocEntry.Open();
    var tocXml = XElement.Load(tocStream);
    var readingOrder = tocXml.Descendants("navPoint")
                             .Select(navPoint => new
                             {
                               Title = navPoint.Element("navLabel")?.Element("text")?.Value,
                               Content = navPoint.Element("content")?.Attribute("src")?.Value
                             });
    foreach (var (chapter, index) in readingOrder.Select((value, i) => (value, i)))
    {
      if (IsSkippable(chapter.Content, chapter.Title)) continue;
      var bookChapter = new EBookChapter
      {
        BookId = bookId,
        Title = chapter.Title,
        HtmlContent = chapter.Content,
        PlainText = StripHtmlTags(chapter.Content),
        ChapterIndex = index + 1,
      };
      dbContext.Add(bookChapter);
    }

    await dbContext.SaveChangesAsync();
  }

  private async Task SaveChaptersToDatabase(IEnumerable<EpubLocalTextContentFile> readingOrder, int bookId)
  {
    foreach (var (chapter, index) in readingOrder.Select((value, i) => (value, i)))
    {
      if (IsSkippable(chapter.FilePath, chapter.Content)) continue;
      var bookChapter = new EBookChapter
      {
        BookId = bookId,
        HtmlContent = chapter.Content,
        PlainText = StripHtmlTags(chapter.Content),
        ChapterIndex = index+1,
        FileName = chapter.FilePath,
      };
      dbContext.Add(bookChapter);
    }

    await dbContext.SaveChangesAsync();
  }

  private static bool IsSkippable(string fileName, string content)
  {
    return fileName.Contains("toc", StringComparison.OrdinalIgnoreCase) ||
           fileName.Contains("cover", StringComparison.OrdinalIgnoreCase) ||
           string.IsNullOrWhiteSpace(content);
  }

  private static string StripHtmlTags(string html) { return Regex.Replace(html, "<[^>]*>", string.Empty); }
}
