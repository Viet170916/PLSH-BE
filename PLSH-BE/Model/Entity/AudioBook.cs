using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
    public class AudioBook
    {
        public int Id {  get; set; }   
        
        public int AccountId { get; set; }  

        public DateTime Duration { get; set; }

        public bool IsAvaible { get; set; } 
        public DateTime EstimatedTime {get; set; }

        //public string? Voice {  get; set; } 
    }
}
