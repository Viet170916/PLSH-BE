#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using BU.Services.Interface;
using Data.DatabaseContext;
using Data.DTO;
using Data.UnitOfWork;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using Model.Entity.User;

namespace BU.Services.Implementation;

public class AccountService : IAccountService
{
    private readonly AppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public AccountService(AppDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Account?> GetOrCreateUserAsync(string email)
    {
        var user = await _unitOfWork.AccountRepository.FindQueryable(acc => acc.Email == email)
                               .Select(acc => new Account
                               {
                                   Email = acc.Email,
                                   Id = acc.Id,
                                   FullName = acc.FullName,
                                   IsVerified = acc.IsVerified,
                                   AvatarUrl = acc.AvatarUrl
                               })
                               .FirstOrDefaultAsync(); 

        return user;
    }

    //public async Task<AccountControllers?> GetUserByEmailAsync(string email)
    //{
    //    return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
    //}


    public async Task<Account?> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentNullException(nameof(email));
        }

        return await _context.Accounts
            .Where(a => a.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<Account?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.RefreshToken == refreshToken);
    }

    //Check AccountControllers qu� 90 ng�y th� ph?i ??i m?t kh?u
    public async Task<bool> IsPasswordExpiredAsync(int accountId)
    {
        var lastPasswordChange = await _context.PasswordAudits
            .Where(p => p.AccountId == accountId)
            .OrderByDescending(p => p.ChangedAt)
            .FirstOrDefaultAsync();

        return lastPasswordChange != null && (DateTime.UtcNow - lastPasswordChange.ChangedAt).TotalDays > 90;
    }

    public async Task UpdateUserAsync(Account user)
    {
        // Find the user in the database
        var existingUser = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == user.Id);

        if (existingUser == null)
        {
            throw new InvalidOperationException($"AccountControllers with ID {user.Id} does not exist.");
        }

        // Update the fields of the existing account
       // existingUser.FullName = user.FullName;
        existingUser.Email = user.Email;
        existingUser.Password = user.Password;
        //existingUser.AvataUrl = user.AvataUrl;
        existingUser.RefreshToken = user.RefreshToken;
        existingUser.RefreshTokenExpiry = user.RefreshTokenExpiry;
        existingUser.IsVerified = user.IsVerified;

        // Save the changes to the database
        await _context.SaveChangesAsync();
    }
    public async Task<Account> CreateUserAsync(Account user)
    {
        _context.Accounts.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

}
