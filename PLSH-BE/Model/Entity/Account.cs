namespace Model.Entity;

public class Account
{
  public int Id { get; set; }
  public string Email { get; set; }
  public string FullName { get; set; }
  public string? IdentityCardNumber { get; set; }
  public int RoleId { get; set; }
  public string PhoneNumber { get; set; }
  public string GoogleToken { get; set; }
  public string UserId { get; set; }
  public string AvataUrl { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public DateTime DeletedAt { get; set; }
  public long CardMemberNumber { get; set; } = GenerateUniqueId();
  public int CardMemberStatus { get; set; } = 0; //(chưa kích hoạt, đang chờ duyệt, đã kích hoạt, tạm khoá, đã huỷ )
  public DateTime CardMemberExpiredDate { get; set; } = DateTime.Now;

  private static long GenerateUniqueId()
  {
    var random = new Random();
    return random.NextInt64(100000000, 999999999); // Sinh số ngẫu nhiên từ 9 chữ số
  }
}