using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
    public class Bookshelf
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public int Level { get; set; }
        [ForeignKey("Shelf")]
        public int ShelfId { get; set; }
        //public Shelf Shelf { get; set; }

        //public ICollection<BookLocation> BookLocations { get; set; }


    }
}
