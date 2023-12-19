using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrator.Common
{
    public record class Mailbox
    {
        public required string Name { get; init; }
        public required string Password { get; init; }
        public required int Quota { get; init; }
    }
}
