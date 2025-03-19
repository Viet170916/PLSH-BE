using System.ComponentModel.DataAnnotations;

namespace Model.Entity;

public class Role
{
  public int Id { get; set; }

  [MaxLength(30)]
  public string? Name { get; set; }

  [MaxLength(255)]
  public string? Description { get; set; }

  public int Level { get; set; }

  //public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}