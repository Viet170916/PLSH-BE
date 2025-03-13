namespace API.DTO.BookDetail
{
    public class BookDetailDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive number.")]
        public int BookId { get; set; }

        [Range(0, 1, ErrorMessage = "Status must be 0 (unavailable) or 1 (available).")]
        public int Status { get; set; } = 1; // 1 is available, 0 is unavailable

        [Url(ErrorMessage = "BookConditionUrl must be a valid URL.")]
        public string? BookConditionUrl { get; set; }

        [StringLength(500, ErrorMessage = "BookConditionDescription cannot be longer than 500 characters.")]
        public string? BookConditionDescription { get; set; }

        [StringLength(255, ErrorMessage = "StatusDescription cannot be longer than 255 characters.")]
        public string? StatusDescription { get; set; }

        [Required(ErrorMessage = "CreatedAt is required.")]
        public DateTime CreatedAt { get; set; }

        [Required(ErrorMessage = "UpdatedAt is required.")]
        public DateTime UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
