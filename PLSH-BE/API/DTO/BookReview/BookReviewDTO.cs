using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DTO.BookReview
{
    public class BookReviewDto
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Book")]
        public int BookId { get; set; }

        [Required]
        [ForeignKey("AccountControllers")]
        public int AccountId { get; set; }  // Người dùng đã đánh giá

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }  // Điểm đánh giá từ 1-5 sao

        [StringLength(2000, ErrorMessage = "Comment cannot be longer than 2000 characters.")]
        public string? Comment { get; set; }  // Bình luận

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
