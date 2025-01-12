using System.Diagnostics.CodeAnalysis;

namespace Common.Message.ResposeMessage
{
    [ExcludeFromCodeCoverage]
    public class Response
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
