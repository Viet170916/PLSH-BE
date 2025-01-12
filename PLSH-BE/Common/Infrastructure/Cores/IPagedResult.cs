using System.Collections.Generic;

namespace Common.Infrastructure.Cores
{
    public interface IPagedResult<T>
    {
        T Data { get; set; }
        string Status { get; set; }
        string ResponseMessage { get; set; }
        bool? HasMessagePercent { get; set; }
    }

    public interface IPagedResults<T>
    {
        public int Total { get; set; }
        public IEnumerable<T> Items { get; set; }
        public string Status { get; set; }
        public string ResponseMessage { get; set; }
    }

    public interface IPagedImportResult<T>
    {
        T Data { get; set; }
        string FileName { get; set; }
        string Status { get; set; }
        string ResponseMessage { get; set; }
    }
    public interface IPagedListResponseResults<T>
    {
        public int Total { get; set; }
        public IEnumerable<T> Items { get; set; }
        public string Status { get; set; }
        public List<string> ResponseMessage { get; set; }
    }
}
