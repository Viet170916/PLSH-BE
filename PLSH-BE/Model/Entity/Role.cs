namespace Model.Entity;

public class Role
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public string? Description { get; set; }
  public int Level { get; set; }

  //public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}