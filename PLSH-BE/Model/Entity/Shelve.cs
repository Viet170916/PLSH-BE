using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
    public class Shelve
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public string? Location { get; set; }

        public int capacity = 100;

        //public ICollection<Bookshelf> Bookshelves { get; set; }
    }
}
