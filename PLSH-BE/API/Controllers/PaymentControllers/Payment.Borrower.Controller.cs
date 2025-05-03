using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Data.DatabaseContext;
using Model.Entity;
using Microsoft.AspNetCore.Cors;

namespace API.Controllers.PaymentControllers
{
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
            ILogger<PaymentController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpClient = httpClientFactory.CreateClient("VietQR") ?? throw new ArgumentNullException(nameof(httpClientFactory));
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
        [HttpGet("borrower/{borrowerId}")]
        public async Task<IActionResult> GetUnpaidFines(int borrowerId)
        {
            try
            {
                var fines = await _context.Fines
                    .Where(f => f.BorrowerId == borrowerId && f.IsFined && f.Status == "incomplete")
                    .Include(f => f.BookBorrowing)
                        .ThenInclude(bb => bb.BookInstance)
                            .ThenInclude(bi => bi.Book)
                    .Select(f => new FineDto
                    {
                        Id = f.Id,
                        FineDate = f.FineDate.ToString("o"),
                        FineType = f.FineType ?? 0,
                        Amount = (decimal)f.Amount,
                        Status = f.Status,
                        Note = f.Note,
                        BookTitle = f.BookBorrowing.BookInstance.Book.Title,
                        BookCode = f.BookBorrowing.BookInstance.Code,
                        BookImage = f.BookBorrowing.BookInstance.Book.Thumbnail,
                        BorrowDate = f.BookBorrowing.BorrowDate.ToString("o"),
                        ActualReturnDate = f.BookBorrowing.ActualReturnDate.HasValue
                            ? f.BookBorrowing.ActualReturnDate.Value.ToString("o")
                            : null,
                        ReturnDates = f.BookBorrowing.ReturnDates != null
                            ? f.BookBorrowing.ReturnDates.Select(rd => rd.ToString("o")).ToList()
                            : new List<string>(),
                        ExtendDate = f.BookBorrowing.ReturnDates != null && f.BookBorrowing.ReturnDates.Any()
                            ? f.BookBorrowing.ReturnDates.Last().ToString("o")
                            : null
                    })
                    .ToListAsync();
                return Ok(new PaymentResponseDto
                {
                    Success = true,
                    Data = fines,
                    Message = "Unpaid fines retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unpaid fines for borrower ID: {BorrowerId}", borrowerId);
                return StatusCode(500, new PaymentResponseDto
                {
                    Success = false,
                    Data = null,
                    Message = "Internal server error while retrieving unpaid fines"
                });
            }
        }


        [HttpPost("create-qr")]
        public async Task<IActionResult> CreatePaymentQR([FromBody] PaymentRequest request)
        {
            try
            {
                if (request == null || request.FineIds == null || request.FineIds.Count == 0)
                {
                    return BadRequest(new PaymentResponseDto
                    {
                        Success = false,
                        Data = null,
                        Message = "No fines selected"
                    });
                }
                var fines = await _context.Fines
                    .Where(f => request.FineIds.Contains(f.Id) && f.BorrowerId == request.BorrowerId)
                    .ToListAsync();

                if (fines.Count != request.FineIds.Count)
                {
                    return BadRequest(new PaymentResponseDto
                    {
                        Success = false,
                        Data = null,
                        Message = "Some fines not found or do not belong to this borrower"
                    });
                }
                var totalAmount = fines.Sum(f => f.Amount ?? 0);

                if (totalAmount <= 0)
                {
                    return BadRequest(new PaymentResponseDto
                    {
                        Success = false,
                        Data = null,
                        Message = "Invalid total amount"
                    });
                }
                var transaction = new Transaction
                {
                    AccountId = 1,
                    Amount = (decimal)totalAmount,
                    Currency = "VND",
                    Status = 1,
                    TransactionDate = DateTime.UtcNow,
                    ReferenceId = Guid.NewGuid().ToString(),
                    TransactionType = 1,
                    Note = $"Fine payment for fines: {string.Join(",", request.FineIds)}"
                };
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                foreach (var fine in fines)
                {
                    fine.TransactionId = transaction.Id;
                }
                await _context.SaveChangesAsync();
                var qrCodeUrl = $"https://img.vietqr.io/image/970422-0346716550-compact2.png?amount={totalAmount}&addInfo=FINE_{transaction.ReferenceId}";

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
                return StatusCode(500, new PaymentResponseDto
                {
                    Success = false,
                    Data = null,
                    Message = "Internal server error while creating payment QR"
                });
            }
        }
        [HttpPut("update-status/{referenceId}")]
        public async Task<IActionResult> UpdatePaymentStatus(string referenceId)
        {
            try
            {
                var transaction = await _context.Transactions
                    .Include(t => t.Fines)
                    .FirstOrDefaultAsync(t => t.ReferenceId == referenceId);

                if (transaction == null)
                {
                    return NotFound(new PaymentResponseDto
                    {
                        Success = false,
                        Data = null,
                        Message = "Transaction not found"
                    });
                }
                transaction.Status = 2;
                transaction.TransactionDate = DateTime.UtcNow;
                foreach (var fine in transaction.Fines)
                {
                    fine.IsFined = false;
                    fine.FineType = 3;
                    fine.Status = "completed";
                }
                await _context.SaveChangesAsync();

                return Ok(new PaymentResponseDto
                {
                    Success = true,
                    Data = new
                    {
                        TransactionId = transaction.Id,
                        UpdatedFines = transaction.Fines.Select(f => f.Id).ToList()
                    },
                    Message = "Payment status updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for reference ID: {ReferenceId}", referenceId);
                return StatusCode(500, new PaymentResponseDto
                {
                    Success = false,
                    Data = null,
                    Message = "Internal server error while updating payment status"
                });
            }
        }
    }
}
