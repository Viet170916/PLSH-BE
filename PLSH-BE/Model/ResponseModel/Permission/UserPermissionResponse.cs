using System.Diagnostics.CodeAnalysis;

namespace Model.ResponseModel.Permission
{
    [ExcludeFromCodeCoverage]
    public class UserPermissionResponse
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string EmployeeName { get; set; }
        public string EmailAddress { get; set; }
        public string UserRole { get; set; }
        public string EmployeeId { get; set; }
        public string ImgUrl { get; set; }
        public string NetworkId { get; set; }
        public string BusinessUnit { get; set; }
        public string BusinessUnitName { get; set; }
        public string ProfitCenter { get; set; }
        public string ProfitCenterDesc { get; set; }
        public string Grade { get; set; }
        public List<UserRoleResponse> UserRoles { get; set; }
        public List<PermissionResponse> Permissions { get; set; }
    }
   
}
