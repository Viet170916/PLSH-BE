using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Common.Enums;
using Model.Entity.book.Dto;

namespace Model.Entity.book;

public class Book
{
  public int? Id { get; set; }

  [MaxLength(255)]
  public string? Title { get; set; }

  public BookType? BookType { get; set; }

  [Column(TypeName = "text")]
  public string? Description { get; set; }

  [ForeignKey("BookAuthor")]
  public long? BookAuthorId { get; set; }

  public AvailabilityKind? Kind { get; set; }
  public int? CoverImageResourceId { get; set; } //Anh bia
  public int? PreviewPdfResourceId { get; set; }
  public int? AudioResourceId { get; set; }
  public int? EpubResourceId { get; set; }

  [MaxLength(30)]
  public string? Version { get; set; }

  [MaxLength(255)]
  public string? Publisher { get; set; }

  public string? PublishDate { get; set; }

  [MaxLength(50)]
  public string? Language { get; set; }

  public int? PageCount { get; set; }

  // public BookType? BookType { get; set; }
  [ForeignKey("Category")]
  public int? CategoryId { get; set; }

  [MaxLength(13)]
  public string? IsbNumber13 { get; set; }

  [MaxLength(100)]
  public string? OtherIdentifier { get; set; }

  [MaxLength(10)]
  public string? IsbNumber10 { get; set; }

  public float? Rating { get; set; }
  public int? TotalCopies { get; set; }
  public int? AvailableCopies { get; set; }
  public double? Price { get; set; }

  [MaxLength(255)]
  public string? Thumbnail { get; set; }

  public double? Fine { get; set; }
  public DateTime? CreateDate { get; set; }
  public DateTime? UpdateDate { get; set; }
  public DateTime? DeletedAt { get; set; }
  public bool? IsChecked { get; set; }
  public int? Height { get; set; }
  public int? Width { get; set; }
  public int? Thickness { get; set; }
  public int? Weight { get; set; }
  public int? BookReviewId { get; set; }
  public int? Quantity { get; set; }
  public Category? Category { get; set; }

  [NotMapped]
  public List<AvailabilityDto>? Availabilities { get; set; } = [];

  [NotMapped]
  public BookAvailabilityDto? BookStatus { get; set; }

  [ForeignKey("CoverImageResourceId")]
  public Resource? CoverImageResource { get; set; }

  [ForeignKey("EpubResourceId")]
  public Resource? EpubResource { get; set; }

  [ForeignKey("PreviewPdfResourceId")]
  public Resource? PreviewPdfResource { get; set; }

  [ForeignKey("AudioResourceId")]
  public Resource? AudioResource { get; set; }

  [JsonIgnore]
  public ICollection<Author>? Authors { get; set; }

  [JsonIgnore]
  public ICollection<BookInstance>? BookInstances { get; set; } = new List<BookInstance>();
}