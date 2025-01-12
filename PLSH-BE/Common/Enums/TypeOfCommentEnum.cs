using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Enums
{
    public enum TypeOfCommentEnum
    {
        [Description("Tagging Entities and Engagement Codes")]
        Tagging_Entities_and_Engagement_Codes = 1,

        [Description("Engagement Hours")]
        Engagement_Hours = 2,

        [Description("Audit Hours")]
        Audit_Hours = 3,

        [Description("Audit Hours Reconciliaton")]
        Audit_Hours_Reconciliaton = 4,

        [Description("Specialist Hours")]
        Specialist_Hours = 5,

        [Description("Reporting Deadlines")]
        Reporting_Deadlines = 6,

        [Description("Reporting Framework")]
        Reporting_Framework = 7,

        [Description("Gross Fees")]
        Gross_Fees = 8,

        [Description("Fee Details")]
        Fee_Details = 9,

        [Description("Billing Schedule")]
        Billing_Schedule = 10,

        [Description("Comment")]
        Comment = 11,

        [Description("Lost Client")]
        Lost_Client = 12
    }
}
