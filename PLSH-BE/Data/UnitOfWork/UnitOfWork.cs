using System.Diagnostics.CodeAnalysis;
using Data.DatabaseContext;
using Data.UnitOfWork;

namespace Data.UnitOfWork
{
    [ExcludeFromCodeCoverage]
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
     
        // public ICYEngagementHoursRepository CYEngagementHoursRepository { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            
            // CYEngagementHoursRepository = new CYEngagementHoursRepository(_context);
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public virtual void BeginTransaction() => _context.Database.BeginTransaction();

        public virtual void Commit() => _context.Database.CommitTransaction();

        public virtual void Rollback() => _context.Database.RollbackTransaction();

        public virtual int SaveChanges() => _context.SaveChanges();
    }
}
