//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using Data.DatabaseContext;
//using Model.Entity;
//using Model.ResponseModel;
//using Common.Enums;
//using System.Linq;
//using System.Net;
//using System;
//using API.DTO.Book;

//namespace API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ManageBookController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly ILogger<ManageBookController> _logger;

//        public ManageBookController(AppDbContext context, ILogger<ManageBookController> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        // GET: api/ManageBook
//        [HttpGet]
//        public IActionResult GetAllBooks()
//        {
//            try
//            {
//                var books = _context.Books.ToList();
//                return Ok(new OkResponse
//                {
//                    Message = "Books retrieved successfully",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = books
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while retrieving books.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while retrieving books",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }

//        // GET: api/ManageBook/5
//        [HttpGet("{id}")]
//        public IActionResult GetBookById(int id)
//        {
//            try
//            {
//                var book = _context.Books.Find(id);
//                if (book == null)
//                {
//                    return NotFound(new OkResponse
//                    {
//                        Message = "Book not found",
//                        StatusCode = HttpStatus.NOT_FOUND,
//                        Status = HttpStatus.NOT_FOUND.ToString(),
//                        Data = null
//                    });
//                }

//                return Ok(new OkResponse
//                {
//                    Message = "Book retrieved successfully",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = book
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while retrieving the book.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while retrieving the book",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }

//        // POST: api/ManageBook/Addbook
//        [HttpPost]
//        public IActionResult AddBook([FromBody] AddBookDTO bookDto)
//        {
//            try
//            {
//                if (bookDto == null)
//                {
//                    return BadRequest(new OkResponse
//                    {
//                        Message = "Book data is null",
//                        StatusCode = HttpStatus.BAD_REQUEST,
//                        Status = HttpStatus.BAD_REQUEST.ToString(),
//                        Data = null
//                    });
//                }

//                // Kiểm tra xem sách có cùng tên đã tồn tại chưa
//                var existingBook = _context.Books.FirstOrDefault(b => b.Title == bookDto.Title);

//                if (existingBook != null)
//                {
//                    // Nếu sách đã tồn tại, cộng thêm số lượng
//                    existingBook.Quantity += bookDto.Quantity;
//                    existingBook.UpdateDate = DateTime.Now;

//                    _context.Books.Update(existingBook);
//                    _context.SaveChanges();

//                    return Ok(new OkResponse
//                    {
//                        Message = "Book quantity updated successfully",
//                        StatusCode = HttpStatus.OK,
//                        Status = HttpStatus.OK.ToString(),
//                        Data = existingBook
//                    });
//                }
//                else
//                {
//                    // Nếu sách chưa tồn tại, thêm sách mới
//                    var book = new Book
//                    {
//                        Title = bookDto.Title,
//                        Author = bookDto.Author,
//                        Description = bookDto.Description,
//                        Publisher = bookDto.Publisher,
//                        PublishDate = bookDto.PublishDate,
//                        Language = bookDto.Language,
//                        Position = bookDto.Position,
//                        PageCount = bookDto.PageCount,
//                        Category = bookDto.Category,
//                        ISBNumber = bookDto.ISBNumber,
//                        Quantity = bookDto.Quantity,
//                        Price = bookDto.Price,
//                        Thumbnail = bookDto.Thumbnail,
//                        Fine = bookDto.Fine,
//                        CreateDate = DateTime.Now,
//                        UpdateDate = DateTime.Now
//                    };

//                    _context.Books.Add(book);
//                    _context.SaveChanges();

//                    return Ok(new OkResponse
//                    {
//                        Message = "Book added successfully",
//                        StatusCode = HttpStatus.OK,
//                        Status = HttpStatus.OK.ToString(),
//                        Data = book
//                    });
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while adding the book.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while adding the book",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }

//        // PUT: api/ManageBook/5
//        [HttpPut("{id}")]
//        public IActionResult UpdateBook(int id, [FromBody] BookDTO bookDto)
//        {
//            try
//            {
//                var book = _context.Books.Find(id);
//                if (book == null)
//                {
//                    return NotFound(new OkResponse
//                    {
//                        Message = "Book not found",
//                        StatusCode = HttpStatus.NOT_FOUND,
//                        Status = HttpStatus.NOT_FOUND.ToString(),
//                        Data = null
//                    });
//                }

//                book.Title = bookDto.Title;
//                book.Author = bookDto.Author;
//                book.Description = bookDto.Description;         
//                book.PublishDate = bookDto.PublishDate;
//                book.UpdateDate = DateTime.Now;

//                _context.Books.Update(book);
//                _context.SaveChanges();

//                return Ok(new OkResponse
//                {
//                    Message = "Book updated successfully",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = book
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while updating the book.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while updating the book",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }

//        // DELETE: api/ManageBook/5
//        [HttpDelete("{id}")]
//        public IActionResult DeleteBook(int id)
//        {
//            try
//            {
//                var book = _context.Books.Find(id);
//                if (book == null)
//                {
//                    return NotFound(new OkResponse
//                    {
//                        Message = "Book not found",
//                        StatusCode = HttpStatus.NOT_FOUND,
//                        Status = HttpStatus.NOT_FOUND.ToString(),
//                        Data = null
//                    });
//                }

//                _context.Books.Remove(book);
//                _context.SaveChanges();

//                return Ok(new OkResponse
//                {
//                    Message = "Book deleted successfully",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = null
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while deleting the book.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while deleting the book",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }

//        // Search Book by Name
//        [HttpGet("searchByName")]
//        public IActionResult SearchBookByName([FromQuery] string name)
//        {
//            try
//            {
//                var books = _context.Books
//                    .Where(b => b.Title.Contains(name, StringComparison.OrdinalIgnoreCase))
//                    .ToList();

//                if (!books.Any())
//                {
//                    return NotFound(new OkResponse
//                    {
//                        Message = "No books found with the given name",
//                        StatusCode = HttpStatus.NOT_FOUND,
//                        Status = HttpStatus.NOT_FOUND.ToString(),
//                        Data = null
//                    });
//                }

//                return Ok(new OkResponse
//                {
//                    Message = "Books retrieved successfully",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = books
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while searching for books by name.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while searching for books by name",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }

//        // Filter Books
//        [HttpGet("filter")]
//        public IActionResult FilterBooks(
//            [FromQuery] string author = null,
//            [FromQuery] DateTime? publishDate = null,
//            [FromQuery] string category = null,
//            [FromQuery] string language = null)
//        {
//            try
//            {
//                var query = _context.Books.AsQueryable();

//                if (!string.IsNullOrEmpty(author))
//                {
//                    query = query.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
//                }

//                if (publishDate.HasValue)
//                {
//                    query = query.Where(b => b.PublishDate == publishDate.Value);
//                }

//                if (!string.IsNullOrEmpty(category))
//                {
//                    query = query.Where(b => b.Category.Contains(category, StringComparison.OrdinalIgnoreCase));
//                }

//                if (!string.IsNullOrEmpty(language))
//                {
//                    query = query.Where(b => b.Language.Contains(language, StringComparison.OrdinalIgnoreCase));
//                }

//                var books = query.ToList();

//                if (!books.Any())
//                {
//                    return NotFound(new OkResponse
//                    {
//                        Message = "No books found with the given filters",
//                        StatusCode = HttpStatus.NOT_FOUND,
//                        Status = HttpStatus.NOT_FOUND.ToString(),
//                        Data = null
//                    });
//                }

//                return Ok(new OkResponse
//                {
//                    Message = "Books filtered successfully",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = books
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while filtering books.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while filtering books",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }

//        // Search Book by Category
//        [HttpGet("searchByCategory")]
//        public IActionResult SearchBookByCategory([FromQuery] string category)
//        {
//            try
//            {
//                var books = _context.Books
//                    .Where(b => b.Category.Contains(category, StringComparison.OrdinalIgnoreCase))
//                    .ToList();

//                if (!books.Any())
//                {
//                    return NotFound(new OkResponse
//                    {
//                        Message = "No books found in the given category",
//                        StatusCode = HttpStatus.NOT_FOUND,
//                        Status = HttpStatus.NOT_FOUND.ToString(),
//                        Data = null
//                    });
//                }

//                return Ok(new OkResponse
//                {
//                    Message = "Books retrieved successfully by category",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = books
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while searching for books by category.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while searching for books by category",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }

//        // Get All Books with Pagination
//        [HttpGet("pagination")]
//        public IActionResult GetAllBooksWithPagination([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
//        {
//            try
//            {
//                var totalBooks = _context.Books.Count();
//                var books = _context.Books
//                    .Skip((page - 1) * pageSize)
//                    .Take(pageSize)
//                    .ToList();

//                return Ok(new OkResponse
//                {
//                    Message = "Books retrieved successfully with pagination",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = new
//                    {
//                        TotalBooks = totalBooks,
//                        Page = page,
//                        PageSize = pageSize,
//                        Books = books
//                    }
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while retrieving books with pagination.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while retrieving books with pagination",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }

//        // Get Books by Author
//        [HttpGet("byAuthor")]
//        public IActionResult GetBooksByAuthor([FromQuery] string author)
//        {
//            try
//            {
//                var books = _context.Books
//                    .Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase))
//                    .ToList();

//                if (!books.Any())
//                {
//                    return NotFound(new OkResponse
//                    {
//                        Message = "No books found for the given author",
//                        StatusCode = HttpStatus.NOT_FOUND,
//                        Status = HttpStatus.NOT_FOUND.ToString(),
//                        Data = null
//                    });
//                }

//                return Ok(new OkResponse
//                {
//                    Message = "Books retrieved successfully by author",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = books
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while retrieving books by author.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while retrieving books by author",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }

//        // Get Books by Publish Date Range
//        [HttpGet("byPublishDateRange")]
//        public IActionResult GetBooksByPublishDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
//        {
//            try
//            {
//                var books = _context.Books
//                    .Where(b => b.PublishDate >= startDate && b.PublishDate <= endDate)
//                    .ToList();

//                if (!books.Any())
//                {
//                    return NotFound(new OkResponse
//                    {
//                        Message = "No books found within the given date range",
//                        StatusCode = HttpStatus.NOT_FOUND,
//                        Status = HttpStatus.NOT_FOUND.ToString(),
//                        Data = null
//                    });
//                }

//                return Ok(new OkResponse
//                {
//                    Message = "Books retrieved successfully by publish date range",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = books
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while retrieving books by publish date range.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while retrieving books by publish date range",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }

//        // Get Books by Price Range
//        [HttpGet("byPriceRange")]
//        public IActionResult GetBooksByPriceRange([FromQuery] double minPrice, [FromQuery] double maxPrice)
//        {
//            try
//            {
//                var books = _context.Books
//                    .Where(b => b.Price >= minPrice && b.Price <= maxPrice)
//                    .ToList();

//                if (!books.Any())
//                {
//                    return NotFound(new OkResponse
//                    {
//                        Message = "No books found within the given price range",
//                        StatusCode = HttpStatus.NOT_FOUND,
//                        Status = HttpStatus.NOT_FOUND.ToString(),
//                        Data = null
//                    });
//                }

//                return Ok(new OkResponse
//                {
//                    Message = "Books retrieved successfully by price range",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = books
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while retrieving books by price range.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while retrieving books by price range",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }

//        // Get Books by Quantity
//        [HttpGet("byQuantity")]
//        public IActionResult GetBooksByQuantity([FromQuery] int quantity)
//        {
//            try
//            {
//                var books = _context.Books
//                    .Where(b => b.Quantity >= quantity)
//                    .ToList();

//                if (!books.Any())
//                {
//                    return NotFound(new OkResponse
//                    {
//                        Message = "No books found with the given quantity",
//                        StatusCode = HttpStatus.NOT_FOUND,
//                        Status = HttpStatus.NOT_FOUND.ToString(),
//                        Data = null
//                    });
//                }

//                return Ok(new OkResponse
//                {
//                    Message = "Books retrieved successfully by quantity",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = books
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while retrieving books by quantity.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while retrieving books by quantity",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }

//        // Get Books by Language
//        [HttpGet("byLanguage")]
//        public IActionResult GetBooksByLanguage([FromQuery] string language)
//        {
//            try
//            {
//                var books = _context.Books
//                    .Where(b => b.Language.Contains(language, StringComparison.OrdinalIgnoreCase))
//                    .ToList();

//                if (!books.Any())
//                {
//                    return NotFound(new OkResponse
//                    {
//                        Message = "No books found for the given language",
//                        StatusCode = HttpStatus.NOT_FOUND,
//                        Status = HttpStatus.NOT_FOUND.ToString(),
//                        Data = null
//                    });
//                }

//                return Ok(new OkResponse
//                {
//                    Message = "Books retrieved successfully by language",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = books
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while retrieving books by language.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while retrieving books by language",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }

//        // Get Books by Publisher
//        [HttpGet("byPublisher")]
//        public IActionResult GetBooksByPublisher([FromQuery] string publisher)
//        {
//            try
//            {
//                var books = _context.Books
//                    .Where(b => b.Publisher.Contains(publisher, StringComparison.OrdinalIgnoreCase))
//                    .ToList();

//                if (!books.Any())
//                {
//                    return NotFound(new OkResponse
//                    {
//                        Message = "No books found for the given publisher",
//                        StatusCode = HttpStatus.NOT_FOUND,
//                        Status = HttpStatus.NOT_FOUND.ToString(),
//                        Data = null
//                    });
//                }

//                return Ok(new OkResponse
//                {
//                    Message = "Books retrieved successfully by publisher",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = books
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while retrieving books by publisher.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while retrieving books by publisher",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }

//        // Get Book by ISBN
//        [HttpGet("byISBN")]
//        public IActionResult GetBookByISBN([FromQuery] string isbn)
//        {
//            try
//            {
//                var book = _context.Books
//                    .FirstOrDefault(b => b.ISBNumber == isbn);

//                if (book == null)
//                {
//                    return NotFound(new OkResponse
//                    {
//                        Message = "No book found with the given ISBN",
//                        StatusCode = HttpStatus.NOT_FOUND,
//                        Status = HttpStatus.NOT_FOUND.ToString(),
//                        Data = null
//                    });
//                }

//                return Ok(new OkResponse
//                {
//                    Message = "Book retrieved successfully by ISBN",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = book
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while retrieving the book by ISBN.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while retrieving the book by ISBN",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }



//        // GET: api/ManageBook/available (Quantity > 0)
//        [HttpGet("available")]
//        public IActionResult GetAvailableBooks()
//        {
//            try
//            {
//                var books = _context.Books.Where(b => b.Quantity > 0).ToList();

//                if (!books.Any())
//                {
//                    return NotFound(new OkResponse
//                    {
//                        Message = "No available books found",
//                        StatusCode = HttpStatus.NOT_FOUND,
//                        Status = HttpStatus.NOT_FOUND.ToString(),
//                        Data = null
//                    });
//                }

//                return Ok(new OkResponse
//                {
//                    Message = "Available books retrieved successfully",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = books
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while retrieving available books.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while retrieving available books",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }

//        //Sort Books by Criteria
//        // GET: api/ManageBook/sort
//        [HttpGet("sort")]
//        public IActionResult SortBooks(
//            [FromQuery] string sortBy = "title",
//            [FromQuery] string sortOrder = "asc")
//        {
//            try
//            {
//                IQueryable<Book> query = _context.Books;

//                switch (sortBy.ToLower())
//                {
//                    case "title":
//                        query = sortOrder.ToLower() == "desc"
//                            ? query.OrderByDescending(b => b.Title)
//                            : query.OrderBy(b => b.Title);
//                        break;
//                    case "publishdate":
//                        query = sortOrder.ToLower() == "desc"
//                            ? query.OrderByDescending(b => b.PublishDate)
//                            : query.OrderBy(b => b.PublishDate);
//                        break;
//                    case "author":
//                        query = sortOrder.ToLower() == "desc"
//                            ? query.OrderByDescending(b => b.Author)
//                            : query.OrderBy(b => b.Author);
//                        break;
//                    case "price":
//                        query = sortOrder.ToLower() == "desc"
//                            ? query.OrderByDescending(b => b.Price)
//                            : query.OrderBy(b => b.Price);
//                        break;
//                    default:
//                        query = query.OrderBy(b => b.Title);
//                        break;
//                }

//                var books = query.ToList();
//                return Ok(new OkResponse
//                {
//                    Message = "Books sorted successfully",
//                    StatusCode = HttpStatus.OK,
//                    Status = HttpStatus.OK.ToString(),
//                    Data = books
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while sorting books.");
//                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
//                {
//                    Message = "An error occurred while sorting books",
//                    StatusCode = HttpStatus.INTERNAL_ERROR,
//                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                    StackTrace = ex.StackTrace,
//                    Data = null
//                });
//            }
//        }
//    }
//}
