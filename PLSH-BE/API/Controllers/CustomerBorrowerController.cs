using API.DTO.Account;
using BU.Services.Interface;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Triangulate.Tri;
using API.DTO.Account.AccountDTO;
using Model.ResponseModel;
using Common.Helper;
using API.DTO.Loan;
using API.DTO.Favorite;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Common.Library;
using API.DTO.Borrower;
using API.DTO.Borrower.Extend;
using API.DTO.ReserveBook;
using Common.Enums;
using Microsoft.AspNetCore.Http;
using Model.Entity;
using Model.Entity.User;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerBorrowerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ManageAccountController> _logger;
        private readonly IEmailService _emailService;
        public CustomerBorrowerController(AppDbContext context, ILogger<ManageAccountController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        //Hàm gửi email
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Status = HttpStatus.BAD_REQUEST.GetDescription(),
                    StatusCode = HttpStatus.BAD_REQUEST,
                    Message = "Dữ liệu không hợp lệ.",
                    Data = ModelState
                });
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == model.Email);
            if (account == null)
            {
                return NotFound(new ErrorResponse
                {
                    Status = HttpStatus.NOT_FOUND.GetDescription(),
                    StatusCode = HttpStatus.NOT_FOUND,
                    Message = "Không tìm thấy tài khoản với email này.",
                    Data = new { IsAuthorized = false }
                });
            }

            // Sử dụng hàm GenerateJwt để tạo token reset mật khẩu
            var resetToken = GenerateJwt(account);

            // Tạo link reset mật khẩu kèm token
            var resetLink = $"https://localhost:7147/api/CustomerBorrower/reset-password?token={resetToken}";
            var message = $"Vui lòng nhấp vào liên kết sau để đặt lại mật khẩu của bạn: <a href='{resetLink}'>Đặt lại mật khẩu</a>";

            await _emailService.SendEmailAsync(account.Email, "Đặt lại mật khẩu", message);

            return Ok(new OkResponse
            {
                Status = HttpStatus.OK.GetDescription(),
                StatusCode = HttpStatus.OK,
                Message = "Email đặt lại mật khẩu đã được gửi.",
                Data = new { EmailSent = true }
            });
        }

        //Hàm Generate JWT Token
        private string GenerateJwt(Account user)
        {
            var claims = new[]
            {
     // new Claim(ClaimTypes.Name, user.FullName), new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new Claim(ClaimTypes.Email, user.Email),
    };
            var key = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable(Constants.JWT_SECRET) ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer: Constants.Issuer,
              audience: Constants.Audience,
              claims: claims,
              expires: DateTime.Now.AddDays(1),
              signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        //Hàm đặt lại mật khẩu và kiểm tra mật khẩu mới khác 3 mật khẩu trước đó
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Status = HttpStatus.BAD_REQUEST.GetDescription(),
                    StatusCode = HttpStatus.BAD_REQUEST,
                    Message = "Dữ liệu không hợp lệ.",
                    Data = ModelState
                });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable(Constants.JWT_SECRET) ?? string.Empty);

            try
            {
                // Giải mã token
                var principal = tokenHandler.ValidateToken(model.Token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = Constants.Issuer,
                    ValidateAudience = true,
                    ValidAudience = Constants.Audience,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                // Lấy thông tin user từ token
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Status = HttpStatus.BAD_REQUEST.GetDescription(),
                        StatusCode = HttpStatus.BAD_REQUEST,
                        Message = "Token không hợp lệ.",
                        Data = new { IsAuthorized = false }
                    });
                }

                int accountId = int.Parse(userIdClaim.Value);
                var account = await _context.Accounts.FindAsync(accountId);
                if (account == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Status = HttpStatus.NOT_FOUND.GetDescription(),
                        StatusCode = HttpStatus.NOT_FOUND,
                        Message = "Không tìm thấy tài khoản.",
                        Data = new { IsAuthorized = false }
                    });
                }

                // Lấy danh sách 3 mật khẩu gần nhất từ PasswordAudit
                var recentPasswords = await _context.PasswordAudits
                    .Where(p => p.AccountId == accountId)
                    .OrderByDescending(p => p.ChangedAt)
                    .Take(3)
                    .Select(p => p.HashedPassword)
                    .ToListAsync();

                // Kiểm tra xem mật khẩu mới có trùng với 3 mật khẩu cũ không
                if (recentPasswords.Any(oldPass => BCrypt.Net.BCrypt.Verify(model.NewPassword, oldPass)))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Status = HttpStatus.BAD_REQUEST.GetDescription(),
                        StatusCode = HttpStatus.BAD_REQUEST,
                        Message = "Mật khẩu mới không được trùng với 3 mật khẩu gần nhất.",
                        Data = new { PasswordReset = false }
                    });
                }

                // Lưu mật khẩu cũ vào PasswordAudit trước khi cập nhật
                _context.PasswordAudits.Add(new PasswordAudit
                {
                    AccountId = accountId,
                    HashedPassword = account.Password, // Lưu mật khẩu cũ đã mã hóa
                    ChangedAt = DateTime.UtcNow
                });

                // Cập nhật mật khẩu mới
                account.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                account.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new OkResponse
                {
                    Status = HttpStatus.OK.GetDescription(),
                    StatusCode = HttpStatus.OK,
                    Message = "Mật khẩu đã được đặt lại thành công.",
                    Data = new { PasswordReset = true }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Status = HttpStatus.BAD_REQUEST.GetDescription(),
                    StatusCode = HttpStatus.BAD_REQUEST,
                    Message = "Token không hợp lệ hoặc đã hết hạn.",
                    Data = new { Error = ex.Message }
                });
            }
        }


        //Cập nhật thông tin cá nhân(các trường cập nhật mới cập nhật)
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Status = HttpStatus.BAD_REQUEST.GetDescription(),
                    StatusCode = HttpStatus.BAD_REQUEST,
                    Message = "Dữ liệu không hợp lệ.",
                    Data = ModelState
                });
            }

            var account = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == model.Id || a.Email == model.Email);

            if (account == null)
            {
                return NotFound(new ErrorResponse
                {
                    Status = HttpStatus.NOT_FOUND.GetDescription(),
                    StatusCode = HttpStatus.NOT_FOUND,
                    Message = "Không tìm thấy tài khoản.",
                    Data = new { IsAuthorized = false }
                });
            }

            // Cập nhật thông tin cá nhân
            bool isUpdated = false;

            if (model.FullName != null && model.FullName != account.FullName)
            {
                account.FullName = model.FullName;
                isUpdated = true;
            }
            if (model.PhoneNumber != null && model.PhoneNumber != account.PhoneNumber)
            {
                account.PhoneNumber = model.PhoneNumber;
                isUpdated = true;
            }
            if (model.Address != null && model.Address != account.Address)
            {
                account.Address = model.Address;
                isUpdated = true;
            }
            if (model.AvatarUrl != null && model.AvatarUrl != account.AvatarUrl)
            {
                account.AvatarUrl = model.AvatarUrl;
                isUpdated = true;
            }
            if (model.Birthdate != DateTime.MinValue && model.Birthdate != account.Birthdate)
            {
                account.Birthdate = model.Birthdate;
                isUpdated = true;
            }
            if (model.Gender.HasValue && model.Gender != account.Gender)
            {
                account.Gender = model.Gender;
                isUpdated = true;
            }
            if (model.IdentityCardNumber != null && model.IdentityCardNumber != account.IdentityCardNumber)
            {
                account.IdentityCardNumber = model.IdentityCardNumber;
                isUpdated = true;
            }

            if (isUpdated)
            {
                account.UpdatedAt = DateTime.UtcNow;
                _context.Accounts.Update(account);
                await _context.SaveChangesAsync();
            }

            var result = new
            {
                account.Id,
                account.FullName,
                account.Email,
                account.PhoneNumber,
                account.Address,
                account.AvatarUrl,
                account.Birthdate,
                account.Gender,
                account.IdentityCardNumber
            };

            return Ok(new OkResponse
            {
                Status = HttpStatus.OK.GetDescription(),
                StatusCode = HttpStatus.OK,
                Message = isUpdated ? "Cập nhật thông tin thành công." : "Không có thay đổi nào.",
                Data = result
            });
        }

        //Đặt mượn sách online và gửi thông báo xét duyệt và tối đa 5 cuốn
        [HttpPost("request-borrowing")]
        public async Task<IActionResult> RequestBorrowing([FromBody] BorrowingRequestDto request)
        {
            if (request.BookDetailIds == null || !request.BookDetailIds.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Status = HttpStatus.BAD_REQUEST.GetDescription(),
                    StatusCode = HttpStatus.BAD_REQUEST,
                    Message = "Danh sách sách mượn không được để trống."
                });
            }

            if (request.BookDetailIds.Count > 5)
            {
                return BadRequest(new ErrorResponse
                {
                    Status = HttpStatus.BAD_REQUEST.GetDescription(),
                    StatusCode = HttpStatus.BAD_REQUEST,
                    Message = "Không thể mượn quá 5 cuốn sách."
                });
            }

            var loan = new Loan
            {
                BorrowerId = request.BorrowerId,
                BorrowingDate = DateTime.UtcNow,
                AprovalStatus = 0,
                Note = request.Note
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            foreach (var bookDetailId in request.BookDetailIds)
            {
                var bookBorrowing = new BookBorrowing
                {
                    LoanId = loan.Id,
                    BookDetailId = bookDetailId,
                    BorrowingStatus = 0,
                    BorrowingDate = DateTime.UtcNow
                };
                _context.BookBorrowings.Add(bookBorrowing);
            }
            await _context.SaveChangesAsync();

            var notification = new Notification
            {
                Title = "Yêu cầu mượn sách mới",
                Content = $"Người dùng có mã {request.BorrowerId} đã gửi yêu cầu mượn sách.",
                Date = DateTime.UtcNow,
                Status = 0,
                AccountId = request.LibrarianId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(new OkResponse
            {
                Status = HttpStatus.OK.GetDescription(),
                StatusCode = HttpStatus.OK,
                Message = "Yêu cầu mượn sách đã được gửi thành công.",
                Data = new { LoanId = loan.Id }
            });
        }




        // Theo dõi lịch sử mượn trả và phân trang (10/1 trang)
        [HttpGet("borrowing-history/{borrowerId}")]
        public async Task<IActionResult> GetBorrowingHistory(int borrowerId, int pageNumber = 1, int pageSize = 10)
        {
            var currentDate = DateTime.UtcNow;
            var loansToNotify = await _context.Loans
                .Where(loan => loan.BorrowerId == borrowerId && loan.ReturnDate.HasValue && loan.ReturnDate.Value.AddDays(-3) <= currentDate && loan.ReturnDate.Value >= currentDate)
                .ToListAsync();

            foreach (var loan in loansToNotify)
            {
                var notification = new Notification
                {
                    Title = "Thông báo gia hạn mượn sách",
                    Content = $"Sách bạn mượn sẽ hết hạn trong 3 ngày tới. Vui lòng gia hạn nếu cần.",
                    Date = DateTime.UtcNow,
                    Status = 0,
                    AccountId = borrowerId
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            var totalCount = await _context.Loans
                .Where(loan => loan.BorrowerId == borrowerId)
                .CountAsync();

            var loans = await (from loan in _context.Loans
                               join bb in _context.BookBorrowings on loan.Id equals bb.LoanId
                               join bd in _context.BookDetails on bb.BookDetailId equals bd.Id
                               join book in _context.Books on bd.BookId equals book.Id
                               where loan.BorrowerId == borrowerId
                               orderby loan.BorrowingDate descending
                               select new
                               {
                                   loan.Id,
                                   LoanBorrowingDate = loan.BorrowingDate,
                                   LoanReturnDate = loan.ReturnDate,
                                   loan.AprovalStatus,
                                   BookId = bb.Id,
                                   BookTitle = book.Title,
                                   BorrowingStatus = bb.BorrowingStatus,
                                   BorrowingDate = bb.BorrowingDate,
                                   BorrowingReturnDate = bb.ReturnDate
                               }).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            if (!loans.Any())
            {
                return Ok(new ErrorResponse
                {
                    Status = HttpStatus.NOT_FOUND.GetDescription(),
                    StatusCode = HttpStatus.NOT_FOUND,
                    Message = "Không có lịch sử mượn trả.",
                    Data = new { HasHistory = false }
                });
            }

            return Ok(new OkResponse
            {
                Status = HttpStatus.OK.GetDescription(),
                StatusCode = HttpStatus.OK,
                Message = "Lịch sử mượn trả được tải thành công.",
                Data = new
                {
                    TotalItems = totalCount,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    Loans = loans
                }
            });
        }

        //Gia hạn sách online
        [HttpPost("extend-borrowing")]
        public async Task<IActionResult> ExtendBorrowing([FromBody] ExtendBorrowingRequestDto request)
        {
            var loan = await (from l in _context.Loans
                              join bb in _context.BookBorrowings on l.Id equals bb.LoanId
                              where l.Id == request.LoanId
                              select l).FirstOrDefaultAsync();

            if (loan == null)
            {
                return NotFound(new ErrorResponse
                {
                    Status = HttpStatus.NOT_FOUND.GetDescription(),
                    StatusCode = HttpStatus.NOT_FOUND,
                    Message = "Không tìm thấy thông tin mượn sách."
                });
            }

            if (loan.ReturnDate.HasValue && loan.ReturnDate.Value < DateTime.UtcNow)
            {
                return BadRequest(new ErrorResponse
                {
                    Status = HttpStatus.BAD_REQUEST.GetDescription(),
                    StatusCode = HttpStatus.BAD_REQUEST,
                    Message = "Không thể gia hạn do sách đã quá hạn trả."
                });
            }

            if (loan.ExtensionCount >= 2)
            {
                return BadRequest(new ErrorResponse
                {
                    Status = HttpStatus.BAD_REQUEST.GetDescription(),
                    StatusCode = HttpStatus.BAD_REQUEST,
                    Message = "Không thể gia hạn quá 2 lần."
                });
            }

            loan.ReturnDate = loan.ReturnDate.HasValue ? loan.ReturnDate.Value.AddDays(7) : DateTime.UtcNow.AddDays(7);
            loan.ExtensionCount++;
            await _context.SaveChangesAsync();

            var notification = new Notification
            {
                Title = "Yêu cầu gia hạn mượn sách",
                Content = $"Người dùng có mã {loan.BorrowerId} đã gửi yêu cầu gia hạn mượn sách.",
                Date = DateTime.UtcNow,
                Status = 0,
                AccountId = request.LibrarianId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(new OkResponse
            {
                Status = HttpStatus.OK.GetDescription(),
                StatusCode = HttpStatus.OK,
                Message = "Yêu cầu gia hạn mượn sách đã được gửi thành công.",
                Data = new { LoanId = loan.Id, NewReturnDate = loan.ReturnDate }
            });
        }


        // Đăng ký trả sách online
        [HttpPost("return-book")]
        public async Task<IActionResult> ReturnBook([FromBody] ReturnBookRequest request)
        {
            if (request == null || request.BookBorrowingId <= 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Status = HttpStatus.BAD_REQUEST.GetDescription(),
                    StatusCode = HttpStatus.BAD_REQUEST,
                    Message = "Dữ liệu không hợp lệ."
                });
            }

            var bookBorrowing = await (from bb in _context.BookBorrowings
                                       join loan in _context.Loans on bb.LoanId equals loan.Id
                                       where bb.Id == request.BookBorrowingId
                                       select new
                                       {
                                           BookBorrowing = bb,
                                           Loan = loan
                                       }).FirstOrDefaultAsync();

            if (bookBorrowing == null)
            {
                return NotFound(new ErrorResponse
                {
                    Status = HttpStatus.NOT_FOUND.GetDescription(),
                    StatusCode = HttpStatus.NOT_FOUND,
                    Message = "Không tìm thấy thông tin mượn sách."
                });
            }

            bookBorrowing.BookBorrowing.BorrowingStatus = BorrowingStatus.Returned; // 2: Returned

            var bookDetail = await _context.BookDetails.FirstOrDefaultAsync(bd => bd.Id == bookBorrowing.BookBorrowing.BookDetailId);
            if (bookDetail != null)
            {
                bookDetail.Status = 1; // 1: Available
            }

            var notification = new Notification
            {
                Title = "Xác nhận trả sách",
                Content = "Sách của bạn đã được trả thành công.",
                Date = DateTime.UtcNow,
                Status = 0,
                AccountId = bookBorrowing.Loan.BorrowerId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(new OkResponse
            {
                Status = HttpStatus.OK.GetDescription(),
                StatusCode = HttpStatus.OK,
                Message = "Trả sách thành công.",
                Data = new { BookBorrowingId = request.BookBorrowingId }
            });
        }


        public class ReturnBookRequest
        {
            public int BookBorrowingId { get; set; }
        }


        // Xem tình trạng yêu cầu
        [HttpGet("request-status/{id}")]
        public async Task<IActionResult> GetRequestStatus(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Status = HttpStatus.BAD_REQUEST.GetDescription(),
                    StatusCode = HttpStatus.BAD_REQUEST,
                    Message = "Id không hợp lệ."
                });
            }

            var requestStatus = await (from bb in _context.BookBorrowings
                                       join loan in _context.Loans on bb.LoanId equals loan.Id
                                       join account in _context.Accounts on loan.BorrowerId equals account.Id
                                       where bb.Id == id
                                       select new
                                       {
                                           BorrowingId = bb.Id,
                                           BorrowingStatus = bb.BorrowingStatus,
                                           BorrowerName = account.FullName,
                                           BorrowDate = bb.BorrowingDate,
                                           ReturnDate = bb.ReturnDate
                                       }).FirstOrDefaultAsync();

            if (requestStatus == null)
            {
                return NotFound(new ErrorResponse
                {
                    Status = HttpStatus.NOT_FOUND.GetDescription(),
                    StatusCode = HttpStatus.NOT_FOUND,
                    Message = "Không tìm thấy yêu cầu."
                });
            }

            return Ok(new OkResponse
            {
                Status = HttpStatus.OK.GetDescription(),
                StatusCode = HttpStatus.OK,
                Message = "Trạng thái yêu cầu.",
                Data = requestStatus
            });
        }

        //Nhận thông báo mượn trả 


        //Đặt chỗ sách
        [HttpPost("reserve-book")]
        public async Task<IActionResult> ReserveBook([FromBody] ReserveBookRequestDto request)
        {
            if (request.BookDetailId <= 0 || request.AccountId <= 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Status = HttpStatus.BAD_REQUEST.GetDescription(),
                    StatusCode = HttpStatus.BAD_REQUEST,
                    Message = "Thông tin không hợp lệ."
                });
            }

            var bookDetail = await _context.BookDetails.FirstOrDefaultAsync(b => b.Id == request.BookDetailId);
            if (bookDetail == null || bookDetail.Status != 1)
            {
                return NotFound(new ErrorResponse
                {
                    Status = HttpStatus.NOT_FOUND.GetDescription(),
                    StatusCode = HttpStatus.NOT_FOUND,
                    Message = "Sách không khả dụng hoặc không tồn tại."
                });
            }

            var reservation = new BookReservation
            {
                BookDetailId = request.BookDetailId,
                AccountId = request.AccountId,
                ReservationDate = DateTime.Now,
                Status = 1
            };

            _context.BookReservations.Add(reservation);
            await _context.SaveChangesAsync();

            return Ok(new OkResponse
            {
                Status = HttpStatus.OK.GetDescription(),
                StatusCode = HttpStatus.OK,
                Message = "Đặt chỗ sách thành công.",
                Data = reservation
            });
        }


        // Kiểm tra trạng thái đặt chỗ quá hạn
        [HttpPost("check-reservation-status")]
        public async Task<IActionResult> CheckReservationStatus()
        {
            try
            {
                var reservations = (from br in _context.BookReservations
                                    join bd in _context.BookDetails on br.BookDetailId equals bd.Id
                                    where br.Status == 1 && br.ReservationDate.AddDays(3) <= DateTime.Now
                                    select br).ToList();

                foreach (var reservation in reservations)
                {
                    var notification = new Notification
                    {
                        Title = "Thông báo hủy đặt chỗ",
                        Content = $"Đơn đặt chỗ sách của bạn đã quá hạn trong 3 ngày.",
                        Date = DateTime.Now,
                        Status = 1,
                        AccountId = reservation.AccountId
                    };
                    _context.Notifications.Add(notification);
                    reservation.Status = 0; // Hủy
                }

                await _context.SaveChangesAsync();
                return Ok(new OkResponse
                {
                    Status = HttpStatus.OK.GetDescription(),
                    StatusCode = HttpStatus.OK,
                    Message = "Kiểm tra trạng thái đặt chỗ thành công."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình kiểm tra trạng thái đặt chỗ.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Có lỗi xảy ra trong quá trình xử lý.");
            }
        }

        //Tạo danh sách yêu thích và phân trang (10/1 trang)
        [HttpGet("favorite-books/{borrowerId}")]
        public async Task<IActionResult> GetFavoriteBooks(int borrowerId, int pageNumber = 1, int pageSize = 10)
        {
            var totalCount = await _context.Favorites.CountAsync(f => f.BorrowerId == borrowerId);

            var favorites = await _context.Favorites
                .Where(f => f.BorrowerId == borrowerId)
                //.Include(f => f.Book)
                .OrderByDescending(f => f.AddedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new
                {
                  //  f.Book.Id,
                  //  f.Book.Title,
                    f.AddedDate,
                    f.Status
                })
                .ToListAsync();

            if (!favorites.Any())
            {
                return Ok(new ErrorResponse
                {
                    Status = HttpStatus.NOT_FOUND.GetDescription(),
                    StatusCode = HttpStatus.NOT_FOUND,
                    Message = "Không có sách yêu thích.",
                    Data = new { HasFavorites = false }
                });
            }

            return Ok(new OkResponse
            {
                Status = HttpStatus.OK.GetDescription(),
                StatusCode = HttpStatus.OK,
                Message = "Lấy danh sách yêu thích thành công.",
                Data = new
                {
                    TotalItems = totalCount,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    Favorites = favorites
                }
            });
        }

        //Thêm vào danh sách đọc
        [HttpPost("wishlist")]
        public async Task<IActionResult> AddToWishlist([FromBody] FavoriteDTO model)
        {
            if (!ModelState.IsValid)
            {
                return Ok(new ErrorResponse
                {
                    Status = HttpStatus.BAD_REQUEST.GetDescription(),
                    StatusCode = HttpStatus.BAD_REQUEST,
                    Message = "Dữ liệu không hợp lệ.",
                    Data = ModelState
                });
            }

            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.BorrowerId == model.BorrowerId && f.BookId == model.BookId);

            if (existingFavorite != null)
            {
                return Ok(new ErrorResponse
                {
                    Status = HttpStatus.CONFLICT.GetDescription(),
                    StatusCode = HttpStatus.CONFLICT,
                    Message = "Sách đã có trong danh sách muốn đọc.",
                    Data = new { BookId = model.BookId }
                });
            }

            var wishlist = new Favorite
            {
                BorrowerId = model.BorrowerId,
                BookId = model.BookId,
                AddedDate = DateTime.UtcNow,
                Status = FavoriteStatus.WantToRead
            };

            _context.Favorites.Add(wishlist);
            await _context.SaveChangesAsync();

            return Ok(new OkResponse
            {
                Status = HttpStatus.OK.GetDescription(),
                StatusCode = HttpStatus.OK,
                Message = "Sách đã được thêm vào danh sách muốn đọc.",
                Data = new { BookId = model.BookId }
            });
        }


        //Quản lý danh sách đọc
        [HttpGet("wishlist/{borrowerId}")]
        public async Task<IActionResult> GetWishlist(int borrowerId, int pageNumber = 1, int pageSize = 10)
        {
            var totalCount = await _context.Favorites
                .CountAsync(f => f.BorrowerId == borrowerId && f.Status == FavoriteStatus.WantToRead);

            var wishlist = await _context.Favorites
                .Where(f => f.BorrowerId == borrowerId && f.Status == FavoriteStatus.WantToRead)
                //.Include(f => f.Book)
                .OrderByDescending(f => f.AddedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new
                {
                   // f.Book.Id,
                   // f.Book.Title,
                    f.AddedDate
                })
                .ToListAsync();

            if (!wishlist.Any())
            {
                return Ok(new ErrorResponse
                {
                    Status = HttpStatus.NOT_FOUND.GetDescription(),
                    StatusCode = HttpStatus.NOT_FOUND,
                    Message = "Danh sách muốn đọc trống.",
                    Data = new { HasWishlist = false }
                });
            }

            return Ok(new OkResponse
            {
                Status = HttpStatus.OK.GetDescription(),
                StatusCode = HttpStatus.OK,
                Message = "Lấy danh sách muốn đọc thành công.",
                Data = new
                {
                    TotalItems = totalCount,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    Wishlist = wishlist
                }
            });
        }


        //Chia sẻ danh sách đọc


        //Nhập danh sách đọc
    }
}
