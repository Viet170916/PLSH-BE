using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Entity
{
    public class HistoryReview
    {
        public int Id { get; set; }

        public int AccountId { get; set; }
        [MaxLength(255)]

        public string? SearchQuery { get; set; }  

        public DateTime? CreateAt { get; set; } = DateTime.Now;


        //[ForeignKey("AccountId")]
        ////public AccountControllers? AccountControllers { get; set; }

    }
}
