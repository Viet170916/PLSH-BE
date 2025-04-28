using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Data.DatabaseContext;
using Model.Entity;
using Model.Entity.Borrow;

namespace API.Controllers.PaymentControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentController> _logger;
        private readonly string _vietQrClientId;
        private readonly string _vietQrApiKey;
        private readonly int _transactionTimeoutHours;
        private Dictionary<string, string> _bankCodes;

        public PaymentController(
            AppDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<PaymentController> logger)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient("VietQR");
            _logger = logger;
            _vietQrClientId = configuration["VietQR:ClientId"] ?? throw new ArgumentNullException("VietQR:ClientId");
            _vietQrApiKey = configuration["VietQR:ApiKey"] ?? throw new ArgumentNullException("VietQR:ApiKey");
            _transactionTimeoutHours = configuration.GetValue<int>("PaymentSettings:TransactionTimeoutHours", 24);
            _httpClient.DefaultRequestHeaders.Add("x-client-id", _vietQrClientId);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _vietQrApiKey);
        }

        [HttpPost("create-qr/{fineId}")]
        public async Task<IActionResult> CreateVietQR(int fineId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var fine = await _context.Fines
                .FirstOrDefaultAsync(f => f.Id == fineId && f.BorrowerId == int.Parse(userId));

            if (fine == null || !fine.IsFined || fine.Status == 3)
                return BadRequest("Phí phạt không hợp lệ hoặc đã thanh toán.");

            var paymentMethod = await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.PaymentType == "Bank" && pm.DeleteAt == null);

            if (paymentMethod == null)
                return BadRequest("Không tìm thấy phương thức thanh toán ngân hàng.");

            if (fine.FineByDate <= 0 || fine.FineByDate > 100000000)
                return BadRequest("Số tiền phạt không hợp lệ.");

            var amount = (int)Math.Round(fine.FineByDate);
            var referenceId = Guid.NewGuid().ToString("N").ToLower();
            var transaction = new Transaction
            {
                AccountId = int.Parse(userId),
                PaymentMethodId = paymentMethod.Id,
                Amount = amount,
                Currency = "VND",
                Status = 1,
                TransactionDate = DateTime.UtcNow,
                ReferenceId = referenceId,
                TransactionType = 1,
                Note = $"Thanh toán phí phạt {fineId}"
            };

            _context.Transactions.Add(transaction);
            fine.TransactionId = transaction.Id;
            fine.Status = 1;
            await _context.SaveChangesAsync();

            var requestBody = new
            {
                accountNo = paymentMethod.BankAccount,
                accountName = "BUI SY DAN",
                acqId = await GetBankAcqId(paymentMethod.BankName),
                amount,
                addInfo = $"Phi phat {fineId} {referenceId}",
                template = "compact"
            };

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("https://api.vietqr.io/v2/generate", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("VietQR API failed, attempt {Attempt}: {StatusCode}", attempt, response.StatusCode);
                        continue;
                    }

                    var result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                    string qrDataUrl = result?.data?.qrDataURL;
                    if (string.IsNullOrEmpty(qrDataUrl))
                        throw new Exception("Invalid VietQR response");

                    return Ok(new
                    {
                        QrCodeImageUrl = qrDataUrl,
                        transaction.Id,
                        Amount = amount,
                        transaction.ReferenceId,
                        paymentMethod.BankAccount,
                        paymentMethod.BankName
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "VietQR attempt {Attempt} failed", attempt);
                    if (attempt == 3)
                    {
                        _context.Transactions.Remove(transaction);
                        fine.TransactionId = null;
                        fine.Status = 0;
                        await _context.SaveChangesAsync();
                        return StatusCode(500, "Không thể tạo mã VietQR.");
                    }
                    await Task.Delay(1000 * attempt);
                }
            }

            return StatusCode(500, "Không thể tạo mã VietQR.");
        }

        [HttpPost("webhook/vietqr")]
        [AllowAnonymous]
        public async Task<IActionResult> VietQRWebhook([FromBody] VietQRWebhookModel model)
        {
            if (!ValidateSignature(model, Request.Headers["x-signature"], _vietQrApiKey))
            {
                _logger.LogWarning("Invalid VietQR webhook signature");
                return Unauthorized();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Fine)
                .FirstOrDefaultAsync(t => t.ReferenceId == model.ReferenceId);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found: {ReferenceId}", model.ReferenceId);
                return NotFound();
            }

            if (model.Amount != transaction.Amount)
            {
                _logger.LogWarning("Amount mismatch: {TransactionId}, Expected: {Expected}, Actual: {Actual}",
                    transaction.Id, transaction.Amount, model.Amount);
                return BadRequest("Số tiền không khớp.");
            }

            if (transaction.Status != 1)
            {
                _logger.LogInformation("Transaction {TransactionId} already processed", transaction.Id);
                return Ok();
            }

            if (model.Status == "success")
            {
                transaction.Status = 2;
                transaction.TransactionDate = DateTime.UtcNow;
                if (transaction.Fine != null)
                    transaction.Fine.Status = 3;
                _logger.LogInformation("Payment confirmed: {TransactionId}", transaction.Id);
            }
            else
            {
                transaction.Status = 3;
                if (transaction.Fine != null)
                    transaction.Fine.Status = 0;
                _logger.LogInformation("Payment failed: {TransactionId}", transaction.Id);
            }

            if (transaction.TransactionDate < DateTime.UtcNow.AddHours(-_transactionTimeoutHours))
            {
                transaction.Status = 3;
                if (transaction.Fine != null)
                    transaction.Fine.Status = 0;
                _logger.LogInformation("Transaction {TransactionId} timed out", transaction.Id);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("status/{transactionId}")]
        public async Task<IActionResult> GetPaymentStatus(int transactionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.AccountId ==int.Parse (userId));

            if (transaction == null)
                return NotFound();

            return Ok(new
            {
                transaction.Status,
                StatusText = transaction.Status switch
                {
                    1 => "Chờ thanh toán",
                    2 => "Đã thanh toán",
                    3 => "Đã hủy",
                    _ => "Không xác định"
                },
                transaction.Amount,
                transaction.Currency,
                PaidDate = transaction.Status == 2 ? transaction.TransactionDate : (DateTime?)null,
                transaction.ReferenceId
            });
        }

        private async Task<string> GetBankAcqId(string bankName)
        {
            _bankCodes ??= await InitializeBankCodes();
            return _bankCodes.TryGetValue(bankName, out var code) ? code : "970418";
        }

        private async Task<Dictionary<string, string>> InitializeBankCodes()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://api.vietqr.io/v2/banks");
                if (response.IsSuccessStatusCode)
                {
                    var banks = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                    var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var bank in banks.data)
                    {
                        dict[(string)bank.name] = (string)bank.code;
                    }
                    return dict;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load bank codes");
            }

            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["BIDV"] = "970418",
                ["Vietcombank"] = "970436",
                ["MB Bank"] = "970415"
            };
        }

        private bool ValidateSignature(VietQRWebhookModel model, string signature, string apiKey)
        {
            if (string.IsNullOrEmpty(signature))
                return false;

            var payload = JsonConvert.SerializeObject(model, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            });

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiKey));
            var computed = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
            return computed.Equals(signature, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class VietQRWebhookModel
    {
        public string ReferenceId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime TransactionDate { get; set; }
    }

}