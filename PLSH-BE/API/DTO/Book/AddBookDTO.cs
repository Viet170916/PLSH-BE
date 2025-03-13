namespace API.DTO.Book
{
    public class AddBookDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters.")]
        public string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Author is required.")]
        [StringLength(200, ErrorMessage = "Author name cannot be longer than 200 characters.")]
        public string Author { get; set; }

        [StringLength(200, ErrorMessage = "Publisher name cannot be longer than 200 characters.")]
        public string? Publisher { get; set; }

        public DateTime? PublishDate { get; set; }

        [StringLength(50, ErrorMessage = "Language cannot be longer than 50 characters.")]
        public string? Language { get; set; }

        [StringLength(100, ErrorMessage = "Position cannot be longer than 100 characters.")]
        public string? Position { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page count must be a positive number.")]
        public int PageCount { get; set; }

        [StringLength(100, ErrorMessage = "Category name cannot be longer than 100 characters.")]
        public string? Category { get; set; }

        [Required(ErrorMessage = "ISBN number is required.")]
        [StringLength(13, ErrorMessage = "ISBN number must be 13 characters long.")]
        public string ISBNumber { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number.")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public double? Price { get; set; }

        [StringLength(500, ErrorMessage = "Thumbnail URL cannot be longer than 500 characters.")]
        public string? Thumbnail { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Fine must be a positive number.")]
        public double? Fine { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}
