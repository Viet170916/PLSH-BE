using System.ComponentModel.DataAnnotations;

namespace BU.Models.DTO.Account.AccountDTO
{
    public class VerifyOtpRequestDto
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }

    public class SendOtpRequest
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
    }
}
