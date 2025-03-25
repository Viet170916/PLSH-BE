using Data.DatabaseContext;
using Data.DTO;
using Data.Repository.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using Model.Entity.User;

namespace Data.Repository.Implementation;

public class AccountRepository: GenericRepository<Account>, IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<AccountDTO?> GetUserByEmailAsync(string email)
    {
        return await (from acc in _context.Accounts
                      join pro in _context.Profiles on acc.Id equals pro.AccountId into profileGroup
                      from pro in profileGroup.DefaultIfEmpty()
                      join role in _context.Roles on acc.RoleId equals role.Id into roleGroup
                      from role in roleGroup.DefaultIfEmpty()
                      where acc.Email == email
                      select new AccountDTO
                      {
                          Id = acc.Id,
                          Email = acc.Email,
                          isVerified = acc.IsVerified,
                          RoleId = acc.RoleId,
                          FullName = pro != null ? pro.FullName : null,
                          Address = pro != null ? pro.Address : null,
                          //RoleName = role != null ? role.Name : null
                      }).FirstOrDefaultAsync();
    }

}



