using System.Threading.Tasks;
using Model.Entity;

namespace BU.Services.Interface;

public interface IAccountService
{
  Task<Account> GetOrCreateUserAsync(string email);
}