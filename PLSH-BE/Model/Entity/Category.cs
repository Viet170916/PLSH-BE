using Common.Enums;

namespace Model.Entity
{
    public class Category
    {
        public int Id { get; set; } 
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow; 
        public DateTime? UpdatedDate { get; set; }
        public CategoryStatus Status { get; set; } = CategoryStatus.Active;

        //public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
