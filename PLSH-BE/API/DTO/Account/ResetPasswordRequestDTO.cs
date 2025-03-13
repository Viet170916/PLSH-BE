namespace API.DTO.Account
{
    public class ResetPasswordRequestDto
    {
        public string Token { get; set; }
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; }
    }
}
