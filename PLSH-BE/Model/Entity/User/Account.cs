using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Model.Entity.Authentication;

namespace Model.Entity.User;

public class Account
{
  public int Id { get; set; }

  [MaxLength(255)]
  public string? FullName { get; set; }

  public bool? Gender { get; set; }
  public DateTime? Birthdate { get; set; }

  [MaxLength(255)]
  public string? Address { get; set; }

  [MaxLength(20)]
  public string? PhoneNumber { get; set; }

  [MaxLength(255)]
  public string? Email { get; set; }

  [MaxLength(255)]
  public string? Password { get; set; }

  [MaxLength(255)]
  public string? AvatarUrl { get; set; }

  [MaxLength(20)]
  public string? IdentityCardNumber { get; set; }

  public int RoleId { get; set; }

  [MaxLength(255)]
  public string? GoogleToken { get; set; }

  [MaxLength(255)]
  public string? GoogleUserId { get; set; }

  public bool IsVerified { get; set; } = false;
  public string Status { get; set; } = "active";
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public DateTime? DeletedAt { get; set; }
  public string CardMemberNumber { get; set; } = GenerateUniqueId();
  public int CardMemberStatus { get; set; } = 0;
  public DateTime CardMemberExpiredDate { get; set; } = DateTime.UtcNow;

  [MaxLength(255)]
  public string? RefreshToken { get; set; }

  public DateTime? RefreshTokenExpiry { get; set; }

  private static string GenerateUniqueId()
  {
    var random = new Random();
    return random.NextInt64(100000000, 999999999).ToString();
  }

  public int FailedLoginAttempts { get; set; } = 0;
  public DateTime? LockoutEnd { get; set; } = null;

  [JsonIgnore]
  public Role Role { get; set; }

  public string? ClassRoom { get; set; }

  [InverseProperty("Account")]
  public ICollection<ResourceAccess> RequestedResources { get; set; } = new List<ResourceAccess>();

  [InverseProperty("Librarian")]
  public ICollection<ResourceAccess> ApprovedResources { get; set; } = new List<ResourceAccess>();
}
