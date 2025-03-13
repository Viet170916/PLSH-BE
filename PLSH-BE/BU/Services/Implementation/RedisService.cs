using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;



namespace BU.Services.Implementation
{
    public class RedisService
    {
        private readonly IDatabase _database;
        private readonly TimeSpan _otpExpiration;

        public RedisService(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");

            var redisConnectionString = configuration["Redis:ConnectionString"];
            if (string.IsNullOrEmpty(redisConnectionString))
                throw new ArgumentException("Redis connection string is missing", nameof(configuration));

            var redis = ConnectionMultiplexer.Connect(redisConnectionString);
            _database = redis.GetDatabase();

            _otpExpiration = TimeSpan.FromMinutes(int.Parse(configuration["Redis:OTPExpirationMinutes"] ?? "5"));
        }

        // Lưu email với OTP vào Redis
        public async Task SetOtpAsync(string email, string otp)
        {
            await _database.StringSetAsync($"OTP:{email}", otp, _otpExpiration);
        }

        // Lấy OTP theo email
        public async Task<string?> GetOtpAsync(string email)
        {
            return await _database.StringGetAsync($"OTP:{email}");
        }

        //Xóa OTP sau khi xác minh
        public async Task<bool> RemoveOtpAsync(string email)
        {
            return await _database.KeyDeleteAsync($"OTP:{email}");
        }
    }
}
