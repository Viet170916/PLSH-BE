using System.Diagnostics.CodeAnalysis;

namespace Model.ResponseModel.Permission
{
    [ExcludeFromCodeCoverage]
    public class PermissionResponse
    {
        public int? PermissionValue { get; set; }
        public string RoleCode { get; set; }
        public string FunctionCode { get; set; }
    }
}
