using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Enums
{
    public enum BudgetGroupCategory
    {
        [Description("Post BudgetGroup")]
        Post_BudgetGroup = 1,
        [Description("Pre BudgetGroup")]
        Pre_BudgetGroup = 2
    }
}
