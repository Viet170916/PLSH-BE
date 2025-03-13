namespace API.DTO.Account.AccountDTO
{
    public class UpdateProfileDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$",
            ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự")]
        public string Address { get; set; }

        [Url(ErrorMessage = "URL avatar không hợp lệ")]
        public string? AvatarUrl { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public DateTime Birthdate { get; set; }

        public bool? Gender { get; set; }

        [MaxLength(20, ErrorMessage = "Số thẻ căn cước tối đa 20 ký tự")]
        public string? IdentityCardNumber { get; set; }
    }
}
