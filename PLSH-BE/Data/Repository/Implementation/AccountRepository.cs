using Data.DatabaseContext;
using Data.Repository.Interfaces;
using Model.Entity;

namespace Data.Repository.Implementation;

public class AccountRepository(AppDbContext context) : GenericRepository<Account>(context), IAccountRepository;