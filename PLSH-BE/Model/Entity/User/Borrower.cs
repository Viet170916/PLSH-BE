using System.ComponentModel.DataAnnotations;

namespace Model.Entity.User;

public class Borrower
{
  public int Id { get; set; }

  public DateOnly BirthDate { get; set; }

  [MaxLength(10)]
  public required string RoleInSchool { get; set; } 

  [MaxLength(10)]
  public string? ClassRoom { get; set; }

  public int AccountId { get; set; }
  public int FavoriteId { get; set; }
  public int LoanId { get; set; }
}