using System.Diagnostics.CodeAnalysis;

namespace BU.Models
{
    [ExcludeFromCodeCoverage]
    public class ImportBillingScheduleDataItem
    {
        public string EntityID { get; set; }
        public string BillingParty { get; set; }
        public string TaxClassification { get; set; }
        public string Ccy { get; set; }
        public string Jan { get; set; }
        public string Feb { get; set; }
        public string Mar { get; set; }
        public string Apr { get; set; }
        public string May { get; set; }
        public string Jun { get; set; }
        public string Jul { get; set; }
        public string Aug { get; set; }
        public string Sep { get; set; }
        public string Oct { get; set; }
        public string Nov { get; set; }
        public string Dec { get; set; }
    }
}
