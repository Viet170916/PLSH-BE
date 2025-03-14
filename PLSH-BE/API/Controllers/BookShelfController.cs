using Data.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using Model.Entity;
using Model.Entity.LibraryRoom;
using OfficeOpenXml;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookShelfController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookShelfController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách kệ sách
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<Bookshelf>>> GetBookshelves()
        // {
        //     return await _context.Bookshelves.ToListAsync();
        // }
        //
        // // 2. Lấy danh sách sách trong một kệ
        // [HttpGet("{bookshelfId}/books")]
        // public async Task<ActionResult<IEnumerable<Book>>> GetBooksInShelf(int bookshelfId)
        // {
        //     //var books = await _context.BookLocations
        //     //    .Where(bl => bl.BookshelfId == bookshelfId)
        //     //   // .Select(bl => bl.Book)
        //     //    .ToListAsync();
        //
        //     return Ok();
        // }
        //
        // // 3. Thêm kệ sách
        // [HttpPost]
        // public async Task<ActionResult<Bookshelf>> CreateBookshelf(Bookshelf bookshelf)
        // {
        //     _context.Bookshelves.Add(bookshelf);
        //     await _context.SaveChangesAsync();
        //     return CreatedAtAction(nameof(GetBookshelves), new { id = bookshelf.Id }, bookshelf);
        // }
        //
        // // 4. Cập nhật thông tin kệ sách
        // [HttpPut("{id}")]
        // public async Task<IActionResult> UpdateBookshelf(int id, Bookshelf bookshelf)
        // {
        //     if (id != bookshelf.Id)
        //         return BadRequest();
        //
        //     _context.Entry(bookshelf).State = EntityState.Modified;
        //     await _context.SaveChangesAsync();
        //     return NoContent();
        // }
        //
        // // 5. Xóa kệ sách
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteBookshelf(int id)
        // {
        //     var bookshelf = await _context.Bookshelves.FindAsync(id);
        //     if (bookshelf == null)
        //         return NotFound();
        //
        //     _context.Bookshelves.Remove(bookshelf);
        //     await _context.SaveChangesAsync();
        //     return NoContent();
        // }
        //
        // // 6. Thêm sách vào kệ
        // [HttpPost("{bookshelfId}/books/{bookId}")]
        // public async Task<IActionResult> AddBookToShelf(int bookshelfId, int bookId)
        // {
        //     var bookLocation = new BookLocation { BookshelfId = bookshelfId, BookId = bookId };
        //     _context.BookLocations.Add(bookLocation);
        //     await _context.SaveChangesAsync();
        //     return Ok(bookLocation);
        // }
        //
        // // 7. Di chuyển sách giữa các kệ
        // [HttpPut("{bookId}/move/{newShelfId}")]
        // public async Task<IActionResult> MoveBookToShelf(int bookId, int newShelfId)
        // {
        //     var bookLocation = await _context.BookLocations.FirstOrDefaultAsync(bl => bl.BookId == bookId);
        //     if (bookLocation == null)
        //         return NotFound();
        //
        //     bookLocation.BookshelfId = newShelfId;
        //     await _context.SaveChangesAsync();
        //     return NoContent();
        // }
        //
        // // 8. Xóa sách khỏi kệ
        // [HttpDelete("{bookshelfId}/books/{bookId}")]
        // public async Task<IActionResult> RemoveBookFromShelf(int bookshelfId, int bookId)
        // {
        //     var bookLocation = await _context.BookLocations
        //         .FirstOrDefaultAsync(bl => bl.BookshelfId == bookshelfId && bl.BookId == bookId);
        //
        //     if (bookLocation == null)
        //         return NotFound();
        //
        //     _context.BookLocations.Remove(bookLocation);
        //     await _context.SaveChangesAsync();
        //     return NoContent();
        // }
        //
        // // 9. Thống kê sách của các kệ và vị trí đặt sách
        // [HttpGet("statistics")]
        // public async Task<ActionResult<IEnumerable<object>>> GetBookshelfStatistics()
        // {
        //     // var statistics = await _context.Bookshelves
        //     //     .Select(bs => new
        //     //     {
        //     //         BookshelfId = bs.Id,
        //     //         BookshelfName = bs.Name,
        //     //         TotalBooks = _context.BookLocations.Count(bl => bl.BookshelfId == bs.Id),
        //     //         BookPositions = _context.BookLocations
        //     //             .Where(bl => bl.BookshelfId == bs.Id)
        //     //             .Select(bl => new { bl.BookId, bl.Position })
        //     //             .ToList()
        //     //     })
        //     //     .ToListAsync();
        //
        //     return Ok();
        // }
        //
        // // 10. Gợi ý vị trí đặt sách khi trả vào kệ
        // [HttpGet("{bookId}/suggest-location")]
        // public async Task<ActionResult<object>> SuggestBookLocation(int bookId)
        // {
        //     var lastLocation = await _context.BookLocations
        //         .Where(bl => bl.BookId == bookId)
        //         .OrderByDescending(bl => bl.Id)
        //         .Select(bl => new { bl.BookshelfId, bl.Position })
        //         .FirstOrDefaultAsync();
        //
        //     if (lastLocation != null)
        //     {
        //         return Ok(new { SuggestedShelfId = lastLocation.BookshelfId, SuggestedPosition = lastLocation.Position });
        //     }
        //     else
        //     {
        //         return NotFound("No previous location found for this book.");
        //     }
        // }
        //
        // // 11. Xuất danh sách sách của các kệ ra file Excel
        // [HttpGet("export-books-excel")]
        // public async Task<IActionResult> ExportBooksInShelvesToExcel()
        // {
        //     var booksInShelves = await _context.BookLocations
        //         .Select(bl => new
        //         {
        //            // BookshelfId = bl.Bookshelf.Id,
        //             //BookshelfName = bl.Bookshelf.Name,
        //            // BookId = bl.Book.Id,
        //            // BookTitle = bl.Book.Title,
        //            // Author = bl.Book.Author,
        //            // Publisher = bl.Book.Publisher,
        //            // PublishYear = bl.Book.PublishDate,
        //            //// Category = bl.Book.Category,
        //            // AvailableCopies = bl.Book.AvailableCopies
        //         })
        //         .ToListAsync();
        //
        //     using var package = new ExcelPackage();
        //     var worksheet = package.Workbook.Worksheets.Add("Books In Shelves");
        //
        //     string[] headers = { "ID Kệ", "Tên Kệ", "ID Sách", "Tên Sách", "Tác Giả", "Nhà Xuất Bản", "Thời gian Xuất Bản", "Thể Loại", "Trạng thái" };
        //     for (int col = 0; col < headers.Length; col++)
        //     {
        //         worksheet.Cells[1, col + 1].Value = headers[col];
        //     }
        //
        //     for (int i = 0; i < booksInShelves.Count; i++)
        //     {
        //         var book = booksInShelves[i];
        //        // worksheet.Cells[i + 2, 1].Value = book.BookshelfId;
        //         //worksheet.Cells[i + 2, 2].Value = book.BookshelfName;
        //         //worksheet.Cells[i + 2, 3].Value = book.BookId;
        //         //worksheet.Cells[i + 2, 4].Value = book.BookTitle;
        //         //worksheet.Cells[i + 2, 5].Value = book.Author;
        //         //worksheet.Cells[i + 2, 6].Value = book.Publisher;
        //         //worksheet.Cells[i + 2, 7].Value = book.PublishYear;
        //         ////worksheet.Cells[i + 2, 8].Value = book.Category;
        //         //worksheet.Cells[i + 2, 9].Value = book.AvailableCopies;
        //     }
        //
        //     var stream = new MemoryStream();
        //     package.SaveAs(stream);
        //     stream.Position = 0;
        //
        //     return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BooksInShelves.xlsx");
        // }


        //[HttpGet("{bookshelfId}/books")]
        //public async Task<ActionResult<IEnumerable<Book>>> GetBooksInShelf(
        //    int bookshelfId,
        //    [FromQuery] string? search,
        //    [FromQuery] string? author,
        //    [FromQuery] int? publishYear,
        //    [FromQuery] string? category,
        //    [FromQuery] int page = 1,
        //    [FromQuery] int pageSize = 10)
        //{
        //    var query = _context.BookLocations
        //        .Where(bl => bl.BookshelfId == bookshelfId)
        //        .Include(bl => bl.Book)
        //            .ThenInclude(b => b.Category)
        //        .Select(bl => bl.Book)
        //        .AsQueryable();

        //    // Tìm kiếm theo tên sách
        //    if (!string.IsNullOrEmpty(search))
        //    {
        //        query = query.Where(b => b.Title.Contains(search));
        //    }

        //    // Lọc theo tác giả
        //    if (!string.IsNullOrEmpty(author))
        //    {
        //        query = query.Where(b => b.Author == author);
        //    }
          

        //    // Lọc theo thể loại
        //    if (!string.IsNullOrEmpty(category))
        //    {
        //        query = query.Where(b => b.Category.Name == category);
        //    }

        //    // Tổng số sách sau khi lọc
        //    int totalBooks = await query.CountAsync();

        //    // Phân trang
        //    var books = await query
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //    return Ok(new
        //    {
        //        TotalBooks = totalBooks,
        //        Page = page,
        //        PageSize = pageSize,
        //        Books = books
        //    });
        //}

    }
}
