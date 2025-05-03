using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Entity.book
{
  public class AudioBook
  {
    public int Id { get; set; }

    // public int LibrarianId { get; set; }
    // public required Account Librarian { get; set; }
    public DateTime Duration { get; set; }
    public bool IsAvailable { get; set; } = false;
    public DateTime EstimatedTime { get; set; }
    public int Chapter { get; set; }
    public int BookId { get; set; }
    public int AudioResourceId { get; set; }

    [ForeignKey("AudioResourceId")]
    public required Resource AudioFile { get; set; }

    public string? Voice { get; set; }
  }
}
