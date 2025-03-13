
namespace Model.Entity
{
    public class SharedReadingList
    {
        public int Id { get; set; }

        public int OwnerId { get; set; } //Tài khoản của mình
        public int? SharedWithUserId { get; set; }//Tài khoản của người khác
        public DateTime SharedDate { get; set; } = DateTime.UtcNow;

        public bool IsPublic { get; set; } = false; //Mặc định không công khai

        public virtual ICollection<Favorite> SharedBooks { get; set; } = new List<Favorite>();
        //public virtual Borrower Owner { get; set; }
        //public virtual Borrower? SharedWithUser { get; set; }
    }

}

