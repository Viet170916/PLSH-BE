using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class PhysicalBook
    {
        public int Id { get; set; } 

        public string? QRCode { get; set; } 

        public int TotalCopies { get; set; }
        public int AvaiableCopies { get; set; } 

        public double? Price { get; set; }

        public double? Fine { get; set; }
        
    }
}
