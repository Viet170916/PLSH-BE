using System.ComponentModel;

namespace Common.Enums
{
    public enum APICBudgetStatusEnum
    {
        [Description("Submitted")]
        Submitted = 0,
        [Description("Accepted")]
        Accepted = 1,
        [Description("Returned")]
        Returned = 2,
    }
}
