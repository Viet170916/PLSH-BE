namespace API.DTO.Account.AccountDTO;

public class AccountGDto
{
  public int Id { get; set; }
  public string? FullName { get; set; }
  public string? Email { get; set; }
  public string? PhoneNumber { get; set; }
  public string? AvatarUrl { get; set; }
  public string Role { get; set; } = "";
  public bool IsVerified { get; set; }
  public bool? Gender { get; set; }
  public DateTime? Birthdate { get; set; }
  public string? Address { get; set; }
  public string? IdentityCardNumber { get; set; }
  // public int? RoleId { get; set; }
  public string Status { get; set; } = "";
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public DateTime? DeletedAt { get; set; }
  public long? CardMemberNumber { get; set; }
  public int? CardMemberStatus { get; set; }
  public DateTime? CardMemberExpiredDate { get; set; }
  public int? FailedLoginAttempts { get; set; }
  public DateTime? LockoutEnd { get; set; }
}