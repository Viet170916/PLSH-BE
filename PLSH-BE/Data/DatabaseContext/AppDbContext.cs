using System.Diagnostics.CodeAnalysis;
using Data.ExceptionTypes;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Entity;
using Model.Entity.book;
using Model.Entity.LibraryRoom;
using Model.Entity.User;

namespace Data.DatabaseContext;

[ExcludeFromCodeCoverage]
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Account> Accounts { get; set; }
  public DbSet<Book> Books { get; set; }
  public DbSet<BookBorrowing> BookBorrowings { get; set; }
  public DbSet<BookDetail> BookDetails { get; set; }
  public DbSet<BookReview> BookReviews { get; set; }
  public DbSet<HistoryReview> HistoryReviews { get; set; }
  public DbSet<Favorite> Favorites { get; set; }
  public DbSet<Borrower> Borrowers { get; set; }
  public DbSet<Fine> Fines { get; set; }
  public DbSet<Librarian> Librarians { get; set; }
  public DbSet<Loan> Loans { get; set; }
  public DbSet<Notification> Notifications { get; set; }
  public DbSet<Page> Pages { get; set; }
  public DbSet<PaymentMethod> PaymentMethods { get; set; }
  public DbSet<Role> Roles { get; set; }
  public DbSet<Transaction> Transactions { get; set; }
  public DbSet<PasswordAudit> PasswordAudits { get; set; }
  public DbSet<Category> Categories { get; set; }
  public DbSet<BookLocation> BookLocations { get; set; }
  public DbSet<Profile> Profiles { get; set; }
  public DbSet<Album> Albums { get; set; }
  public DbSet<AudioBook> AudioBooks { get; set; }
  public DbSet<EBook> EBooks { get; set; }
  public DbSet<Lecture> Lectures { get; set; }
  public DbSet<LifeSkill> LifeSkills { get; set; }
  public DbSet<Magazine> Magazines { get; set; }
  public DbSet<PhysicalBook> PhysicalBooks { get; set; }
  public DbSet<Video> Videos { get; set; }

  //public DbSet<Availability> Availabilities { get; set; } 
  public DbSet<Resource> Resources { get; set; }
  public DbSet<Author> Authors { get; set; }
  public DbSet<ShortBookInfo> ShortBookInfos { get; set; }
  public DbSet<BookReservation> BookReservations { get; set; }
  public DbSet<LibraryRoom> LibraryRooms { get; set; }
  public DbSet<Shelf> Shelves { get; set; }
  public DbSet<RowShelf> RowShelves { get; set; }
  public DbSet<BookInstance> BookInstances { get; set; }

  // public IQueryable<Author> FindSimilarAuthors(IQueryable<Author> , string fullName)
  // {
  //   var possibleMatches = Authors
  //     .FromSqlRaw(@"SELECT * FROM Authors WHERE MATCH(FullName) AGAINST ({0} IN NATURAL LANGUAGE MODE)",
  //       fullName)
  //   return possibleMatches
  //          .Where(a => StringExtensions.LevenshteinDistance(a.FullName, fullName) <= 3)
  //          .ToList();
  // }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    modelBuilder.Entity<Book>()
                .HasMany(b => b.Authors)
                .WithMany(a => a.Books)
                .UsingEntity<Dictionary<string, object>>("AuthorBook",
                  j => j.HasOne<Author>().WithMany().HasForeignKey("AuthorId"),
                  j => j.HasOne<Book>().WithMany().HasForeignKey("BookId"));
    modelBuilder.Entity<Author>()
                .HasIndex(a => a.FullName)
                .HasDatabaseName("idx_author_fullname")
                .IsUnique(false)
                .HasAnnotation("MySql:FullTextIndex", true);

    // Profile - AccountControllers (1 - 1)
    //modelBuilder.Entity<Profile>()
    //    .HasOne(p => p.AccountControllers)
    //    .WithOne(a => a.Profile)
    //    .HasForeignKey<Profile>(p => p.AccountId)
    //    .OnDelete(DeleteBehavior.Cascade);

    //// PasswordAudit - AccountControllers (1 - N)
    //modelBuilder.Entity<PasswordAudit>()
    //    .HasOne(p => p.AccountControllers)
    //    .WithMany(a => a.PasswordAudits)
    //    .HasForeignKey(p => p.AccountId)
    //    .OnDelete(DeleteBehavior.Cascade);

    //// Book - Category (1 - N)
    //modelBuilder.Entity<Book>()
    //    .HasOne(b => b.Category)
    //    .WithMany(c => c.Books)
    //    .HasForeignKey(b => b.CategoryId)
    //    .OnDelete(DeleteBehavior.Restrict);

    //// Borrower - Favorite - Book
    //modelBuilder.Entity<Favorite>()
    //    .HasOne(f => f.Borrower)
    //    .WithMany(b => b.Favorites)
    //    .HasForeignKey(f => f.BorrowerId)
    //    .OnDelete(DeleteBehavior.Cascade);

    //modelBuilder.Entity<Favorite>()
    //    .HasOne(f => f.Book)
    //    .WithMany(b => b.Favorites)
    //    .HasForeignKey(f => f.BookId)
    //    .OnDelete(DeleteBehavior.Cascade);

    //// Librarian - Loan (1 - N)
    //modelBuilder.Entity<Loan>()
    //    .HasOne(l => l.Librarian)
    //    .WithMany(lb => lb.Loans)
    //    .HasForeignKey(l => l.LibrarianId)
    //    .OnDelete(DeleteBehavior.Restrict);

    //// Borrower - Loan (1 - N)
    //modelBuilder.Entity<Loan>()
    //    .HasOne(l => l.Borrower)
    //    .WithMany(b => b.Loans)
    //    .HasForeignKey(l => l.BorrowerId)
    //    .OnDelete(DeleteBehavior.Cascade);

    //// Loan - BookBorrowing (1 - N)
    //modelBuilder.Entity<BookBorrowing>()
    //    .HasOne(bb => bb.Loan)
    //    .WithMany(l => l.BookBorrowings)
    //    .HasForeignKey(bb => bb.LoanId)
    //    .OnDelete(DeleteBehavior.Cascade);

    //// BookBorrowing - BookDetail (N - 1)
    //modelBuilder.Entity<BookBorrowing>()
    //    .HasOne(bb => bb.BookDetail)
    //    .WithMany()
    //    .HasForeignKey(bb => bb.BookDetailId)
    //    .OnDelete(DeleteBehavior.Restrict);

    //// AccountControllers - Role (N - 1)
    //modelBuilder.Entity<AccountControllers>()
    //    .HasOne(a => a.Role)
    //    .WithMany(r => r.Accounts)
    //    .HasForeignKey(a => a.RoleId)
    //    .OnDelete(DeleteBehavior.Restrict);

    //// BookDetail - Book (N - 1)
    //modelBuilder.Entity<BookDetail>()
    //    .HasOne(bd => bd.Book)
    //    .WithMany(b => b.BookDetails)
    //    .HasForeignKey(bd => bd.BookId)
    //    .OnDelete(DeleteBehavior.Cascade);

    //// Notification - AccountControllers (N - 1)
    //modelBuilder.Entity<Notification>()
    //    .HasOne(n => n.AccountControllers)
    //    .WithMany(a => a.Notifications)
    //    .HasForeignKey(n => n.AccountId)
    //    .OnDelete(DeleteBehavior.Cascade);

    //// Bookshelves & Book Locations
    //modelBuilder.Entity<Book>()
    //    .HasMany(b => b.BookLocations)
    //    .WithOne(bl => bl.Book);

    //modelBuilder.Entity<Shelf>()
    //    .HasMany(s => s.Bookshelves)
    //    .WithOne(bs => bs.Shelf);

    //modelBuilder.Entity<Bookshelf>()
    //    .HasMany(bs => bs.BookLocations)
    //    .WithOne(bl => bl.Bookshelf);

    //modelBuilder.Entity<Borrower>()
    //     .HasOne(b => b.AccountControllers)
    //     .WithOne(a => a.Borrower)
    //     .HasForeignKey<Borrower>(b => b.AccountId)
    //     .OnDelete(DeleteBehavior.Cascade);
  }
}