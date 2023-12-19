using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmostMailProvider
{
    internal class Mail
    {
        public double MailboxSize { get; set; }
        public int MailboxQuota { get; set; }
        public AlmostMail[]? Mails { get; set; }
        public required string Password { get; set; }
    }
}
