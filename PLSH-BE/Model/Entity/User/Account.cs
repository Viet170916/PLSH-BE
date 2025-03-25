using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
  public int Status { get; set; } = 0;
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public DateTime? DeletedAt { get; set; }
  public long CardMemberNumber { get; set; } = GenerateUniqueId();
  public int CardMemberStatus { get; set; } = 0;
  public DateTime CardMemberExpiredDate { get; set; } = DateTime.Now;

  [MaxLength(255)]
  public string? RefreshToken { get; set; }

  public DateTime? RefreshTokenExpiry { get; set; }

  private static long GenerateUniqueId()
  {
    var random = new Random();
    return random.NextInt64(100000000, 999999999); // Sinh số ngẫu nhiên từ 9 chữ số
  }

  public int FailedLoginAttempts { get; set; } = 0; // Đếm số lần đăng nhập sai
  public DateTime? LockoutEnd { get; set; } = null; // Thời gian khóa tài khoản (nếu có)

  [JsonIgnore]
  public Role Role { get; set; }
}