using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmostMailProvider
{
    internal class Mailbox
    {
        public double MailboxSize { get; set; }
        public int MailboxQuota { get; set; }
        public List<AlmostMail>? Mails { get; set; }
        public required string Password { get; set; }
    }
}
