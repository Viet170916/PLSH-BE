using Data.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Model.ResponseModel;
using Microsoft.Extensions.Logging;
using API.DTO.Book;
using Common.Enums;
using Model.Entity;
using Model.Entity.book;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageBookDetailController(AppDbContext context, ILogger<ManageBookDetailController> logger)
        : ControllerBase
    {
        // GET: api/ManageBookDetail
        [HttpGet]
        public IActionResult GetAllBookDetails()
        {
            try
            {
                var bookDetails = context.BookDetails.ToList();
                return Ok(new OkResponse
                {
                    Message = "Book details retrieved successfully",
                    StatusCode = HttpStatus.OK,
                    Status = HttpStatus.OK.ToString(),
                    Data = bookDetails
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving book details");
                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
                {
                    Message = "Internal server error",
                    StatusCode = HttpStatus.INTERNAL_ERROR,
                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
                    StackTrace = ex.StackTrace??"",
                });
            }
        }

        // GET: api/ManageBookDetail/5
        [HttpGet("{id}")]
        public IActionResult GetBookDetailById(int id)
        {
            try
            {
                var bookDetail = context.BookDetails.Find(id);
                if (bookDetail == null)
                {
                    return NotFound(new OkResponse
                    {
                        Message = "Book detail not found",
                        StatusCode = HttpStatus.NOT_FOUND,
                        Status = HttpStatus.NOT_FOUND.ToString()
                    });
                }
                return Ok(new OkResponse
                {
                    Message = "Book detail retrieved successfully",
                    StatusCode = HttpStatus.OK,
                    Status = HttpStatus.OK.ToString(),
                    Data = bookDetail
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving book detail");
                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
                {
                    Message = "Internal server error",
                    StatusCode = HttpStatus.INTERNAL_ERROR,
                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
                    StackTrace = ex.StackTrace
                });
            }
        }

        // POST: api/ManageBookDetail
        [HttpPost]
        public IActionResult CreateBookDetail([FromBody] BookDetailCreateDto dto)
        {
            try
            {
                // Validate book exists
                if (!context.Books.Any(b => b.Id == dto.BookId))
                {
                    return BadRequest(new OkResponse
                    {
                        Message = "Book ID does not exist",
                        StatusCode = HttpStatus.BAD_REQUEST,
                        Status = HttpStatus.BAD_REQUEST.ToString()
                    });
                }

                var bookDetail = new BookDetail
                {
                    BookId = dto.BookId,
                    Status = dto.Status,
                    BookConditionUrl = dto.BookConditionUrl,
                    BookConditionDescription = dto.BookConditionDescription,
                    StatusDescription = dto.StatusDescription,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.BookDetails.Add(bookDetail);
                context.SaveChanges();

                return CreatedAtAction(nameof(GetBookDetailById), new { id = bookDetail.Id }, new OkResponse
                {
                    Message = "Book detail created successfully",
                    StatusCode = HttpStatus.OK,
                    Status = HttpStatus.OK.ToString(),
                    Data = bookDetail
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating book detail");
                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
                {
                    Message = "Internal server error",
                    StatusCode = HttpStatus.INTERNAL_ERROR,
                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
                    StackTrace = ex.StackTrace
                });
            }
        }

        // PUT: api/ManageBookDetail/5
        [HttpPut("{id}")]
        public IActionResult UpdateBookDetail(int id, [FromBody] BookDetailUpdateDTO dto)
        {
            try
            {
                var bookDetail = context.BookDetails.Find(id);
                if (bookDetail == null)
                {
                    return NotFound(new OkResponse
                    {
                        Message = "Book detail not found",
                        StatusCode = HttpStatus.NOT_FOUND,
                        Status = HttpStatus.NOT_FOUND.ToString()
                    });
                }

                // Validate status value
                if (dto.Status != 0 && dto.Status != 1)
                {
                    return BadRequest(new OkResponse
                    {
                        Message = "Invalid status value (0 or 1 only)",
                        StatusCode = HttpStatus.BAD_REQUEST,
                        Status = HttpStatus.BAD_REQUEST.ToString()
                    });
                }

                bookDetail.Status = dto.Status;
                bookDetail.BookConditionUrl = dto.BookConditionUrl;
                bookDetail.BookConditionDescription = dto.BookConditionDescription;
                bookDetail.StatusDescription = dto.StatusDescription;
                bookDetail.UpdatedAt = DateTime.UtcNow;

                context.SaveChanges();

                return Ok(new OkResponse
                {
                    Message = "Book detail updated successfully",
                    StatusCode = HttpStatus.OK,
                    Status = HttpStatus.OK.ToString(),
                    Data = bookDetail
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating book detail");
                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
                {
                    Message = "Internal server error",
                    StatusCode = HttpStatus.INTERNAL_ERROR,
                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
                    StackTrace = ex.StackTrace
                });
            }
        }

        // DELETE: api/ManageBookDetail/5
        [HttpDelete("{id}")]
        public IActionResult DeleteBookDetail(int id)
        {
            try
            {
                var bookDetail = context.BookDetails.Find(id);
                if (bookDetail == null)
                {
                    return NotFound(new OkResponse
                    {
                        Message = "Book detail not found",
                        StatusCode = HttpStatus.NOT_FOUND,
                        Status = HttpStatus.NOT_FOUND.ToString()
                    });
                }

                context.BookDetails.Remove(bookDetail);
                context.SaveChanges();

                return Ok(new OkResponse
                {
                    Message = "Book detail deleted successfully",
                    StatusCode = HttpStatus.OK,
                    Status = HttpStatus.OK.ToString()
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting book detail");
                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
                {
                    Message = "Internal server error",
                    StatusCode = HttpStatus.INTERNAL_ERROR,
                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
                    StackTrace = ex.StackTrace
                });
            }
        }

        // GET: api/ManageBookDetail/byBook/5
        [HttpGet("byBook/{bookId}")]
        public IActionResult GetDetailsByBook(int bookId)
        {
            try
            {
                var details = context.BookDetails
                    .Where(bd => bd.BookId == bookId)
                    .ToList();

                if (!details.Any())
                {
                    return NotFound(new OkResponse
                    {
                        Message = "No details found for this book",
                        StatusCode = HttpStatus.NOT_FOUND,
                        Status = HttpStatus.NOT_FOUND.ToString()
                    });
                }

                return Ok(new OkResponse
                {
                    Message = "Book details retrieved successfully",
                    StatusCode = HttpStatus.OK,
                    Status = HttpStatus.OK.ToString(),
                    Data = details
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving book details");
                return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
                {
                    Message = "Internal server error",
                    StatusCode = HttpStatus.INTERNAL_ERROR,
                    Status = HttpStatus.INTERNAL_ERROR.ToString(),
                    StackTrace = ex.StackTrace
                });
            }
        }

        // PATCH: api/ManageBookDetail/updateStatus/5
        // [HttpPatch("updateStatus/{id}")]
        // public IActionResult UpdateStatus(int id, [FromBody] BookDetailStatusUpdateDto dto)
        // {
        //     try
        //     {
        //         var bookDetail = _context.BookDetails.Find(id);
        //         if (bookDetail == null)
        //         {
        //             return NotFound(new OkResponse
        //             {
        //                 Message = "Book detail not found",
        //                 StatusCode = HttpStatus.NOT_FOUND,
        //                 Status = HttpStatus.NOT_FOUND.ToString()
        //             });
        //         }
        //
        //         if (dto.Status != 0 && dto.Status != 1)
        //         {
        //             return BadRequest(new OkResponse
        //             {
        //                 Message = "Invalid status value (0 or 1 only)",
        //                 StatusCode = HttpStatus.BAD_REQUEST,
        //                 Status = HttpStatus.BAD_REQUEST.ToString()
        //             });
        //         }
        //
        //         bookDetail.Status = dto.Status;
        //         bookDetail.StatusDescription = dto.StatusDescription;
        //         bookDetail.UpdatedAt = DateTime.UtcNow;
        //
        //         _context.SaveChanges();
        //
        //         return Ok(new OkResponse
        //         {
        //             Message = "Status updated successfully",
        //             StatusCode = HttpStatus.OK,
        //             Status = HttpStatus.OK.ToString(),
        //             Data = bookDetail
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error updating status");
        //         return StatusCode((int)HttpStatus.INTERNAL_ERROR, new OkResponse
        //         {
        //             Message = "Internal server error",
        //             StatusCode = HttpStatus.INTERNAL_ERROR,
        //             Status = HttpStatus.INTERNAL_ERROR.ToString(),
        //             StackTrace = ex.StackTrace
        //         });
        //     }
        // }
    }
}