using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Migrator.Common;

namespace MerelyMailProvider
{
    public class MerelyMail : IMail
    {
        public string Subject { get; }
        public string Body { get; }
        public string Sender { get; }
        public string Receiver { get; }
    }
}
