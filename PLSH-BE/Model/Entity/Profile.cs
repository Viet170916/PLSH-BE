using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
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

    //public virtual Account Account { get; set; } 
  }
}