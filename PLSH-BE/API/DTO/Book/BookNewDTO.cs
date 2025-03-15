#nullable enable
using Common.Enums;
using Microsoft.AspNetCore.Http;
using Model.Entity;

namespace API.DTO.Book
{
  public class BookNewDto
  {
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int AuthorId { get; set; }
    public int CategoryId { get; set; }
    public AvailabilityKind Kind { get; set; } // 1: PhysicalBook, 2: Ebook, 3: Audiobook
    public string Version { get; set; }
    public string Publisher { get; set; }
    public DateTime PublishDate { get; set; }
    public string Language { get; set; }
    public int PageCount { get; set; }
    public string IsbnNumber { get; set; }
    public double Price { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }

    // Thông tin tác giả
    public AuthorDto Author { get; set; }

    // Thông tin thể loại sách
    public CategoryDto Category { get; set; }

    // File upload nếu là Ebook hoặc Audiobook
    public IFormFile? File { get; set; }
  }

  public class AuthorDto
  {
    public int Id { get; set; }
    public string FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Description { get; set; }
    public string? SummaryDescription { get; set; }
    public string? BirthYear { get; set; }
    public string? DeathYear { get; set; }
    public IFormFile? AuthorImageResource { get; set; }
    public Resource? Resource { get; set; }
  }

  public class CategoryDto
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
  }
}