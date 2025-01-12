using System.ComponentModel;

namespace Common.Enums
{
    public enum BudgetGroupStatusEnum
    {
        [Description("Draft")]
        Draft = 0,
        [Description("Pending Approval")]
        Pending_Approval = 1,
        [Description("Approved")]
        Approved = 2,
        [Description("Rejected")]
        Rejected = 3
    }
}
