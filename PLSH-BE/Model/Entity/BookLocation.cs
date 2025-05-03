using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
  public class BookLocation
  {
    [Key]
    public int Id { get; set; }

    [ForeignKey("Book")]
    public int BookId { get; set; }
    //public Book Book { get; set; }

    [ForeignKey("Shelf")]
    public int ShelfId { get; set; }
    //public Shelf Shelf { get; set; }

    [ForeignKey("Bookshelf")]
    public int BookshelfId { get; set; }
    //public Bookshelf Bookshelf { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Position { get; set; }

    public int Quantity { get; set; }
  }
}