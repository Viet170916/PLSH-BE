namespace Model.Entity;

public class Librarian
{
  public int Id { get; set; }
  public string FullName { get; set; }
  public DateOnly BirthDate { get; set; }
  public string IdentityCardNumber { get; set; }
  public int AccountId { get; set; }
}