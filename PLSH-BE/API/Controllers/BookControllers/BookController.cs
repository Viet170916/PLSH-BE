using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using API.DTO.Book;
using AutoMapper;
using BU.Services.Interface;
using Common.Enums;
using Common.Helper;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Model.Entity.book;
using Model.ResponseModel;

namespace API.Controllers.BookControllers
{
  [Route("api/v1/book")]
  [ApiController]
  public partial class BookController(
    AppDbContext context,
    IAuthorService authorService,
    ILogger<BookController> logger,
    IHttpClientFactory httpClientFactory,
    IMapper mapper,
    IBookInstanceService bookInstanceService
  )
    : ControllerBase
  {
    [HttpPost("add")] public async Task<IActionResult> AddOrUpdateBook([FromForm] BookNewDto? bookDto)
    {
      logger.LogInformation($"{nameof(AddOrUpdateBook)} called.");
      try
      {
        if (bookDto is null)
        {
          return BadRequest(new ErrorResponse
          {
            Status = HttpStatus.BAD_REQUEST.GetDescription(),
            StatusCode = HttpStatus.BAD_REQUEST,
            Message = "Dữ liệu không hợp lệ.",
            Data = null,
          });
        }

        Book? book = null;
        if (bookDto.Id.HasValue)
        {
          book = await context.Books
                              .Include(b => b.Authors)
                              .FirstOrDefaultAsync(b => b.Id == bookDto.Id.Value);
        }

        if (book is null)
        {
          var exists = await context.Books.AnyAsync(b =>
            b.Title == bookDto.Title &&
            b.Version == bookDto.Version);
          if (exists)
          {
            return Conflict(new ErrorResponse
            {
              Status = HttpStatus.CONFLICT.GetDescription(),
              StatusCode = HttpStatus.CONFLICT,
              Message = "Sách đã tồn tại với cùng tiêu đề và phiên bản.",
              Data = null,
            });
          }

          book = new Book { CreateDate = DateTime.UtcNow, };
          context.Books.Add(book);
        }

        await bookInstanceService.AddBookInstancesIfNeeded(book.Id, bookDto.Quantity);
        IList<string> authorNames = new List<string>();
        if (bookDto.Authors is not null && bookDto.Authors.Any())
        {
          authorNames = bookDto.Authors.Select(a => a.FullName.Trim()).ToList();
          await authorService.AddIfAuthorsNameDoesntExistAsync(authorNames);
        }

        IList<Author> processedAuthors = new List<Author>();
        if (authorNames.Any())
        {
          processedAuthors = await context.Authors
                                          .Where(a => authorNames.Contains(a.FullName))
                                          .ToListAsync();
        }

        book.Title = bookDto.Title;
        book.Description = bookDto.Description;
        book.Authors = processedAuthors;
        book.Thumbnail = bookDto.Thumbnail;
        book.Kind = bookDto.Kind ?? 0;
        book.Version = bookDto.Version;
        book.Publisher = bookDto.Publisher;
        book.Width = bookDto.Width;
        book.Height = bookDto.Height;
        book.Thickness = bookDto.Thickness;
        book.Weight = bookDto.Weight;
        book.PublishDate = bookDto.PublishDate;
        book.Language = bookDto.Language;
        book.PageCount = bookDto.PageCount ?? 0;
        book.IsbNumber13 = bookDto.IsbnNumber13;
        book.IsbNumber10 = bookDto.IsbnNumber10;
        book.OtherIdentifier = bookDto.OtherIdentifier;
        book.Price = bookDto.Price;
        book.TotalCopies = bookDto.TotalCopies ?? 0;
        book.AvailableCopies = bookDto.AvailableCopies ?? 0;
        book.UpdateDate = DateTime.UtcNow;
        if (bookDto.CategoryId is not null || (bookDto.Category?.Id is not null && bookDto.Category.Id != 0))
        {
          book.CategoryId = bookDto.CategoryId ?? bookDto.Category?.Id;
        }
        else
          if (bookDto.Category is not null) { book.Category = new Category { Name = bookDto.Category.Name }; }

        await context.SaveChangesAsync();
        var bookAdded = mapper.Map<BookDto>(book);
        return Ok(bookAdded);
      }
      catch (Exception ex)
      {
        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
        {
          Status = HttpStatusCode.InternalServerError.ToString(),
          StatusCode = HttpStatus.INTERNAL_ERROR,
          Message = "Lỗi máy chủ nội bộ.",
          Data = new { Error = ex.Message },
        });
      }
    }
  }
}