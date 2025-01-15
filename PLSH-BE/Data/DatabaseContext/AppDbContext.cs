using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Model.Entity;

namespace Data.DatabaseContext
{
  [ExcludeFromCodeCoverage]
  public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
  {
    protected override void OnModelCreating(ModelBuilder modelBuilder) { base.OnModelCreating(modelBuilder); }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<BookBorrowing> BookBorrowings { get; set; }
    public DbSet<BookDetail> BookDetails { get; set; }
    public DbSet<Borrower> Borrowers { get; set; }
    public DbSet<Fine> Fines { get; set; }
    public DbSet<Librarian> Librarians { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Page> Pages { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
  }
}