namespace Model.Entity.book
{
    public class EBook
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public int EstimatePage { get; set; }

        public bool IsAvailable { get; set; }    
    }
}
