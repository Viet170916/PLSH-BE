using System.ComponentModel.DataAnnotations;

namespace Model.Entity.User;

public class Librarian
{
  public int Id { get; set; }

  [MaxLength(55)]
  public string? IdentityCardNumber { get; set; }

  public int AccountId { get; set; }

  // Li�n k?t danh s�ch phi?u m??n c?a ng??i n�y
  //public List<Loan> Loans { get; set; } = new();
}