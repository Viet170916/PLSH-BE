using Data.DatabaseContext;
using Model.Entity;
using Microsoft.EntityFrameworkCore;
using Model.Entity.User;

namespace Data.Repository.Implementation
{
    public class AccRepository
    {
        private readonly AppDbContext _context;

        public AccRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Account> GetUserByEmailAsync(string email)
        {
            return await _context.Accounts.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Account> CreateUserAsync(Account account)
        {
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<Account> GetUserByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Accounts.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        public async Task UpdateUserAsync(Account account)
        {
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }
    }
}
