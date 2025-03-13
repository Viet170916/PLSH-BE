using System.Threading.Tasks;
using Model.Entity;

namespace BU.Services.Interface;

public interface IAccountService
{
  Task<Account> GetOrCreateUserAsync(string email);
    Task<Account?> GetUserByEmailAsync(string email);
    Task<Account?> GetUserByRefreshTokenAsync(string refreshToken);
    Task UpdateUserAsync(Account user);
    Task<Account> CreateUserAsync(Account user);
    Task<bool> IsPasswordExpiredAsync(int accountId);
}