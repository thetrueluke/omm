using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrator.Common
{
    public interface IMail
    {
        string? Subject { get; }
        string? Body { get; }
        string Sender { get; }
        string Receiver { get; }

        int Size { get; }
        string? Folder { get; }
    }
}
