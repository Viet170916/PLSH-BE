using System.ComponentModel.DataAnnotations;

namespace Model.Entity.User;

public class Borrower
{
  public int Id { get; set; }

  // public string FullName { get; set; }
  public DateOnly BirthDate { get; set; }

  [MaxLength(10)]
  public required string RoleInSchool { get; set; } //(student or teacher)

  [MaxLength(10)]
  public string? ClassRoom { get; set; } //if student

  public int AccountId { get; set; }
  public int FavoriteId { get; set; }
  public int LoanId { get; set; }

  //public virtual AccountControllers AccountControllers { get; set; }
  //public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
  //public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
}