using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Model.Entity.User;

public class Role
{
  public int Id { get; set; }

  [MaxLength(12)]
  public required string Name { get; set; }

  [MaxLength(255)]
  public string? Description { get; set; }

  public int Level { get; set; }

  [JsonIgnore]
  public ICollection<Account> Accounts { get; set; } = new List<Account>();

  //public virtual ICollection<AccountControllers> Accounts { get; set; } = new List<AccountControllers>();
}