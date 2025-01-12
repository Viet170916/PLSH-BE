using System.Diagnostics.CodeAnalysis;
using Common.Enums;

namespace Common.Infrastructure.Cores
{
    [ExcludeFromCodeCoverage]
    public class Pagination
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SortField { get; set; }
        public SortOrderEnum SortOrder { get; set; }
    }
}
