using System.Text.RegularExpressions;

namespace API.DTO.Account.AccountDTO
{
    public class AccountDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự")]
        public string FullName { get; set; }

        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
        [MaxLength(50, ErrorMessage = "Mật khẩu tối đa 50 ký tự")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ cái, 1 số và 1 ký tự đặc biệt")]
        public string? Password { get; set; }

        [StringLength(12, MinimumLength = 9, ErrorMessage = "Số CMND/CCCD phải từ 9 đến 12 số")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Số CMND/CCCD chỉ được chứa số")]
        public string? IdentityCardNumber { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Vai trò không hợp lệ")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$",
            ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự")]
        public string Address { get; set; }

        // Các trường liên quan đến Google không cần validate
        public string? GoogleToken { get; set; }
        public string? GoogleUserId { get; set; }

        [Url(ErrorMessage = "URL avatar không hợp lệ")]
        public string? AvataUrl { get; set; }

        public bool isVerified { get; set; } = false;

        [Range(0, 2, ErrorMessage = "Trạng thái phải từ 0 đến 2")]
        public int Status { get; set; } = 0;

        // Các trường thời gian do hệ thống tự quản lý, không validate
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime DeletedAt { get; set; }

        // Số thẻ thành viên được sinh tự động, không validate
        public long CardMemberNumber { get; set; } = GenerateUniqueId();

        [Range(0, 4, ErrorMessage = "Trạng thái thẻ phải từ 0 đến 4")]
        public int CardMemberStatus { get; set; } = 0;

        [FutureDate(ErrorMessage = "Ngày hết hạn thẻ phải trong tương lai")]
        public DateTime CardMemberExpiredDate { get; set; } = DateTime.Now;

        // Các trường liên quan đến token không cần validate
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        // Các trường liên quan đến khóa tài khoản
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; } = null;

        private static long GenerateUniqueId()
        {
            var random = new Random();
            return random.NextInt64(100000000, 999999999);
        }
    }

    // Custom validation attribute cho ngày trong tương lai
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime date)
            {
                return date > DateTime.Now;
            }
            return false;
        }
    }
}