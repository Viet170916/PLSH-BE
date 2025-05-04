using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Authorization;
using Model.Entity;
using Newtonsoft.Json;

namespace API.Controllers.PaymentControllers;

[Route("api/v1/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
  private readonly AppDbContext _context;
  private readonly HttpClient _httpClient;
  private readonly ILogger<PaymentController> _logger;
  private readonly string _vietQrClientId;
  private readonly string _vietQrApiKey;

  public PaymentController(
    AppDbContext context,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<PaymentController> logger
  )
  {
    _context = context ?? throw new ArgumentNullException(nameof(context));
    _httpClient = httpClientFactory.CreateClient("VietQR") ??
                  throw new ArgumentNullException(nameof(httpClientFactory));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _vietQrClientId = configuration["VietQR:ClientId"] ?? throw new ArgumentNullException("VietQR:ClientId");
    _vietQrApiKey = configuration["VietQR:ApiKey"] ?? throw new ArgumentNullException("VietQR:ApiKey");
    _httpClient.DefaultRequestHeaders.Add("x-client-id", _vietQrClientId);
    _httpClient.DefaultRequestHeaders.Add("x-api-key", _vietQrApiKey);
  }

  public class FineDto
  {
    public int Id { get; set; }
    public string? FineDate { get; set; }
    public int FineType { get; set; }
    public decimal? Amount { get; set; }
    public string Status { get; set; }
    public string? Note { get; set; }
    public string? BookTitle { get; set; }
    public string? BookImage { get; set; }
    public string? BookCode { get; set; }
    public string? BorrowDate { get; set; }
    public string? ActualReturnDate { get; set; }
    public List<string>? ReturnDates { get; set; }
    public string? ExtendDate { get; set; }
  }

  public class PaymentResponseDto
  {
    public bool Success { get; set; }
    public object? Data { get; set; }
    public string? Message { get; set; }
  }

  public class QrCodeDataDto
  {
    public string? QrCode { get; set; }
    public string? ReferenceId { get; set; }
    public decimal? Amount { get; set; }
    public string? Description { get; set; }
  }

  public class PaymentRequest
  {
    public int BorrowerId { get; set; }
    public List<int>? FineIds { get; set; }
  }

  public class SheetTransaction
  {
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
  }

  [Authorize("BorrowerPolicy")] [HttpGet("borrower/{borrowerId}")]
  public async Task<IActionResult> GetUnpaidFines(int borrowerId)
  {
    var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    try
    {
      var fines = await _context.Fines
                                .Where(f => f.BorrowerId == accountId && f.IsFined && f.Status == "incomplete")
                                .Include(f => f.BookBorrowing)
                                .ThenInclude(bb => bb.BookInstance)
                                .ThenInclude(bi => bi.Book)
                                .Select(f => new
                                {
                                  f.Id,
                                  f.FineDate,
                                  f.FineType,
                                  f.Amount,
                                  f.Status,
                                  f.Note,
                                  BookTitle = f.BookBorrowing.BookInstance.Book.Title,
                                  BookCode = f.BookBorrowing.BookInstance.Code,
                                  BookImage = f.BookBorrowing.BookInstance.Book.Thumbnail,
                                  f.BookBorrowing.BorrowDate,
                                  f.BookBorrowing.ActualReturnDate,
                                  f.BookBorrowing.ReturnDates
                                })
                                .ToListAsync();
      var fineDtos = fines.Select(f => new FineDto
                          {
                            Id = f.Id,
                            FineDate = f.FineDate.ToString("o"),
                            FineType = f.FineType ?? 0,
                            Amount = (decimal)f.Amount!,
                            Status = f.Status,
                            Note = f.Note,
                            BookTitle = f.BookTitle,
                            BookCode = f.BookCode,
                            BookImage = f.BookImage,
                            BorrowDate = f.BorrowDate.ToString("o"),
                            ActualReturnDate = f.ActualReturnDate?.ToString("o"),
                            ReturnDates =
                              f.ReturnDates?.Select(rd => rd.ToString("o")).ToList() ?? new List<string>(),
                            ExtendDate = f.ReturnDates?.LastOrDefault().ToString("o")
                          })
                          .ToList();
      return Ok(new PaymentResponseDto
      {
        Success = true, Data = fineDtos, Message = "Unpaid fines retrieved successfully"
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting unpaid fines for borrower ID: {AccountId}", accountId);
      return StatusCode(500,
        new PaymentResponseDto
        {
          Success = false, Data = null, Message = "Internal server error while retrieving unpaid fines"
        });
    }
  }

  [HttpPost("create-qr")] public async Task<IActionResult> CreatePaymentQR([FromBody] PaymentRequest request)
  {
    try
    {
      var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
      if (request.FineIds == null || request.FineIds.Count == 0)
      {
        return BadRequest(new PaymentResponseDto { Success = false, Data = null, Message = "No fines selected" });
      }

      var fines = await _context.Fines
                                .Where(f => request.FineIds.Contains(f.Id) && f.BorrowerId == accountId)
                                .ToListAsync();
      if (fines.Count != request.FineIds.Count)
      {
        return BadRequest(new PaymentResponseDto
        {
          Success = false, Data = null, Message = "Some fines not found or do not belong to this borrower"
        });
      }

      var totalAmount = fines.Sum(f => f.Amount ?? 0);
      if (totalAmount <= 0)
      {
        return BadRequest(new PaymentResponseDto { Success = false, Data = null, Message = "Invalid total amount" });
      }

      var transaction = new Transaction
      {
        AccountId = accountId,
        Amount = (decimal)totalAmount,
        Currency = "VND",
        Status = 2,
        TransactionDate = DateTime.UtcNow,
        ReferenceId = Guid.NewGuid().ToString(),
        TransactionType = 1,
        Note = $"Fine payment for fines: {string.Join(",", request.FineIds)}"
      };
      _context.Transactions.Add(transaction);
      await _context.SaveChangesAsync();
      foreach (var fine in fines) { fine.TransactionId = transaction.Id; }

      await _context.SaveChangesAsync();
      var qrCodeUrl =
        $"https://img.vietqr.io/image/970422-0346716550-compact2.png?amount={totalAmount}&addInfo=FINE_{transaction.ReferenceId}";
      return Ok(new PaymentResponseDto
      {
        Success = true,
        Data = new QrCodeDataDto
        {
          QrCode = qrCodeUrl,
          ReferenceId = transaction.ReferenceId,
          Amount = (decimal)totalAmount,
          Description = $"FINE {transaction.ReferenceId}"
        },
        Message = "QR code created successfully"
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error while creating QR for payment");
      return StatusCode(500,
        new PaymentResponseDto
        {
          Success = false, Data = null, Message = "Internal server error while creating payment QR"
        });
    }
  }

  [HttpPut("update-status/{referenceId}")]
  public async Task<IActionResult> UpdatePaymentStatus(string referenceId)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(referenceId))
      {
        return BadRequest(new PaymentResponseDto { Success = false, Message = "Reference ID is required" });
      }

      var transaction = await _context.Transactions
                                      .Include(t => t.Fines)
                                      .FirstOrDefaultAsync(t => t.ReferenceId == referenceId);
      if (transaction == null)
      {
        return NotFound(new PaymentResponseDto { Success = false, Message = "Transaction not found" });
      }

      var externalApiUrl =
        "https://script.googleusercontent.com/a/macros/fpt.edu.vn/echo?user_content_key=AehSKLj7hZgPrG52nVJpBKgDVzpSmhuJiVjgSoTsOY1TXAeRHjLus-NdG8DthgjTkRPuCbk_AqGgTP4HJ0cKZOcti0LvVPDg7Hsiku3FokROeaeIlQw96EPHHc_JhpuGjMmcbEjTEyR0vzG9tk7EmgNMr45f1YJNBmS90qQLCItQLJ5Mfl5tiKWu4aaVFTF6voivLGLMA6FSz_5mKnsN8gIejFZMEcSUOV-HV1ohfC0qDbGnKC2ukF2xixMtP-YyRzdQzN5hRXcBcf77OkqFhJ-7-BX5h9zYOjI49pvflWkedvEDUFLg6h0&lib=MW1Uyq0glv2m5c--HH9TlZU-LqlQWFUuU";
      var response = await _httpClient.GetAsync(externalApiUrl);
      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError("Failed to connect to external API. StatusCode: {StatusCode}", response.StatusCode);
        return StatusCode(502, new PaymentResponseDto { Success = false, Message = "Failed to connect to bank API" });
      }

      var content = await response.Content.ReadAsStringAsync();
      var externalData = JsonConvert.DeserializeObject<ExternalApiResponse>(content);
      if (externalData == null || externalData.Data == null || externalData.Data.Count < 2)
      {
        return BadRequest(new PaymentResponseDto
        {
          Success = false, Message = "Insufficient transaction data from bank API"
        });
      }

      var secondTransaction = externalData.Data[1];
      var cleanedReferenceId = referenceId.Replace(" ", "").Replace("-", "").Replace("_", "");
      var cleanedDescription = string.IsNullOrEmpty(secondTransaction.Description) ?
        "" :
        secondTransaction.Description.Replace(" ", "").Replace("-", "").Replace("_", "");
      if (string.IsNullOrEmpty(cleanedDescription) || !cleanedDescription.Contains(cleanedReferenceId))
      {
        return Ok(new PaymentResponseDto
        {
          Success = false, Message = "Transaction not recorded in bank (checked second transaction)"
        });
      }

      if (!DateTime.TryParseExact(secondTransaction.TransactionDate,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsedTransactionDate))
      {
        return StatusCode(500,
          new PaymentResponseDto { Success = false, Message = "Invalid transaction date format from bank API" });
      }

      transaction.Status = 1;
      transaction.TransactionDate = parsedTransactionDate;
      if (transaction.Fines != null)
      {
        foreach (var fine in transaction.Fines)
        {
          fine.IsFined = false;
          fine.Status = "completed";
        }
      }

      await _context.SaveChangesAsync();
      return Ok(new PaymentResponseDto
      {
        Success = true,
        Data = new { TransactionId = transaction.Id },
        Message = "Transaction status updated successfully"
      });
    }
    catch (Exception ex)
    {
      return StatusCode(500, new PaymentResponseDto { Success = false, Message = "Internal server error" });
    }
  }

  public class ExternalTransaction
  {
    [JsonProperty("Mã GD")]
    public string TransactionId { get; set; }

    [JsonProperty("Mô tả")]
    public string Description { get; set; }

    [JsonProperty("Giá trị")]
    public decimal Amount { get; set; }

    [JsonProperty("Ngày diễn ra")]
    public string TransactionDate { get; set; }

    [JsonProperty("Số tài khoản")]
    public string AccountNumber { get; set; }
  }

  public class ExternalApiResponse
  {
    public List<ExternalTransaction> Data { get; set; }
    public bool Error { get; set; }
  }

  [HttpGet("fine-for-lib")] public async Task<IActionResult> GetAllFines(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string search = "",
    [FromQuery] string status = "",
    [FromQuery] DateTime? fromDate = null,
    [FromQuery] DateTime? toDate = null,
    [FromQuery] decimal? minAmount = null,
    [FromQuery] decimal? maxAmount = null,
    [FromQuery] string sortBy = "fineDate",
    [FromQuery] bool sortDesc = true,
    [FromQuery] int? borrowerId = null,
    [FromQuery] int? fineType = null
  )
  {
    try
    {
      // Validate input parameters
      if (page < 1) page = 1;
      if (pageSize < 1 || pageSize > 100) pageSize = 10;
      if (toDate.HasValue && fromDate.HasValue && toDate < fromDate)
      {
        return BadRequest(new PaymentResponseDto
        {
          Success = false, Message = "End date must be greater than or equal to start date"
        });
      }

      // Build base query with includes
      var query = _context.Fines
                          .Include(f => f.BookBorrowing)
                          .ThenInclude(bb => bb.BookInstance)
                          .ThenInclude(bi => bi.Book)
                          .AsQueryable();

      // Apply filters
      if (borrowerId.HasValue) query = query.Where(f => f.BorrowerId == borrowerId.Value);
      if (fineType.HasValue) query = query.Where(f => f.FineType == fineType.Value);
      if (!string.IsNullOrWhiteSpace(search))
      {
        query = query.Where(f =>
          f.Note.Contains(search) ||
          (f.BookBorrowing != null &&
           f.BookBorrowing.BookInstance != null &&
           f.BookBorrowing.BookInstance.Book != null &&
           f.BookBorrowing.BookInstance.Book.Title.Contains(search)) ||
          (f.BookBorrowing != null &&
           f.BookBorrowing.BookInstance != null &&
           f.BookBorrowing.BookInstance.Code.Contains(search)));
      }

      if (!string.IsNullOrWhiteSpace(status)) query = query.Where(f => f.Status == status);
      if (fromDate.HasValue) query = query.Where(f => f.FineDate >= fromDate.Value);
      if (toDate.HasValue) query = query.Where(f => f.FineDate <= toDate.Value);
      if (minAmount.HasValue) query = query.Where(f => f.Amount >= (double)minAmount.Value);
      if (maxAmount.HasValue) query = query.Where(f => f.Amount <= (double)maxAmount.Value);

      // Apply sorting
      query = sortBy.ToLower() switch
      {
        "amount" => sortDesc ? query.OrderByDescending(f => f.Amount) : query.OrderBy(f => f.Amount),
        "finedate" => sortDesc ? query.OrderByDescending(f => f.FineDate) : query.OrderBy(f => f.FineDate),
        "booktitle" => sortDesc ?
          query.OrderByDescending(f => f.BookBorrowing.BookInstance.Book.Title) :
          query.OrderBy(f => f.BookBorrowing.BookInstance.Book.Title),
        _ => sortDesc ? query.OrderByDescending(f => f.FineDate) : query.OrderBy(f => f.FineDate)
      };

      // Get total count before pagination
      var totalRecords = await query.CountAsync();

      // Apply pagination and select only needed fields for database query
      var finesData = await query
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .Select(f => new
                            {
                              Fine = f,
                              BookTitle = f.BookBorrowing != null &&
                                          f.BookBorrowing.BookInstance != null &&
                                          f.BookBorrowing.BookInstance.Book != null ?
                                f.BookBorrowing.BookInstance.Book.Title :
                                null,
                              BookImage = f.BookBorrowing != null &&
                                          f.BookBorrowing.BookInstance != null &&
                                          f.BookBorrowing.BookInstance.Book != null ?
                                f.BookBorrowing.BookInstance.Book.Thumbnail :
                                null,
                              BookCode = f.BookBorrowing != null &&
                                         f.BookBorrowing.BookInstance != null ?
                                f.BookBorrowing.BookInstance.Code :
                                null,
                              BorrowDate = f.BookBorrowing!.BorrowDate,
                              ActualReturnDate = f.BookBorrowing != null ? f.BookBorrowing.ActualReturnDate : null,
                              ReturnDates = f.BookBorrowing != null ? f.BookBorrowing.ReturnDates : null
                            })
                            .ToListAsync();

      // Transform data in memory (client-side)
      var fines = finesData.Select(x => new FineDto
                           {
                             Id = x.Fine.Id,
                             FineDate = x.Fine.FineDate.ToString("o"),
                             FineType = x.Fine.FineType ?? 0,
                             Amount = (decimal)(x.Fine.Amount ?? 0),
                             Status = x.Fine.Status,
                             Note = x.Fine.Note,
                             BookTitle = x.BookTitle ?? "Unknown",
                             BookImage = x.BookImage,
                             BookCode = x.BookCode,
                             BorrowDate = x.BorrowDate.ToString("o"),
                             ActualReturnDate = x.ActualReturnDate?.ToString("o"),
                             ReturnDates = x.ReturnDates?.Select(rd => rd.ToString("o")).ToList(),
                             ExtendDate = x.ReturnDates?.LastOrDefault().ToString("o")
                           })
                           .ToList();
      return Ok(new
      {
        Success = true,
        Data = fines,
        Pagination = new
        {
          CurrentPage = page,
          PageSize = pageSize,
          TotalRecords = totalRecords,
          TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
        },
        Message = "Fines retrieved successfully"
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting all fines");
      return StatusCode(500,
        new PaymentResponseDto
        {
          Success = false, Message = "Internal server error while retrieving fines", Data = null
        });
    }
  }
}
