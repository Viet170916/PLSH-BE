using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Common;
using AutoMapper;
using BU.Models.DTO;
using BU.Models.DTO.Notification;
using BU.Services.Interface;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Model.Entity.Borrow;
using Model.Entity.User;
using System.Security.Claims;
using BU.Hubs;

namespace API.Controllers.FineControllers;

[Authorize]
[Route("api/v1/fine")]
[ApiController]
public class FineController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly GoogleCloudStorageHelper _googleCloudStorageHelper;
    private readonly IHubContext<ReviewHub> _hubContext;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public FineController(
        AppDbContext context,
        IMapper mapper,
        GoogleCloudStorageHelper googleCloudStorageHelper,
        IHubContext<ReviewHub> hubContext,
        INotificationService notificationService,
        IEmailService emailService)
    {
        _context = context;
        _mapper = mapper;
        _googleCloudStorageHelper = googleCloudStorageHelper;
        _hubContext = hubContext;
        _notificationService = notificationService;
        _emailService = emailService;
    }

    public class UpdateFineByDateDto
    {
        public double FineByDate { get; set; }
    }

    public class UpdateFineStatusDto
    {
        public int Status { get; set; }
    }

    public class ReturnInfoRequestDto
    {
        public string? NoteAfterBorrow { get; set; }
        public List<IFormFile>? Images { get; set; }
    }

    public class FineDto
    {
        public int Id { get; set; }
        public DateTime FineDate { get; set; }
        public bool IsFined { get; set; }
        public int? FineType { get; set; }
        public double? Amount { get; set; }
        public double FineByDate { get; set; }
        public string? Note { get; set; }
        public int? BookBorrowingId { get; set; }
        public int BorrowerId { get; set; }
        public int Status { get; set; }
        public int? TransactionId { get; set; }
    }

    [Authorize(Policy = "LibrarianPolicy")]
    [HttpPut("update-fine-by-date")]
    public async Task<IActionResult> UpdateFineByDate([FromBody] UpdateFineByDateDto request)
    {
        if (request.FineByDate <= 0)
        {
            return BadRequest(new BaseResponse<string>
            {
                Message = "Phí phạt mỗi ngày phải lớn hơn 0.",
                Status = "error"
            });
        }

        var fines = await _context.Fines
            .Include(f => f.BookBorrowing)
            .Where(f => f.FineType == 0 && f.Status != "completed")
            .ToListAsync();

        foreach (var fine in fines)
        {
            if (fine.BookBorrowing?.ActualReturnDate == null || fine.BookBorrowing.ReturnDates == null || !fine.BookBorrowing.ReturnDates.Any())
                continue;

            var lastReturnDate = fine.BookBorrowing.ReturnDates.Last();
            var daysLate = (fine.BookBorrowing.ActualReturnDate.Value - lastReturnDate).Days;
            if (daysLate > 0)
            {
                fine.FineByDate = request.FineByDate;
                fine.Amount = request.FineByDate * daysLate;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new BaseResponse<string>
        {
            Message = "Cập nhật phí phạt mỗi ngày và tính lại Amount thành công.",
            Status = "success"
        });
    }

    [Authorize(Policy = "LibrarianPolicy")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFine(int id)
    {
        var fine = await _context.Fines.FindAsync(id);
        if (fine == null)
        {
            return NotFound(new BaseResponse<string>
            {
                Message = "Không tìm thấy phí phạt.",
                Status = "error"
            });
        }

        _context.Fines.Remove(fine);
        await _context.SaveChangesAsync();

        return Ok(new BaseResponse<string>
        {
            Message = "Xóa phí phạt thành công.",
            Status = "success"
        });
    }

    [Authorize(Policy = "BorrowerPolicy")]
    [HttpGet("personal")]
    public async Task<IActionResult> GetPersonalFines(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new BaseResponse<string>
            {
                Status = "fail",
                Message = "Không xác thực: Token không hợp lệ."
            });
        }

        if (page < 1) page = 1;
        if (limit < 1) limit = 10;

        var query = _context.Fines
            .Include(f => f.BookBorrowing)
            .ThenInclude(bb => bb.BookInstance)
            .ThenInclude(bi => bi.Book)
            .Include(f => f.Borrower)
            .Where(f => f.BorrowerId == userId);

        var totalRecords = await query.CountAsync();
        var fines = await query
            .OrderByDescending(f => f.FineDate)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var fineDtos = _mapper.Map<List<FineDto>>(fines);

        return Ok(new BaseResponse<List<FineDto>>
        {
            Message = "Lấy danh sách phí phạt cá nhân thành công.",
            Status = "success",
            Data = fineDtos,
            Count = totalRecords,
            Page = page,
            Limit = limit,
            CurrentPage = page,
            PageCount = (int)Math.Ceiling((double)totalRecords / limit)
        });
    }
}
