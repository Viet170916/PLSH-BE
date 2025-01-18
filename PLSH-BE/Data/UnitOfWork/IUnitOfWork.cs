using Data.Repository.Interfaces;

namespace Data.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
  IAccountRepository AccountRepository { get; }

  // ICYEngagementHoursRepository CYEngagementHoursRepository { get; }
  int Complete();
  void BeginTransaction();
  int SaveChanges();
  void Commit();
  void Rollback();
}