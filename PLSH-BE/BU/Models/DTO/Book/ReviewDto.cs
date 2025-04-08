using System;
using Microsoft.AspNetCore.Http;

namespace BU.Models.DTO.Book;

public class ReviewDto
{
  public bool IsYouAreSender { get; set; } = false;
  public int? Id { get; set; }
  public int? AccountSenderId { get; set; }
  public string? AccountSenderName { get; set; }
  public string? AccountSenderAvatar { get; set; }
  public int? BookId { get; set; }
  public string? BookTitle { get; set; }
  public int? ResourceId { get; set; }
  public string? ResourceUrl { get; set; }
  public IFormFile? ResourceFile { get; set; }
  public float? Rating { get; set; }
  public string? Content { get; set; }
  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedDate { get; set; }
}

public class MessageDto
{
  public bool IsYouAreSender { get; set; } = false;
  public int? Id { get; set; }
  public int? SenderId { get; set; }
  public string? SenderName { get; set; }
  public string? SenderAvatar { get; set; }
  public int? RepliedId { get; set; }
  public string? RepliedAvatar { get; set; }
  public string? RepliedPersonName { get; set; }
  public int ReviewId { get; set; }
  public string? ReviewTitle { get; set; }
  public string? Content { get; set; }
  public int? ResourceId { get; set; }
  public string? ResourceUrl { get; set; }
  public IFormFile? ResourceFile { get; set; }
  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedDate { get; set; }
}
