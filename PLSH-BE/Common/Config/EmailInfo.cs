using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Common.Config
{
    [ExcludeFromCodeCoverage]
    public class EmailInfo
    {
        public string To { get; set; }
        public IEnumerable<string> Bcc { get; set; }
        public IEnumerable<string> Cc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public byte[] Attactment { get; set; }
        public string AttactmentName { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public class RazorModel<T>
    {
        public string EmployeeName { get; set; }
        public T Data { get; set; }
        public UrlConfig Url { get; set; }

    }
    [ExcludeFromCodeCoverage]
    public class EmailStatus
    {
        public string To { get; set; }
        public string Status { get; set; }
        public DateTime Time = DateTime.Now;

        public override string ToString()
        {
            return "Email Sent To " + To + " " + Status + " at " + Time;
        }
    }
}
