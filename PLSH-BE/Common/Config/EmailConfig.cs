using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;

namespace Common.Config
{
    [ExcludeFromCodeCoverage]
    public class EmailConfig
    {
        public String SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool SmtpEnableSsl { get; set; }
        public string SmtpDeliveryMethod { get; set; }
        public int SmtpTimeout { get; set; }
        public String SenderEmail { get; set; }
        public String SmtpAccount { get; set; }
        public String SmtpAccountPassword { get; set; }
        public int FailCount { get; set; }
        public string ReceiverEmails { get; set; }
    }
}
