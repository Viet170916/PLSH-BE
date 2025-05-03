#nullable enable
using System.Collections.Generic;
using Common.Enums;
using Microsoft.AspNetCore.Http;
using Model.Entity;
using Model.Entity.book;

namespace BU.Models.DTO.Book;

public class BookNewDto
{
  public bool HasEbook { get; set; } = false;
  public bool? IsFavorite { get; set; } = false;
  public int? Id { get; set; }
  public int? AvailableBookCount { get; set; }
  public required string Title { get; set; }
  public string? Description { get; set; }
  public string? Thumbnail { get; set; }
  public int? CategoryId { get; set; }
  public int? Width { get; set; }
  public int? Weight { get; set; }
  public int? Thickness { get; set; }
  public int? Height { get; set; }
  public AvailabilityKind? Kind { get; set; } // 1: PhysicalBook, 2: Ebook, 3: Audiobook
  public string? Version { get; set; }
  public string? Publisher { get; set; }
  public string? PublishDate { get; set; }
  public string? Language { get; set; }
  public int? PageCount { get; set; }
  public string? IsbnNumber13 { get; set; }
  public string? IsbnNumber10 { get; set; }
  public string? OtherIdentifier { get; set; }
  public double? Price { get; set; }
  public int? TotalCopies { get; set; }
  public int? AvailableCopies { get; set; }
  public IList<AuthorDto>? Authors { get; set; }
  public int? EpubResourceId { get; set; }
  public int? AudioResourceId { get; set; }
  public IList<BookInstance> BookInstances { get; set; } = new List<BookInstance>();
  public int Quantity { get; set; } = 0;
  public CategoryDto? Category { get; set; }

  // public CategoryDto? NewCategory { get; set; }
  public IFormFile? CoverImage { get; set; }
  public IFormFile? ContentPdf { get; set; }
  public IFormFile? AudioFile { get; set; }
  public IFormFile? Epub { get; set; }
  public Resource? AudioResource { get; set; }
  public Resource? EpubResource { get; set; }
  public Resource? CoverImageResource { get; set; }
  public Resource? PreviewPdfResource { get; set; }
  public float? Rating { get; set; }
}

public class BookNewDtoAiRes
{
  public string? Title { get; set; }
  public string? Description { get; set; }
  public string? Thumbnail { get; set; }
  public int? Width { get; set; }
  public int? Weight { get; set; }
  public int? Thickness { get; set; }
  public int? Height { get; set; }
  public string? Version { get; set; }
  public string? Publisher { get; set; }
  public string? PublishDate { get; set; }
  public string? Language { get; set; }
  public int? PageCount { get; set; }
  public string? IsbnNumber13 { get; set; }
  public string? IsbnNumber10 { get; set; }
  public string? OtherIdentifier { get; set; }
  public double? Price { get; set; }
  public IList<AuthorAiDto>? Authors { get; set; }
  public CategoryAiDto? Category { get; set; }

  public CategoryAiDto? NewCategory { get; set; }
}

public class BookMinimalDto
{
  public int? Id { get; set; }
  public bool? IsFavorite { get; set; } = false;
  public required string Title { get; set; }
  public string? Thumbnail { get; set; }
  public int? CategoryId { get; set; }
  public required string Version { get; set; }
  public IList<AuthorDto>? Authors { get; set; }
  public IFormFile? CoverImage { get; set; }
  public float? Rating { get; set; }
}

public class AuthorDto
{
  public int? Id { get; set; }
  public required string FullName { get; set; }
  public string? AvatarUrl { get; set; }
  public string? Description { get; set; }
  public string? SummaryDescription { get; set; }
  public string? BirthYear { get; set; }
  public string? DeathYear { get; set; }
  public IFormFile? AuthorImageResource { get; set; }
  public Resource? Resource { get; set; }
}
public class AuthorAiDto
{
  public int? Id { get; set; }
  public string? FullName { get; set; }
  public string? Description { get; set; }
  public string? BirthYear { get; set; }
  public string? DeathYear { get; set; }
}

public class CategoryDto
{
  public int? Id { get; set; }
  public required string Name { get; set; }
  public string? Description { get; set; }
}
public class CategoryAiDto
{
  public string? Name { get; set; }
  public bool? Chosen { get; set; } = false;
  public string? Description { get; set; }
}
