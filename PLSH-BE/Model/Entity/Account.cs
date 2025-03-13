namespace Model.Entity;

public class Account
{
  public int Id { get; set; }
  public string? FullName { get; set; }
  public bool? Gender { get; set; }
  public DateTime? Birthdate { get; set; }
  public string? Address { get; set; }
  public string? PhoneNumber { get; set; }
  public string? Email { get; set; }
  public string? Password { get; set; }
  public string? AvatarUrl { get; set; }
  public string? IdentityCardNumber { get; set; }
  public int RoleId { get; set; }
  public string? GoogleToken { get; set; }
  public string? GoogleUserId { get; set; }
  public bool isVerified { get; set; } = false;

  public int Status { get; set; } = 0; //(chờ duyệt, đã duyệt, bị từ chối)
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public DateTime? DeletedAt { get; set; }
  public long CardMemberNumber { get; set; } = GenerateUniqueId();
  public int CardMemberStatus { get; set; } = 0; //(chưa kích hoạt, đang chờ duyệt, đã kích hoạt, tạm khoá, đã huỷ )
  public DateTime CardMemberExpiredDate { get; set; } = DateTime.Now;
  public string? RefreshToken { get; set; }
  public DateTime? RefreshTokenExpiry { get; set; }
    private static long GenerateUniqueId()
  {
    var random = new Random();
    return random.NextInt64(100000000, 999999999); // Sinh số ngẫu nhiên từ 9 chữ số
  }
  public int FailedLoginAttempts { get; set; } = 0;       // Đếm số lần đăng nhập sai
  public DateTime? LockoutEnd { get; set; } = null;       // Thời gian khóa tài khoản (nếu có)

}