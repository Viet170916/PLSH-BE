using System.Diagnostics.CodeAnalysis;

namespace BU.Models
{
    [ExcludeFromCodeCoverage]
    public class ImportBuggetGroupDataItem
    {
        public string BudgetGroupStringId { get; set; }
        public int SubgroupId { get; set; }
        public string SubgroupIdString { get; set; }
        public string Pie { get; set; }
        public string NewClient { get; set; }
        public string LastYearOfAudit { get; set; }
        public string Eqcr { get; set; }
        public string Ep { get; set; }
        public string CoEm { get; set; }
    }
}
