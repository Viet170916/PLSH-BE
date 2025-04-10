using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Entity.Borrow;
using Model.Entity.User;

namespace BU.Services.Interface;

public interface ILoanService
{
  Task<Loan> GetLoanByIdAsync(int loanId);
  Task<Account?> GetUserByIdAsync(int userId);
  Task<List<Account>> GetLibrariansAsync();
  Task UpdateLoanAsync(Loan loan);
}
