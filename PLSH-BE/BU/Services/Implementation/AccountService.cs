#nullable enable
using System.Linq;
using System.Threading.Tasks;
using BU.Services.Interface;
using Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Model.Entity;

namespace BU.Services.Implementation;

public class AccountService(IUnitOfWork unitOfWork) : IAccountService
{
  public async Task<Account?> GetOrCreateUserAsync(string email)
  {
    var user = await unitOfWork.AccountRepository.FindQueryable(acc => acc.Email == email)
                               .Select(acc => new Account()
                               {
                                 Email = acc.Email, 
                                 Id = acc.Id, 
                                 FullName = acc.FullName,
                                 isVerified = acc.isVerified,
                                 AvataUrl = acc.AvataUrl,
                                 // PhoneNumber = acc.PhoneNumber,
                               })
                               .FirstOrDefaultAsync();
    // user = new Account { Email = email, CreatedAt = DateTime.UtcNow, };
    // await unitOfWork.AccountRepository.Add(user);
    // unitOfWork.SaveChanges();
    return user;
  }
}