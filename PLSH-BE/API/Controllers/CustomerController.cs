// using System.Diagnostics;
// using System.Drawing;
// using System.IO;
// using API.DTO.Book;
// using API.DTO.BookReview;
// using Common.Enums;
// using Common.Helper;
// using Data.DatabaseContext;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using Model.Entity;
// using Model.ResponseModel;
//
// namespace API.Controllers;
//
// [Route("api/[controller]")]
// [ApiController]
// public class CustomerController : ControllerBase
// {
//   private readonly AppDbContext _context;
//   private readonly ILogger<ManageAccountController> _logger;
//   private readonly GenAIService _genAIService;
//
//   public CustomerController(AppDbContext context, ILogger<ManageAccountController> logger, GenAIService genAIService)
//   {
//     _context = context;
//     _logger = logger;
//     _genAIService = genAIService;
//   }
//
//   // 1. Tìm kiếm sách theo từ khóa
//   [HttpGet("search")] public async Task<IActionResult> SearchBooks([FromQuery] string keyword)
//   {
//     try
//     {
//       if (string.IsNullOrWhiteSpace(keyword))
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.BAD_REQUEST.GetDescription(),
//           StatusCode = HttpStatus.BAD_REQUEST,
//           Message = "Keyword cannot be empty."
//         });
//       }
//
//       var bookEntities = await (
//         from b in _context.Books
//         join c in _context.Categories on b.CategoryId equals c.Id
//         where b.Title.Contains(keyword) //|| b.Author.Contains(keyword)
//         select new BookDto
//         {
//           Id = b.Id,
//           Title = b.Title,
//           // Author = b.Author,
//           Description = b.Description,
//           PublishDate = b.PublishDate,
//           Language = b.Language,
//           Thumbnail = b.Thumbnail,
//           Category = new CategoryDto() { Id = c.Id, Name = c.Name }
//         }).ToListAsync();
//       if (bookEntities == null || !bookEntities.Any())
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.NOT_FOUND.GetDescription(),
//           StatusCode = HttpStatus.NOT_FOUND,
//           Message = "No books found matching the search criteria."
//         });
//       }
//
//       return Ok(new OkResponse
//       {
//         Status = HttpStatus.OK.GetDescription(),
//         StatusCode = HttpStatus.OK,
//         Message = "Search successful.",
//         Data = bookEntities
//       });
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError(ex, "Error occurred while searching books.");
//       return Ok(new ErrorResponse
//       {
//         Status = HttpStatus.INTERNAL_ERROR.GetDescription(),
//         StatusCode = HttpStatus.INTERNAL_ERROR,
//         Message = "An error occurred while processing your request."
//       });
//     }
//   }
//
//   //Tìm kiếm sách bằng giọng nói
//   //[HttpPost("search-by-voice")]
//   //public async task<iactionresult> searchbyvoice([fromform] iformfile audiofile)
//   //{
//   //    try
//   //    {
//   //        if (audiofile == null || audiofile.length == 0)
//   //        {
//   //            return badrequest("file âm thanh không hợp lệ.");
//   //        }
//
//   //        var model = new model(path.combine(directory.getcurrentdirectory(), "wwwroot", "models", "vosk-model-small-vn-0.4"));
//   //        using var memorystream = new memorystream();
//   //        await audiofile.copytoasync(memorystream);
//   //        memorystream.seek(0, seekorigin.begin);
//
//   //        using var wavereader = new wavefilereader(memorystream);
//   //        var rec = new voskrecognizer(model, wavereader.waveformat.samplerate);
//   //        byte[] buffer = new byte[4096];
//   //        int bytesread;
//
//   //        while ((bytesread = wavereader.read(buffer, 0, buffer.length)) > 0)
//   //        {
//   //            rec.acceptwaveform(buffer, bytesread);
//   //        }
//
//   //        var result = rec.finalresult();
//   //        var searchquery = result.split('"')[3];
//
//   //        if (string.isnullorempty(searchquery))
//   //        {
//   //            return notfound("không nhận dạng được nội dung giọng nói.");
//   //        }
//
//   //        _logger.loginformation($"giọng nói nhận được: {searchquery}");
//   //        var books = await _context.books
//   //            .where(b => b.title.contains(searchquery) || b.author.contains(searchquery))
//   //            .tolistasync();
//
//   //        if (books.any())
//   //        {
//   //            _logger.loginformation("tìm thấy sách phù hợp.");
//   //            return ok(books);
//   //        }
//   //        else
//   //        {
//   //            _logger.loginformation("không tìm thấy sách phù hợp.");
//   //            return notfound("không tìm thấy sách phù hợp.");
//   //        }
//   //    }
//   //    catch (exception ex)
//   //    {
//   //        _logger.logerror($"lỗi: {ex.message}");
//   //        return statuscode(500, "đã xảy ra lỗi trong quá trình nhận dạng giọng nói.");
//   //    }
//   //}
//
//   //  2	Tìm kiếm nâng cao
//   [HttpGet("advanced-search")] [AllowAnonymous]
//   public async Task<IActionResult> AdvancedSearch(
//     [FromQuery] string? title,
//     [FromQuery] string? author,
//     [FromQuery] string? category,
//     [FromQuery] string? language,
//     [FromQuery] int? year
//   )
//   {
//     try
//     {
//       var query = _context.Books.AsQueryable();
//       if (!string.IsNullOrWhiteSpace(title)) query = query.Where(b => b.Title.Contains(title));
//
//       //if (!string.IsNullOrWhiteSpace(author))
//       //    query = query.Where(b => b.Author.Contains(author));
//       if (!string.IsNullOrWhiteSpace(category))
//         query = query.Join(_context.Categories,
//                        book => book.CategoryId,
//                        cat => cat.Id,
//                        (book, cat) => new { book, cat })
//                      .Where(bc => bc.cat.Name.Contains(category))
//                      .Select(bc => bc.book);
//       if (!string.IsNullOrWhiteSpace(language)) query = query.Where(b => b.Language.Contains(language));
//       if (year.HasValue) query = query.Where(b => b.PublishDate.HasValue && b.PublishDate.Value.Year == year.Value);
//       var books = await query.ToListAsync();
//       if (books == null || !books.Any())
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.NOT_FOUND.GetDescription(),
//           StatusCode = HttpStatus.NOT_FOUND,
//           Message = "No books found matching the search criteria.",
//         });
//       }
//
//       return Ok(new OkResponse
//       {
//         Status = HttpStatus.OK.GetDescription(),
//         StatusCode = HttpStatus.OK,
//         Message = "Advanced search successful.",
//         Data = books
//       });
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError(ex, "Error occurred during advanced search.");
//       return Ok(new ErrorResponse
//       {
//         Status = HttpStatus.INTERNAL_ERROR.GetDescription(),
//         StatusCode = HttpStatus.INTERNAL_ERROR,
//         Message = "An error occurred while processing your request.",
//       });
//     }
//   }
//
//   //3	Tra cứu bằng QR code(Quét mã QR trên kệ sách hiển thị thông tin chi tiết sách, công nghệ: QRCode)
//   [HttpPost("book-by-qrcode")] [AllowAnonymous]
//   public async Task<IActionResult> GetBookByQRCode([FromForm] IFormFile qrImage)
//   {
//     try
//     {
//       if (qrImage == null || qrImage.Length == 0)
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.BAD_REQUEST.GetDescription(),
//           StatusCode = HttpStatus.BAD_REQUEST,
//           Message = "QR code image is required.",
//         });
//       }
//
//       using var stream = qrImage.OpenReadStream();
//       var bitmap = new Bitmap(stream);
//       var reader = new ZXing.BarcodeReader();
//       var result = reader.Decode(bitmap);
//       if (result == null)
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.BAD_REQUEST.GetDescription(),
//           StatusCode = HttpStatus.BAD_REQUEST,
//           Message = "Invalid QR code image.",
//         });
//       }
//
//       var qrCode = result.Text;
//       //var book = await _context.Books
//       //    .FirstOrDefaultAsync(b => b.QRCode == qrCode);
//
//       //if (book == null)
//       //{
//       //    return Ok(new ErrorResponse
//       //    {
//       //        Status = HttpStatus.NOT_FOUND.GetDescription(),
//       //        StatusCode = HttpStatus.NOT_FOUND,
//       //        Message = "No book found with this QR code.",
//       //    });
//       //}
//       return Ok(new OkResponse
//       {
//         Status = HttpStatus.OK.GetDescription(),
//         StatusCode = HttpStatus.OK,
//         Message = "Book retrieved successfully by QR code.",
//         // Data = book
//       });
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError(ex, "Error occurred while retrieving book by QR code.");
//       return Ok(new ErrorResponse
//       {
//         Status = HttpStatus.INTERNAL_ERROR.GetDescription(),
//         StatusCode = HttpStatus.INTERNAL_ERROR,
//         Message = "An error occurred while processing your request.",
//       });
//     }
//   }
//
//   // 3. Tra cứu sách bằng QR Code từ hình ảnh
//   //[HttpPost("lookup-qrcode-image")]
//   //public async Task<IActionResult> LookupQRCodeImage([FromForm] IFormFile imageFile)
//   //{
//   //    try
//   //    {
//   //        if (imageFile == null || imageFile.Length == 0)
//   //        {
//   //            return Ok(new ErrorResponse
//   //            {
//   //                Status = HttpStatus.BAD_REQUEST.GetDescription(),
//   //                StatusCode = HttpStatus.BAD_REQUEST,
//   //                Message = "Image file cannot be empty.",
//   //            });
//   //        }
//
//   //        // Đọc hình ảnh bằng ImageSharp
//   //        using var image = await Image.LoadAsync<Rgba32>(imageFile.OpenReadStream());
//
//   //        // Chuyển ImageSharp.Image sang Bitmap
//   //        using var memoryStream = new MemoryStream();
//   //        image.SaveAsBmp(memoryStream);
//   //        memoryStream.Seek(0, SeekOrigin.Begin);
//   //        var bitmap = new System.Drawing.Bitmap(memoryStream);
//
//   //        // Đọc QR Code
//   //        var reader = new ZXing.BarcodeReader();
//   //        var result = reader.Decode(bitmap);
//
//   //        if (result == null)
//   //        {
//   //            return Ok(new ErrorResponse
//   //            {
//   //                Status = HttpStatus.NOT_FOUND.GetDescription(),
//   //                StatusCode = HttpStatus.NOT_FOUND,
//   //                Message = "No QR Code found in the image.",
//   //            });
//   //        }
//
//   //        string qrCode = result.Text;
//
//   //        // Tìm sách theo QR Code
//   //        var book = await _context.Books.FirstOrDefaultAsync(b => b.QRCode == qrCode);
//   //        if (book == null)
//   //        {
//   //            return Ok(new ErrorResponse
//   //            {
//   //                Status = HttpStatus.NOT_FOUND.GetDescription(),
//   //                StatusCode = HttpStatus.NOT_FOUND,
//   //                Message = "Book not found with the provided QR Code.",
//   //            });
//   //        }
//
//   //        // Trả về thông tin sách nếu tìm thấy
//   //        return Ok(new OkResponse
//   //        {
//   //            Status = HttpStatus.OK.GetDescription(),
//   //            StatusCode = HttpStatus.OK,
//   //            Message = "Book lookup successful.",
//   //            Data = new
//   //            {
//   //                Id = book.Id,
//   //                Title = book.Title,
//   //                Author = book.Author,
//   //                Description = book.Description,
//   //                PublishDate = book.PublishDate,
//   //                Language = book.Language,
//   //                Thumbnail = book.Thumbnail
//   //            }
//   //        });
//   //    }
//   //    catch (Exception ex)
//   //    {
//   //        _logger.LogError(ex, "Error occurred while looking up QR Code from image.");
//   //        return Ok(new ErrorResponse
//   //        {
//   //            Status = HttpStatus.INTERNAL_ERROR.GetDescription(),
//   //            StatusCode = HttpStatus.INTERNAL_ERROR,
//   //            Message = "An error occurred while processing your request.",
//   //        });
//   //    }
//   //}
//
//   //4	Xem thông tin sách
//   [HttpGet("book/{id}")] public async Task<IActionResult> GetBookDetails(int id)
//   {
//     try
//     {
//       var book = await _context.Books.FindAsync(id);
//       if (book == null)
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.NOT_FOUND.GetDescription(),
//           StatusCode = HttpStatus.NOT_FOUND,
//           Message = "Book not found.",
//         });
//       }
//
//       return Ok(new OkResponse
//       {
//         Status = HttpStatus.OK.GetDescription(),
//         StatusCode = HttpStatus.OK,
//         Message = "Book details retrieved successfully.",
//         Data = book
//       });
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError(ex, "Error occurred while retrieving book details.");
//       return Ok(new ErrorResponse
//       {
//         Status = HttpStatus.INTERNAL_ERROR.GetDescription(),
//         StatusCode = HttpStatus.INTERNAL_ERROR,
//         Message = "An error occurred while processing your request.",
//       });
//     }
//   }
//
//   //5	Xem sách cùng tác giả
//   //[HttpGet("books-by-author/{author}")]
//   //public async Task<IActionResult> GetBooksByAuthor(string author)
//   //{
//   //    try
//   //    {
//   //        var books = await _context.Books.Where(b => b.Author == author).ToListAsync();
//   //        if (!books.Any())
//   //        {
//   //            return Ok(new ErrorResponse
//   //            {
//   //                Status = HttpStatus.NOT_FOUND.GetDescription(),
//   //                StatusCode = HttpStatus.NOT_FOUND,
//   //                Message = "No books found by this author.",
//   //            });
//   //        }
//
//   //        return Ok(new OkResponse
//   //        {
//   //            Status = HttpStatus.OK.GetDescription(),
//   //            StatusCode = HttpStatus.OK,
//   //            Message = "Books by author retrieved successfully.",
//   //            Data = books
//   //        });
//   //    }
//   //    catch (Exception ex)
//   //    {
//   //        _logger.LogError(ex, "Error occurred while retrieving books by author.");
//   //        return Ok(new ErrorResponse
//   //        {
//   //            Status = HttpStatus.INTERNAL_ERROR.GetDescription(),
//   //            StatusCode = HttpStatus.INTERNAL_ERROR,
//   //            Message = "An error occurred while processing your request.",
//   //        });
//   //    }
//   //}
//
//   //6	Xem sách cùng thể loại theo bookId
//   [HttpGet("GetBooksByCategory/{bookId}")]
//   public async Task<IActionResult> GetBooksByCategory(int bookId)
//   {
//     try
//     {
//       var book = await _context.Books.FindAsync(bookId);
//       if (book == null)
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.NOT_FOUND.GetDescription(),
//           StatusCode = HttpStatus.NOT_FOUND,
//           Message = "Book not found.",
//         });
//       }
//
//       var books = await (
//         from b in _context.Books
//         join c in _context.Categories on b.CategoryId equals c.Id
//         where b.CategoryId == book.CategoryId && b.Id != bookId
//         select new
//         {
//           b.Id,
//           b.Title,
//           // b.Author,
//           b.Publisher,
//           b.Thumbnail,
//           b.Price,
//           CategoryName = c.Name
//         }).ToListAsync();
//       if (books.Count == 0)
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.NOT_FOUND.GetDescription(),
//           StatusCode = HttpStatus.NOT_FOUND,
//           Message = "No books found in this category.",
//         });
//       }
//
//       return Ok(new OkResponse
//       {
//         Status = HttpStatus.OK.GetDescription(),
//         StatusCode = HttpStatus.OK,
//         Message = "Books by category retrieved successfully.",
//         Data = books
//       });
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError($"Lỗi khi lấy sách cùng thể loại: {ex.Message}");
//       return StatusCode(500, "Lỗi máy chủ nội bộ");
//     }
//   }
//
//   //Xem sách theo thể loại theo Category Name
//   [HttpGet("GetBooksByCategory/{categoryName}")]
//   public async Task<IActionResult> GetBooksByCategoryName(string categoryName)
//   {
//     try
//     {
//       var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);
//       if (category == null)
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.NOT_FOUND.GetDescription(),
//           StatusCode = HttpStatus.NOT_FOUND,
//           Message = "Category not found.",
//         });
//       }
//
//       var books = await (
//         from b in _context.Books
//         join c in _context.Categories on b.CategoryId equals c.Id
//         where c.Name == categoryName
//         select new
//         {
//           b.Id,
//           b.Title,
//           // b.Author,
//           b.Publisher,
//           b.Thumbnail,
//           b.Price,
//           CategoryName = c.Name
//         }).ToListAsync();
//       if (books.Count == 0)
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.NOT_FOUND.GetDescription(),
//           StatusCode = HttpStatus.NOT_FOUND,
//           Message = "No books found in this category.",
//         });
//       }
//
//       return Ok(new OkResponse
//       {
//         Status = HttpStatus.OK.GetDescription(),
//         StatusCode = HttpStatus.OK,
//         Message = "Books by category retrieved successfully.",
//         Data = books
//       });
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError($"Lỗi khi lấy sách cùng thể loại: {ex.Message}");
//       return StatusCode(500, "Lỗi máy chủ nội bộ");
//     }
//   }
//
//   //Xem sách theo thể loại CategoryId
//   [HttpGet("GetBooksByCategory/{categoryId}")]
//   public async Task<IActionResult> GetBooksByCategoryId(int categoryId)
//   {
//     try
//     {
//       var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
//       if (category == null)
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.NOT_FOUND.GetDescription(),
//           StatusCode = HttpStatus.NOT_FOUND,
//           Message = "Category not found.",
//         });
//       }
//
//       var books = await (
//         from b in _context.Books
//         join c in _context.Categories on b.CategoryId equals c.Id
//         where b.CategoryId == categoryId
//         select new
//         {
//           b.Id,
//           b.Title,
//           // b.Author,
//           b.Publisher,
//           b.Thumbnail,
//           b.Price,
//           CategoryName = c.Name
//         }).ToListAsync();
//       if (books.Count == 0)
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.NOT_FOUND.GetDescription(),
//           StatusCode = HttpStatus.NOT_FOUND,
//           Message = "No books found in this category.",
//         });
//       }
//
//       return Ok(new OkResponse
//       {
//         Status = HttpStatus.OK.GetDescription(),
//         StatusCode = HttpStatus.OK,
//         Message = "Books by category retrieved successfully.",
//         Data = books
//       });
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError($"Lỗi khi lấy sách cùng thể loại: {ex.Message}");
//       return StatusCode(500, "Lỗi máy chủ nội bộ");
//     }
//   }
//
//   //7	Xem đánh giá/bình luận
//   [HttpGet("book-reviews/{bookId}")] public async Task<IActionResult> GetBookReviews(int bookId)
//   {
//     try
//     {
//       var reviews = await _context.BookReviews.Where(r => r.BookId == bookId).ToListAsync();
//       return Ok(new OkResponse
//       {
//         Status = HttpStatus.OK.GetDescription(),
//         StatusCode = HttpStatus.OK,
//         Message = "Book reviews retrieved successfully.",
//         Data = reviews
//       });
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError(ex, "Error occurred while retrieving book reviews.");
//       return Ok(new ErrorResponse
//       {
//         Status = HttpStatus.INTERNAL_ERROR.GetDescription(),
//         StatusCode = HttpStatus.INTERNAL_ERROR,
//         Message = "An error occurred while processing your request.",
//       });
//     }
//   }
//
//   //8	Đánh giá sách
//   [HttpPost("rate-book")] [Authorize] public async Task<IActionResult> RateBook([FromBody] BookReviewDto reviewDto)
//   {
//     try
//     {
//       var accountId = User.Claims.FirstOrDefault(c => c.Type == "AccountId")?.Value;
//       if (string.IsNullOrEmpty(accountId))
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.UNAUTHORIZED.GetDescription(),
//           StatusCode = HttpStatus.UNAUTHORIZED,
//           Message = "Unauthorized access.",
//         });
//       }
//
//       var book = await _context.Books.FindAsync(reviewDto.BookId);
//       if (book == null)
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.NOT_FOUND.GetDescription(),
//           StatusCode = HttpStatus.NOT_FOUND,
//           Message = "Book not found.",
//         });
//       }
//
//       var review = new BookReview
//       {
//         BookId = reviewDto.BookId,
//         AccountId = int.Parse(accountId),
//         Rating = reviewDto.Rating,
//         Comment = reviewDto.Comment,
//         CreatedAt = DateTime.UtcNow
//       };
//       await _context.BookReviews.AddAsync(review);
//       await _context.SaveChangesAsync();
//       return Ok(new OkResponse
//       {
//         Status = HttpStatus.OK.GetDescription(),
//         StatusCode = HttpStatus.OK,
//         Message = "Book rated successfully.",
//         Data = review
//       });
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError(ex, "Error occurred while rating the book.");
//       return Ok(new ErrorResponse
//       {
//         Status = HttpStatus.INTERNAL_ERROR.GetDescription(),
//         StatusCode = HttpStatus.INTERNAL_ERROR,
//         Message = "An error occurred while processing your request.",
//       });
//     }
//   }
//
//   //9	Viết bình luận
//   [HttpPost("comment-book")] [Authorize]
//   public async Task<IActionResult> CommentBook([FromBody] BookReviewDto commentDto)
//   {
//     try
//     {
//       var accountId = User.Claims.FirstOrDefault(c => c.Type == "AccountId")?.Value;
//       if (string.IsNullOrEmpty(accountId))
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.UNAUTHORIZED.GetDescription(),
//           StatusCode = HttpStatus.UNAUTHORIZED,
//           Message = "Unauthorized access.",
//         });
//       }
//
//       var book = await _context.Books.FindAsync(commentDto.BookId);
//       if (book == null)
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.NOT_FOUND.GetDescription(),
//           StatusCode = HttpStatus.NOT_FOUND,
//           Message = "Book not found.",
//         });
//       }
//
//       var comment = new BookReview
//       {
//         BookId = commentDto.BookId,
//         AccountId = int.Parse(accountId),
//         Rating = 0, // Rating không bắt buộc khi chỉ bình luận
//         Comment = commentDto.Comment,
//         CreatedAt = DateTime.UtcNow
//       };
//       await _context.BookReviews.AddAsync(comment);
//       await _context.SaveChangesAsync();
//       return Ok(new OkResponse
//       {
//         Status = HttpStatus.OK.GetDescription(),
//         StatusCode = HttpStatus.OK,
//         Message = "Comment added successfully.",
//         Data = comment
//       });
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError(ex, "Error occurred while adding a comment.");
//       return Ok(new ErrorResponse
//       {
//         Status = HttpStatus.INTERNAL_ERROR.GetDescription(),
//         StatusCode = HttpStatus.INTERNAL_ERROR,
//         Message = "An error occurred while processing your request.",
//       });
//     }
//   }
//
//   //10	Gợi ý sách cá nhân hóa(Gợi ý sách dựa trên lịch sử đọc. => Danh sách sách gợi ý. Công nghệ : GenAI)
//   [HttpGet("personalized-recommendations")] [Authorize]
//   public async Task<IActionResult> GetPersonalizedRecommendations()
//   {
//     try
//     {
//       var accountId = User.FindFirst("AccountId")?.Value;
//       if (string.IsNullOrEmpty(accountId))
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.UNAUTHORIZED.GetDescription(),
//           StatusCode = HttpStatus.UNAUTHORIZED,
//           Message = "User is not authenticated.",
//         });
//       }
//
//       var readHistory = await _context.BookReviews
//                                       .Where(r => r.AccountId == int.Parse(accountId))
//                                       .Select(r => r.BookId)
//                                       .Distinct()
//                                       .ToListAsync();
//       if (!readHistory.Any())
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.NOT_FOUND.GetDescription(),
//           StatusCode = HttpStatus.NOT_FOUND,
//           Message = "No reading history found for personalized recommendations.",
//         });
//       }
//
//       // Gọi API GenAI để lấy gợi ý sách
//       var bookIds = string.Join(",", readHistory);
//       var aiResponse = await _genAIService.GetBookRecommendations(bookIds);
//       if (aiResponse == null || !aiResponse.Any())
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.NOT_FOUND.GetDescription(),
//           StatusCode = HttpStatus.NOT_FOUND,
//           Message = "No recommendations found from GenAI.",
//         });
//       }
//
//       var recommendedBooks = await _context.Books
//                                            .Where(b => aiResponse.Contains(b.Id))
//                                            .ToListAsync();
//       return Ok(new OkResponse
//       {
//         Status = HttpStatus.OK.GetDescription(),
//         StatusCode = HttpStatus.OK,
//         Message = "Personalized book recommendations retrieved successfully.",
//         Data = recommendedBooks
//       });
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError(ex, "Error occurred while generating personalized book recommendations.");
//       return Ok(new ErrorResponse
//       {
//         Status = HttpStatus.INTERNAL_ERROR.GetDescription(),
//         StatusCode = HttpStatus.INTERNAL_ERROR,
//         Message = "An error occurred while processing your request.",
//       });
//     }
//   }
//
//   //11	Gợi ý sách theo ngữ cảnh
//
//   //12	Khám phá sách mới (Hiển thị sách mới nhất.)
//   [HttpGet("new-books")] [AllowAnonymous]
//   public async Task<IActionResult> GetNewBooks()
//   {
//     try
//     {
//       var newBooks = await _context.Books
//                                    .OrderByDescending(b => b.CreateDate)
//                                    .Take(10)
//                                    .ToListAsync();
//       if (newBooks == null || !newBooks.Any())
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.NOT_FOUND.GetDescription(),
//           StatusCode = HttpStatus.NOT_FOUND,
//           Message = "No new books found.",
//         });
//       }
//
//       return Ok(new OkResponse
//       {
//         Status = HttpStatus.OK.GetDescription(),
//         StatusCode = HttpStatus.OK,
//         Message = "New books retrieved successfully.",
//         Data = newBooks
//       });
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError(ex, "Error occurred while retrieving new books.");
//       return Ok(new ErrorResponse
//       {
//         Status = HttpStatus.INTERNAL_ERROR.GetDescription(),
//         StatusCode = HttpStatus.INTERNAL_ERROR,
//         Message = "An error occurred while processing your request.",
//       });
//     }
//   }
//
//   //13	Khám phá sách phổ biến (Hiển thị sách phổ biến nhất.)
//   [HttpGet("popular-books")] [AllowAnonymous]
//   public async Task<IActionResult> GetPopularBooks()
//   {
//     try
//     {
//       var popularBooks = await _context.Books
//                                        .OrderByDescending(b => b.TotalCopies - b.AvailableCopies)
//                                        .Take(10)
//                                        .ToListAsync();
//       if (popularBooks == null || !popularBooks.Any())
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.NOT_FOUND.GetDescription(),
//           StatusCode = HttpStatus.NOT_FOUND,
//           Message = "No popular books found.",
//         });
//       }
//
//       return Ok(new OkResponse
//       {
//         Status = HttpStatus.OK.GetDescription(),
//         StatusCode = HttpStatus.OK,
//         Message = "Popular books retrieved successfully.",
//         Data = popularBooks
//       });
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError(ex, "Error occurred while retrieving popular books.");
//       return Ok(new ErrorResponse
//       {
//         Status = HttpStatus.INTERNAL_ERROR.GetDescription(),
//         StatusCode = HttpStatus.INTERNAL_ERROR,
//         Message = "An error occurred while processing your request.",
//       });
//     }
//   }
//
//   //14	Xem lịch sử tra cứu
//   [HttpGet("View History")] public async Task<IActionResult> GetHistoryByAccount(int AccountId)
//   {
//     var account = _context.Accounts.FirstOrDefault(x => x.Id == AccountId);
//     if (account == null)
//     {
//       return Ok(new ErrorResponse
//       {
//         Status = HttpStatus.NOT_FOUND.GetDescription(),
//         StatusCode = HttpStatus.NOT_FOUND,
//         Message = "AccountControllers not found.",
//       });
//     }
//
//     var history = await _context.HistoryReviews
//                                 .Where(x => x.AccountId == AccountId)
//                                 .OrderByDescending(x => x.CreateAt)
//                                 .ToListAsync();
//     return Ok(new OkResponse
//     {
//       Status = HttpStatus.OK.GetDescription(),
//       StatusCode = HttpStatus.OK,
//       Message = "View History successfully.",
//       Data = history
//     });
//   }
//
//   //15	Lưu tìm kiếm
//   [HttpPost("SaveSearch")] public async Task<IActionResult> SaveSearchHistory(int AccountId, string SearchQuery)
//   {
//     var account = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == AccountId);
//     if (account == null)
//     {
//       return Ok(new ErrorResponse
//       {
//         Status = HttpStatus.NOT_FOUND.GetDescription(),
//         StatusCode = HttpStatus.NOT_FOUND,
//         Message = "AccountControllers not found.",
//       });
//     }
//
//     var searchHistory = new HistoryReview { AccountId = AccountId, SearchQuery = SearchQuery, CreateAt = DateTime.Now };
//     _context.HistoryReviews.Add(searchHistory);
//     await _context.SaveChangesAsync();
//     return Ok(new
//     {
//       Status = HttpStatus.OK.GetDescription(),
//       StatusCode = HttpStatus.OK,
//       Message = "Save search customer added successfully.",
//       Data = searchHistory
//     });
//   }
//
//   //16	Tìm kiếm sách bằng giọng nói // Speech 
//   [HttpPost("SearchBookByVoice")] public async Task<IActionResult> SearchBookByVoice([FromForm] IFormFile audioFile)
//   {
//     if (audioFile == null || audioFile.Length == 0)
//     {
//       return BadRequest(new { Message = "Không có file âm thanh nào được tải lên." });
//     }
//
//     try
//     {
//       // Lưu file tạm thời
//       var filePath = Path.GetTempFileName();
//       using (var stream = new FileStream(filePath, FileMode.Create)) { await audioFile.CopyToAsync(stream); }
//
//       // Gọi script Python để xử lý giọng nói
//       string pythonScriptPath = "speech_to_text.py"; // Đường dẫn file Python
//       var process = new Process
//       {
//         StartInfo = new ProcessStartInfo
//         {
//           FileName = "python",
//           Arguments = $"{pythonScriptPath} \"{filePath}\"",
//           RedirectStandardOutput = true,
//           UseShellExecute = false,
//           CreateNoWindow = true
//         }
//       };
//       process.Start();
//       string recognizedText = await process.StandardOutput.ReadToEndAsync();
//       process.WaitForExit();
//       if (string.IsNullOrEmpty(recognizedText)) { return Ok(new { Message = "Không nhận diện được giọng nói." }); }
//
//       // Tìm kiếm sách trong cơ sở dữ liệu
//       var books = await _context.Books
//                                 .Where(b => b.Title.Contains(recognizedText))
//                                 .ToListAsync();
//       return Ok(new
//       {
//         Message = books.Any() ? "Tìm thấy sách" : "Không tìm thấy sách.", SearchQuery = recognizedText, Books = books
//       });
//     }
//     catch (Exception ex) { return StatusCode(500, new { Message = "Lỗi xử lý yêu cầu.", Error = ex.Message }); }
//   }
//
//   //17	Xem sách theo bộ sưu tập
//   [HttpGet("collection-books")] [AllowAnonymous]
//   public async Task<IActionResult> GetBooksByCollection([FromQuery] BookType collectionId)
//   {
//     try
//     {
//       var books = await _context.Books
//                                 .Where(b => b.BookType == collectionId)
//                                 .ToListAsync();
//       if (books == null || !books.Any())
//       {
//         return Ok(new ErrorResponse
//         {
//           Status = HttpStatus.NOT_FOUND.GetDescription(),
//           StatusCode = HttpStatus.NOT_FOUND,
//           Message = "No books found in this collection.",
//         });
//       }
//
//       return Ok(new OkResponse
//       {
//         Status = HttpStatus.OK.GetDescription(),
//         StatusCode = HttpStatus.OK,
//         Message = "Books by collection retrieved successfully.",
//         Data = books
//       });
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError(ex, "Error occurred while retrieving books by collection.");
//       return Ok(new ErrorResponse
//       {
//         Status = HttpStatus.INTERNAL_ERROR.GetDescription(),
//         StatusCode = HttpStatus.INTERNAL_ERROR,
//         Message = "An error occurred while processing your request.",
//       });
//     }
//   }
// }