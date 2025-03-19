namespace Model.Entity.book
{
    public class AudioBook
    {
        public int Id {  get; set; }   
        
        public int AccountId { get; set; }  

        public DateTime Duration { get; set; }

        public bool IsAvailable { get; set; } 
        public DateTime EstimatedTime {get; set; }

        //public string? Voice {  get; set; } 
    }
}
