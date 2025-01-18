using System.Diagnostics.CodeAnalysis;
using Data.DatabaseContext;
using Data.Repository.Implementation;
using Data.Repository.Interfaces;
using Data.UnitOfWork;

namespace Data.UnitOfWork
{
  [ExcludeFromCodeCoverage]
  public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
  {
    public IAccountRepository AccountRepository { get; } = new AccountRepository(context);
    public int Complete() { return context.SaveChanges(); }
    public void Dispose() { context.Dispose(); }
    public void BeginTransaction() => context.Database.BeginTransaction();
    public void Commit() => context.Database.CommitTransaction();
    public void Rollback() => context.Database.RollbackTransaction();
    public int SaveChanges() => context.SaveChanges();
  }
}