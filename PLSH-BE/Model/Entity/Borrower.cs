using Microsoft.EntityFrameworkCore;

namespace Model.Entity;

public class Borrower
{
  public int Id { get; set; }
  // public string FullName { get; set; }
  public DateOnly BirthDate { get; set; }
  public string RoleInSchool { get; set; }//(student or teacher)
  public string? ClassRoom { get; set; }//if student
  public int AccountId { get; set; }

  public int FavoriteId { get; set; }
  public int LoanId { get; set; }

  //public virtual Account Account { get; set; }
  //public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
  //public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
}