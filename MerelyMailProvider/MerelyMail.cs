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
        public required string Subject { get; set; }
        public string? Body { get; set; }
        public required string Sender { get; set; }
        public required string Receiver { get; set; }
        public int Size { get; set; }

        public string? Folder { get; set; }
    }
}
