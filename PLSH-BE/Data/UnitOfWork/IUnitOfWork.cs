namespace Data.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
  // ICYEngagementHoursRepository CYEngagementHoursRepository { get; }
  int Complete();
  void BeginTransaction();
  int SaveChanges();
  void Commit();
  void Rollback();
}