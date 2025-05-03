using BU.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BU.Services.Implementation
{
    public class OTPService : IOTPService
    {
        private readonly Dictionary<string, OtpInfo> _otpStorage = new();

        public string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString(); // 6-digit OTP
        }

        public void StoreOtp(string email, string otp)
        {
            _otpStorage[email] = new OtpInfo(
                Otp: otp,
                Expiry: DateTime.UtcNow.AddMinutes(5) // OTP hết hạn sau 5 phút
            );
        }

        public bool ValidateOtp(string email, string otp)
        {
            if (!_otpStorage.TryGetValue(email, out var otpInfo)) return false;

            // Check expiry và OTP
            return otpInfo.Otp == otp && otpInfo.Expiry > DateTime.UtcNow;
        }

        private record OtpInfo(string Otp, DateTime Expiry);
    }
}
