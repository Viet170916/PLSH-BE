namespace Model.Entity;

public class Librarian
{
  public int Id { get; set; }
  public string? IdentityCardNumber { get; set; }
  public int AccountId { get; set; }

    // Liên k?t danh sách phi?u m??n c?a ng??i này
  //public List<Loan> Loans { get; set; } = new();
}