using System.ComponentModel.DataAnnotations;

namespace Model.Entity.User
{
  public class Profile
  {
    public int Id { get; set; }

    [MaxLength(255)]
    public string? FullName { get; set; }

    public bool? Gender { get; set; } // true: Nam, false: Nữ, null: Không xác định
    public DateTime? Birthdate { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(15)]
    public string? PhoneNumber { get; set; }

    [MaxLength(255)]
    public string? Email { get; set; }
    [MaxLength(255)]

    public string? AvatarUrl { get; set; } // Ảnh đại diện
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int AccountId { get; set; }

    //public virtual AccountControllers AccountControllers { get; set; } 
  }
}