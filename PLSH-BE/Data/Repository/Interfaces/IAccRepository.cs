using Data.Repository.Implementation;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Entity.User;

namespace Data.Repository.Interfaces
{
    public interface IAccRepository
    {
        Task<Account> GetUserByEmailAsync(string email);
        Task<Account> CreateUserAsync(Account account);
        Task<Account> GetUserByRefreshTokenAsync(string refreshToken);
        Task UpdateUserAsync(Account account);
    }
}
