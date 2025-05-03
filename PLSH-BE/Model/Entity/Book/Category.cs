using System.ComponentModel.DataAnnotations;
using Common.Enums;

namespace Model.Entity.book
{
    public class Category
    {
        public int? Id { get; set; } 
        [MaxLength(100)]
        public string? Name { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow; 
        public DateTime? UpdatedDate { get; set; }
        public CategoryStatus? Status { get; set; } = CategoryStatus.Active;

        public virtual ICollection<Book>? Books { get; set; } = new List<Book>();
    }
}
