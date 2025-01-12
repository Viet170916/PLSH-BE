using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace Model.Entity.Authentication
{
    [ExcludeFromCodeCoverage]
    public abstract class ApplicationUser : IdentityUser
    {

        public string EmployeeId { get; set; }


        public string NetWorkId { get; set; }

        public string UserSignature { get; set; }
    }
}
