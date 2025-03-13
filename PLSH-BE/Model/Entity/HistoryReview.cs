using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Entity
{
    public class HistoryReview
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public string? SearchQuery { get; set; }  

        public DateTime? CreateAt { get; set; } = DateTime.Now;


        //[ForeignKey("AccountId")]
        ////public Account? Account { get; set; }

    }
}
