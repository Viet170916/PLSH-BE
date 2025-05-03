using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BU.Services.Interface
{
    public interface IOTPService
    {
        string GenerateOtp();
        void StoreOtp(string email, string otp);
        bool ValidateOtp(string email, string otp);
    }
}
