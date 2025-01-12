using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace Model.Entity.Authentication
{
    [ExcludeFromCodeCoverage]
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole()
        {
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
        }
    }
}
