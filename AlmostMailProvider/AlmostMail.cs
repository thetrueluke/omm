using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Migrator.Common;

namespace AlmostMailProvider
{
    public class AlmostMail : IMail
    {
        [JsonPropertyName("subject")]
        public string? Subject { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("from")]
        public required string Sender { get; set; }

        [JsonPropertyName("to")]
        public required string Receiver { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("filedInto")]
        public string? Folder { get; set; }
    }
}
