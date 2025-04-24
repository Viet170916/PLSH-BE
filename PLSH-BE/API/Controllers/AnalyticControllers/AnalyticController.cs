using Data.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Model.Entity.book;
using Model.Entity.Borrow;

namespace API.Controllers.AnalyticControllers;

[ApiController]
[Route("api/v1/analytic")]
public partial class AnalyticController(AppDbContext _context) : Controller
{
    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var totalBooks = await _context.Books.CountAsync();
        var totalMembers = await _context.Borrowers.CountAsync();
        var totalBorrowedBooks = await _context.BookBorrowings
            .Where(bb => bb.BorrowingStatus != "returned")
            .CountAsync();
        var totalAccounts = await _context.Accounts.CountAsync();

        var stats = new
        {
            TotalBooks = totalBooks,
            TotalMembers = totalMembers,
            TotalBorrowedBooks = totalBorrowedBooks,
            TotalAccounts = totalAccounts
        };

        return Ok(stats);
    }

    [HttpGet("book-stats")]
    public async Task<IActionResult> GetBookStats()
    {
        var bookStats = await _context.BookInstances
            .Include(bi => bi.Book)
                .ThenInclude(b => b.Authors)
            .Include(bi => bi.Book)
                .ThenInclude(b => b.Category)
            .Include(bi => bi.Book)
                .ThenInclude(b => b.AudioResource)
            .Include(bi => bi.Book)
                .ThenInclude(b => b.EpubResource)
            .Include(bi => bi.RowShelf)
                .ThenInclude(rs => rs.Shelf)
            .Where(bi => bi.DeletedAt == null) // Only include non-deleted book instances
            .Select(bi => new
            {
                Id = bi.Id,
                Title = bi.Book.Title,
                Author = bi.Book.Authors.Any() ? string.Join(", ", bi.Book.Authors.Select(a => a.FullName)) : "Unknown",
                Category = bi.Book.Category != null ? bi.Book.Category.Name : "Uncategorized",
                Place = bi.RowShelf != null ? $"{bi.RowShelf.Shelf.Name} - Row {bi.RowShelf.Position} - Pos {bi.Position}" : "Not Assigned",
                Status = bi.IsInBorrowing ? "Borrowed" : "In-Shelf",
                Audio = bi.Book.AudioResourceId != null ? "Checked" : "Not Checked",
                Ebook = bi.Book.EpubResourceId != null ? "Checked" : "Not Checked"
            })
            .Take(100) // Limit to 100 records for performance
            .ToListAsync();

        return Ok(new
        {
            Message = "Book statistics retrieved successfully.",
            Success = true,
            Data = bookStats
        });
    }

    [HttpGet("borrower-stats")]
    public async Task<IActionResult> GetBorrowerStats()
    {
        var currentDate = DateTime.UtcNow;
        var currentYear = currentDate.Year;

        var borrowerStats = await _context.Loans
            .Include(l => l.Borrower)
                .ThenInclude(b => b.Role)
            .Include(l => l.BookBorrowings)
                .ThenInclude(bb => bb.BookInstance)
                .ThenInclude(bi => bi.Book)
            .Where(l => !l.IsCart && l.AprovalStatus != "cancel" && l.AprovalStatus != "rejected") // Exclude carts and cancelled/rejected loans
            .SelectMany(l => l.BookBorrowings, (l, bb) => new { Loan = l, BookBorrowing = bb })
            .Select(x => new
            {
                Id = x.Loan.Borrower.Id,
                Name = x.Loan.Borrower.FullName,
                Age = x.Loan.Borrower.Birthdate.HasValue
                    ? currentYear - x.Loan.Borrower.Birthdate.Value.Year -
                      (x.Loan.Borrower.Birthdate.Value.Date > currentDate.AddYears(-(currentYear - x.Loan.Borrower.Birthdate.Value.Year)) ? 1 : 0)
                    : 0,
                Phone = x.Loan.Borrower.PhoneNumber,
                Email = x.Loan.Borrower.Email,
                Role = x.Loan.Borrower.Role != null ? $"{x.Loan.Borrower.Role.Name} ({x.Loan.Borrower.Role.Id})" : "Unknown",
                IdMember = x.Loan.Borrower.CardMemberNumber ?? "N/A",
                BookBorrow = x.BookBorrowing.BookInstance.Book.Title,
                Status = x.BookBorrowing.BorrowingStatus == "on-loan"
                    ? (x.BookBorrowing.ReturnDates.Any() && x.BookBorrowing.ReturnDates.Last() < currentDate
                        ? "Quá H?n"
                        : (x.BookBorrowing.ExtendDates.Any() ? "Gia h?n" : "?ang m??n"))
                    : (x.BookBorrowing.BorrowingStatus == "returned" ? "?ã tr?" : "?ang ch? xét duy?t"),
                IsFine = x.BookBorrowing.ReturnDates.Any() && x.BookBorrowing.ReturnDates.Last() < currentDate && x.BookBorrowing.BorrowingStatus != "returned"
                    ? "Checked"
                    : "Not Checked",
                FineAmount = x.BookBorrowing.ReturnDates.Any() && x.BookBorrowing.ReturnDates.Last() < currentDate && x.BookBorrowing.BorrowingStatus != "returned"
                    ? "5000 vnd" // Hard-coded fine amount; adjust based on your fine calculation logic
                    : "0 vnd"
            })
            .Take(100) // Limit to 100 records for performance
            .ToListAsync();

        return Ok(new
        {
            Message = "Borrower statistics retrieved successfully.",
            Success = true,
            Data = borrowerStats
        });
    }
}