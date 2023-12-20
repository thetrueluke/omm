using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AlmostMailProvider
{
    internal class Mailbox
    {
        [JsonPropertyName("mailbox_size")]
        public double MailboxSize { get; set; }

        [JsonPropertyName("mailbox_quota")]
        public int MailboxQuota { get; set; }

        [JsonPropertyName("mails")]
        public List<AlmostMail>? Mails { get; set; }

        [JsonPropertyName("password")]
        public required string Password { get; set; }
    }
}
