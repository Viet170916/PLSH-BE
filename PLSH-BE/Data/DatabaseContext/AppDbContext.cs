using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Data.DatabaseContext
{
    [ExcludeFromCodeCoverage]
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        // public DbSet<Roles> Roles { get; set; }

    }
}
