using Data.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BU.Services.Interface;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
}