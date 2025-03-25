using System.Text.Json.Serialization;
using Model.Entity.LibraryRoom;

namespace Model.Entity.book;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public sealed class BookInstance
{
  [Key]
  public int Id { get; set; }

  [MaxLength(10)]
  public string? Code { get; set; }

  public long? RowShelfId { get; set; }
  public int? BookId { get; set; }
  public int? Position { get; set; }
  public DateTime? DeletedAt { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.Now;
  public int? BookIdRestore { get; set; }

  [JsonIgnore]
  [ForeignKey("BookId")]
  public Book Book { get; set; } = null!;

  [JsonIgnore]
  [ForeignKey("RowShelfId")]
  public RowShelf? RowShelf { get; set; }
}