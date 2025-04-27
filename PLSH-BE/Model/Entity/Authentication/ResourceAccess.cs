using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Model.Entity.User;

namespace Model.Entity.Authentication;

public class ResourceAccess
{
  [Key]
  public int Id { get; set; }

  public int BookId { get; set; }
  public book.Book Book { get; set; }
  public string BookType { get; set; } = "e-book";
  public string AprroveStatus { get; set; } = "pending";
  public int AccountId { get; set; }

  [ForeignKey("AccountId")]
  public Account Account { get; set; }

  public int? LibrarianId { get; set; }

  [ForeignKey("LibrarianId")]
  public Account? Librarian { get; set; }

  public DateTime AccessDate { get; set; } = DateTime.UtcNow;
  public DateTime ExpireDate { get; set; }
}
