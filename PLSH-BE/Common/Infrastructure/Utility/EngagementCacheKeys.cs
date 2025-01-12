using System.Diagnostics.CodeAnalysis;

namespace Common.Infrastructure.Utility
{
    [ExcludeFromCodeCoverage]
    public static class EngagementCacheKeys
    {
        public static string ListKey => "EngagementList";

        public static string SelectListKey => "EngagementSelectList";

        public static string GetKeyCommon(long brandId) => $"Engagement-Common-{brandId}";

        public static string GetKeyConditions(long brandId) => $"Engagement-Conditions-{brandId}";

        public static string GetKeyEAudit(long brandId) => $"Engagement-EAudit-{brandId}";

        public static string GetKeyClient(long brandId) => $"Engagement-Client-{brandId}";

        public static string GetDetailsKey(long brandId) => $"EngagementDetails-{brandId}";

        public static string GetConditionKey(string condition, long engagementId) => $"{condition}-{engagementId}";
    }
}
