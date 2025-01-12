using System.Diagnostics.CodeAnalysis;

namespace Data.Configurations
{
    [ExcludeFromCodeCoverage]
    public class AppConfigSection
    {
        public string DebugAccount { get; set; }
        public string DomainCrsApiUrl { get; set; }
        public string DomainBrpUrl { get; set; }
        public int TimeToUnLock { get; set; }
        public string DomainCrsUrl { get; set; }

    }
}
