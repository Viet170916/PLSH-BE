using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTO
{
    public class AccountDTO
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        // Thêm các thuộc tính bị thiếu
        public string? IdentityCardNumber { get; set; }
        public string? GoogleToken { get; set; }
        public string? GoogleUserId { get; set; }
        public string? AvataUrl { get; set; }
        public int RoleId { get; set; }
        public bool isVerified { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public long CardMemberNumber { get; set; }
        public int CardMemberStatus { get; set; }
        public DateTime CardMemberExpiredDate { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }

}
