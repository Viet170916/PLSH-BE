namespace Model.Entity;

public class Admin
{
  public int Id { get; set; }
  public string FullName { get; set; }
  public DateOnly BirthDate { get; set; }
  public string IdentityCardNumber { get; set; }
  public int AccountId { get; set; }
}