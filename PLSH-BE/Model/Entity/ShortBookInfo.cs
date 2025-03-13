using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
    public class ShortBookInfo
    {
        public int? Id { get; set; }
        public string? Title { get; set; }
        public int? PageCount { get; set; }
        public string? CoverUrl { get; set; }
        public DateTime? PublishDate { get; set; }
    }
}
