using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Common.Infrastructure.Cores
{
    [ExcludeFromCodeCoverage]
    public class PagedResults<T> : IPagedResults<T>
    {
        public PagedResults()
        {
        }

        public PagedResults(IEnumerable<T> items, int totalCount)
        {
            Items = items;
            Total = totalCount;
        }

        public int Total { get; set; }
        public IEnumerable<T> Items { get; set; }
        public string Status { get; set; }
        public string ResponseMessage { get; set; }
        public bool? HasMessagePercent { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public class PagedResultsUpdate<T> : IPagedResults<T>
    {
        public PagedResultsUpdate()
        {
        }

        public PagedResultsUpdate(IEnumerable<T> items, int totalCount)
        {
            Items = items;
            Total = totalCount;
        }

        public int Total { get; set; }
        public IEnumerable<T> Items { get; set; }
        public string Status { get; set; }
        public string ResponseMessage { get; set; }
        public string NameLastUpdatedBy { get; set; }
        public string LastUpdatedDateAll { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class PagedResult<T> : IPagedResult<T>
    {
        public PagedResult(T data, string status, string responseMessage)
        {
            Data = data;
            Status = status;
            ResponseMessage = responseMessage;
        }

        public PagedResult()
        {
        }

        public T Data { get; set; }
        public string Status { get; set; }
        public string ResponseMessage { get; set; }
        public bool? HasMessagePercent { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class PagedImportResult<T> : IPagedImportResult<T>
    {
        public PagedImportResult(T data,string fileName, string status, string responseMessage)
        {
            Data = data;
            FileName = fileName;
            Status = status;
            ResponseMessage = responseMessage;
        }

        public PagedImportResult()
        {
        }

        public T Data { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; }
        public string ResponseMessage { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public class PagedListResponseResults<T> : IPagedListResponseResults<T>
    {
        public PagedListResponseResults()
        {
        }

        public PagedListResponseResults(IEnumerable<T> items, int totalCount)
        {
            Items = items;
            Total = totalCount;
        }

        public int Total { get; set; }
        public IEnumerable<T> Items { get; set; }
        public string Status { get; set; }
        public List<string> ResponseMessage { get; set; }
    }
}
