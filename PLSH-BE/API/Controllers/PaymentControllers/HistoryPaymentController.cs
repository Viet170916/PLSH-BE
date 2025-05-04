using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.DatabaseContext;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Model.Entity;
using Model.Entity.Borrow;

namespace API.Controllers.PaymentControllers
{
  [Route("api/v1/transaction")]
  [ApiController]
  public class TransactionController : ControllerBase
  {
    private readonly AppDbContext _context;
    public TransactionController(AppDbContext context) { _context = context; }

    [Authorize("BorrowerPolicy")] [HttpGet("account/{accountId}")]
    public async Task<ActionResult> GetTransactionsByAccount(
      int accountId,
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 10,
      [FromQuery] string search = "",
      [FromQuery] int? status = null,
      [FromQuery] DateTime? fromDate = null,
      [FromQuery] DateTime? toDate = null,
      [FromQuery] decimal? minAmount = null,
      [FromQuery] decimal? maxAmount = null,
      [FromQuery] string sortBy = "date",
      [FromQuery] bool sortDesc = true
    )
    {
      try
      {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;
        if (toDate.HasValue && fromDate.HasValue && toDate < fromDate)
        {
          return BadRequest(new ApiResponse
          {
            Success = false, Message = "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu"
          });
        }

        var query = _context.Transactions
                            .Where(t => t.AccountId == userId && t.TransactionType == 1)
                            .Include(t => t.Fines)
                            .ThenInclude(f => f.BookBorrowing)
                            .ThenInclude(bb => bb.BookInstance)
                            .ThenInclude(bi => bi != null ? bi.Book : null)
                            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
          query = query.Where(t =>
            t.ReferenceId.Contains(search) ||
            t.Note.Contains(search) ||
            t.Fines.Any(f =>
              (f.BookBorrowing.BookInstance.Book != null &&
               f.BookBorrowing.BookInstance.Book.Title.Contains(search)) ||
              f.BookBorrowing.BookInstance.Code.Contains(search)));
        }

        if (status.HasValue) { query = query.Where(t => t.Status == status.Value); }

        if (fromDate.HasValue) { query = query.Where(t => t.TransactionDate >= fromDate.Value); }

        if (toDate.HasValue) { query = query.Where(t => t.TransactionDate <= toDate.Value); }

        if (minAmount.HasValue) { query = query.Where(t => t.Amount >= minAmount.Value); }

        if (maxAmount.HasValue) { query = query.Where(t => t.Amount <= maxAmount.Value); }

        query = sortBy.ToLower() switch
        {
          "amount" => sortDesc ? query.OrderByDescending(t => t.Amount) : query.OrderBy(t => t.Amount),
          "title" => sortDesc ?
            query.OrderByDescending(t =>
              t.Fines.FirstOrDefault().BookBorrowing.BookInstance.Book.Title) :
            query.OrderBy(t =>
              t.Fines.FirstOrDefault().BookBorrowing.BookInstance.Book.Title),
          _ => sortDesc ? query.OrderByDescending(t => t.TransactionDate) : query.OrderBy(t => t.TransactionDate)
        };
        var totalRecords = await query.CountAsync();
        var transactions = await query
                                 .Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .Select(t => new
                                 {
                                   Id = t.Id,
                                   Amount = t.Amount,
                                   Currency = t.Currency,
                                   Status = t.Status,
                                   TransactionDate = t.TransactionDate,
                                   ReferenceId = t.ReferenceId,
                                   TransactionType = t.TransactionType,
                                   Note = t.Note,
                                   Fine = t.Fines.Select(f => new
                                           {
                                             Id = f.Id,
                                             BookTitle = f.BookBorrowing != null &&
                                                         f.BookBorrowing.BookInstance != null &&
                                                         f.BookBorrowing.BookInstance.Book != null ?
                                               f.BookBorrowing.BookInstance.Book.Title :
                                               "Không xác định",
                                             BookId = f.BookBorrowing != null && f.BookBorrowing.BookInstance != null ?
                                               (f.BookBorrowing.BookInstance.Book != null ?
                                                 (f.BookBorrowing.BookInstance.Book.IsbNumber10 ??
                                                  f.BookBorrowing.BookInstance.Book.IsbNumber13 ??
                                                  f.BookBorrowing.BookInstance.Code) :
                                                 f.BookBorrowing.BookInstance.Code) :
                                               "N/A",
                                             FineDate = f.FineDate,
                                             FineType = f.FineType,
                                             FineAmount = f.FineByDate,
                                             FineStatus = f.Status
                                           })
                                           .ToList()
                                 })
                                 .ToListAsync();
        return Ok(new ApiPagedResponse<object>
        {
          Success = true,
          Data = transactions,
          Pagination = new PaginationMeta
          {
            CurrentPage = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
          }
        });
      }
      catch (Exception ex)
      {
        return StatusCode(500,
          new ApiResponse { Success = false, Message = "Lỗi server khi lấy danh sách giao dịch.", Error = ex.Message });
      }
    }

    [HttpGet("search")] public async Task<ActionResult> SearchFines(
      [FromQuery] string bookTitle = "",
      [FromQuery] string bookCode = "",
      [FromQuery] int? fineType = null,
      [FromQuery] string? status = null,
      [FromQuery] DateTime? fromDate = null,
      [FromQuery] DateTime? toDate = null
    )
    {
      try
      {
        var query = _context.Fines
                            .Include(f => f.BookBorrowing)
                            .ThenInclude(bb => bb.BookInstance)
                            .ThenInclude(bi => bi.Book)
                            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(bookTitle))
        {
          query = query.Where(f =>
            f.BookBorrowing.BookInstance.Book.Title.Contains(bookTitle));
        }

        if (!string.IsNullOrWhiteSpace(bookCode))
        {
          query = query.Where(f =>
            f.BookBorrowing.BookInstance.Code.Contains(bookCode));
        }

        if (fineType.HasValue) { query = query.Where(f => f.FineType == fineType.Value); }

        if (status is not null) { query = query.Where(f => f.Status == status); }

        if (fromDate.HasValue) { query = query.Where(f => f.FineDate >= fromDate.Value); }

        if (toDate.HasValue) { query = query.Where(f => f.FineDate <= toDate.Value); }

        var fines = await query
                          .Select(f => new
                          {
                            Id = f.Id,
                            BookTitle = f.BookBorrowing.BookInstance.Book.Title,
                            BookCode = f.BookBorrowing.BookInstance.Code,
                            FineDate = f.FineDate,
                            FineType = f.FineType,
                            Amount = f.FineByDate,
                            Status = f.Status,
                            TransactionId = f.TransactionId
                          })
                          .ToListAsync();
        return Ok(new ApiResponse { Success = true, Data = fines });
      }
      catch (Exception ex)
      {
        return StatusCode(500,
          new ApiResponse { Success = false, Message = "Lỗi server khi tìm kiếm phí phạt.", Error = ex.Message });
      }
    }

    [Authorize("LibrarianPolicy")] [HttpGet("for-lib")]
    public async Task<ActionResult> GetTransactions(
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 10,
      [FromQuery] string search = "",
      [FromQuery] int? status = null,
      [FromQuery] DateTime? fromDate = null,
      [FromQuery] DateTime? toDate = null,
      [FromQuery] decimal? minAmount = null,
      [FromQuery] decimal? maxAmount = null,
      [FromQuery] string sortBy = "date",
      [FromQuery] bool sortDesc = true,
      [FromQuery] int? accountId = null,
      [FromQuery] int? transactionType = null
    )
    {
      try
      {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;
        if (toDate.HasValue && fromDate.HasValue && toDate < fromDate)
        {
          return BadRequest(new ApiResponse
          {
            Success = false, Message = "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu"
          });
        }

        var query = _context.Transactions
                            .Include(t => t.Fines)
                            .ThenInclude(f => f.BookBorrowing)
                            .ThenInclude(bb => bb.BookInstance)
                            .ThenInclude(bi => bi != null ? bi.Book : null)
                            .AsQueryable();
        if (accountId.HasValue) { query = query.Where(t => t.AccountId == accountId.Value); }

        if (transactionType.HasValue) { query = query.Where(t => t.TransactionType == transactionType.Value); }

        if (!string.IsNullOrWhiteSpace(search))
        {
          query = query.Where(t =>
            t.ReferenceId.Contains(search) ||
            t.Note.Contains(search) ||
            t.Fines.Any(f =>
              (f.BookBorrowing.BookInstance.Book != null &&
               f.BookBorrowing.BookInstance.Book.Title.Contains(search)) ||
              f.BookBorrowing.BookInstance.Code.Contains(search)));
        }

        if (status.HasValue) { query = query.Where(t => t.Status == status.Value); }

        if (fromDate.HasValue) { query = query.Where(t => t.TransactionDate >= fromDate.Value); }

        if (toDate.HasValue) { query = query.Where(t => t.TransactionDate <= toDate.Value); }

        if (minAmount.HasValue) { query = query.Where(t => t.Amount >= minAmount.Value); }

        if (maxAmount.HasValue) { query = query.Where(t => t.Amount <= maxAmount.Value); }

        query = sortBy.ToLower() switch
        {
          "amount" => sortDesc ? query.OrderByDescending(t => t.Amount) : query.OrderBy(t => t.Amount),
          "title" => sortDesc ?
            query.OrderByDescending(t =>
              t.Fines.FirstOrDefault().BookBorrowing.BookInstance.Book.Title) :
            query.OrderBy(t =>
              t.Fines.FirstOrDefault().BookBorrowing.BookInstance.Book.Title),
          _ => sortDesc ? query.OrderByDescending(t => t.TransactionDate) : query.OrderBy(t => t.TransactionDate)
        };
        var totalRecords = await query.CountAsync();
        var transactions = await query
                                 .Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .Select(t => new
                                 {
                                   Id = t.Id,
                                   AccountId = t.AccountId,
                                   Amount = t.Amount,
                                   Currency = t.Currency,
                                   Status = t.Status,
                                   TransactionDate = t.TransactionDate,
                                   ReferenceId = t.ReferenceId,
                                   TransactionType = t.TransactionType,
                                   Note = t.Note,
                                   Fine = t.Fines.Select(f => new
                                           {
                                             Id = f.Id,
                                             BookTitle = f.BookBorrowing != null &&
                                                         f.BookBorrowing.BookInstance != null &&
                                                         f.BookBorrowing.BookInstance.Book != null ?
                                               f.BookBorrowing.BookInstance.Book.Title :
                                               "Không xác định",
                                             BookId = f.BookBorrowing != null && f.BookBorrowing.BookInstance != null ?
                                               (f.BookBorrowing.BookInstance.Book != null ?
                                                 (f.BookBorrowing.BookInstance.Book.IsbNumber10 ??
                                                  f.BookBorrowing.BookInstance.Book.IsbNumber13 ??
                                                  f.BookBorrowing.BookInstance.Code) :
                                                 f.BookBorrowing.BookInstance.Code) :
                                               "N/A",
                                             FineDate = f.FineDate,
                                             FineType = f.FineType,
                                             FineAmount = f.FineByDate,
                                             FineStatus = f.Status
                                           })
                                           .ToList()
                                 })
                                 .ToListAsync();
        return Ok(new ApiPagedResponse<object>
        {
          Success = true,
          Data = transactions,
          Pagination = new PaginationMeta
          {
            CurrentPage = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
          }
        });
      }
      catch (Exception ex)
      {
        return StatusCode(500,
          new ApiResponse { Success = false, Message = "Lỗi server khi lấy danh sách giao dịch.", Error = ex.Message });
      }
    }
  }

  public class ApiResponse
  {
    public bool? Success { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public object? Data { get; set; }
  }

  public class ApiPagedResponse<T> : ApiResponse
  {
    public new T Data { get; set; }
    public PaginationMeta? Pagination { get; set; }
  }

  public class PaginationMeta
  {
    public int? CurrentPage { get; set; }
    public int? PageSize { get; set; }
    public int? TotalRecords { get; set; }
    public int? TotalPages { get; set; }
  }
}
