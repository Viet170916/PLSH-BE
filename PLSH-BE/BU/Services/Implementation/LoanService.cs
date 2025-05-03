#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BU.Services.Interface;
using Data.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Model.Entity.Borrow;
using Model.Entity.User;

namespace BU.Services.Implementation;

public class LoanService(AppDbContext context) : ILoanService
{
  public async Task<Loan?> GetLoanByIdAsync(int loanId)
  {
    return await context.Loans
                         .Include(l => l.BookBorrowings)
                         .FirstOrDefaultAsync(l => l.Id == loanId);
  }

  public async Task<Account?> GetUserByIdAsync(int userId)
  {
    return await context.Accounts.FindAsync(userId);
  }

  public async Task<List<Account>> GetLibrariansAsync()
  {
    return await context.Accounts
                         .Where(a => a.Role.Name == "librarian")
                         .ToListAsync();
  }

  public async Task UpdateLoanAsync(Loan loan)
  {
    context.Loans.Update(loan);
    await context.SaveChangesAsync();
  }
}
