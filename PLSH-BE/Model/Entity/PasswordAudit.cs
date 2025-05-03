using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
  public class PasswordAudit
  {
    public int Id { get; set; }
    public int AccountId { get; set; } //ID account

    [MaxLength(255)]
    public string HashedPassword { get; set; } = string.Empty; //Mật khẩu đã mã hóa

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow; // Thời điểm thay đổi mật khẩu

    //public virtual AccountControllers? AccountControllers { get; set; }
  }
}