namespace API.DTO.Account.LoginResponse
{
    public class LoginResponseDTO
    {
        public bool IsAuthenticated { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public int RemainingAttempts { get; set; }
    }
}
