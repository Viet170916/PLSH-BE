// API/DTO/BookDetail/BookDetailCreateDTO.cs
namespace API.DTO.Book;

public class BookDetailCreateDto
{
  [Required(ErrorMessage = "BookId is required.")]
  [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive number.")]
  public int BookId { get; set; }

  [Required(ErrorMessage = "Status is required.")]
  [Range(0, 1, ErrorMessage = "Status must be either 0 (unavailable) or 1 (available).")]
  public int Status { get; set; } = 1;

  [Url(ErrorMessage = "BookConditionUrl must be a valid URL.")]
  [StringLength(500, ErrorMessage = "BookConditionUrl cannot exceed 500 characters.")]
  public string? BookConditionUrl { get; set; }

  [StringLength(1000, ErrorMessage = "BookConditionDescription cannot exceed 1000 characters.")]
  public string? BookConditionDescription { get; set; }

  [StringLength(500, ErrorMessage = "StatusDescription cannot exceed 500 characters.")]
  public string? StatusDescription { get; set; }
}

public class BookDetailUpdateDTO
{
  [Required(ErrorMessage = "Status is required.")]
  [Range(0, 1, ErrorMessage = "Status must be either 0 (unavailable) or 1 (available).")]
  public int Status { get; set; }

  [Url(ErrorMessage = "BookConditionUrl must be a valid URL.")]
  [StringLength(500, ErrorMessage = "BookConditionUrl cannot exceed 500 characters.")]
  public string? BookConditionUrl { get; set; }

  [StringLength(1000, ErrorMessage = "BookConditionDescription cannot exceed 1000 characters.")]
  public string? BookConditionDescription { get; set; }

  [StringLength(500, ErrorMessage = "StatusDescription cannot exceed 500 characters.")]
  public string? StatusDescription { get; set; }
}

public class BookDetailStatusUpdateDTO
{
  [Required(ErrorMessage = "Status is required.")]
  [Range(0, 1, ErrorMessage = "Status must be either 0 (unavailable) or 1 (available).")]
  public int Status { get; set; }

  [StringLength(500, ErrorMessage = "StatusDescription cannot exceed 500 characters.")]
  public string? StatusDescription { get; set; }
}