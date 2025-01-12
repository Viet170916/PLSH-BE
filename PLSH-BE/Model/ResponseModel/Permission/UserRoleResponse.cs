using System.Diagnostics.CodeAnalysis;

namespace Model.ResponseModel.Permission
{
    [ExcludeFromCodeCoverage]
    public class UserRoleResponse
    {
        public long RoleId { get; set; }
        public string RoleCode { get; set; }

        public string RoleName { get; set; }
    }
}
