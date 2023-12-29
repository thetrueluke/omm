using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
//JsonConvert used here because, unlike json💩net, it has reliable TypeNameHandling.
//Of course, in a production system this wouldn't be nesessary, as resolving types would be handled by a json converter.

namespace Migrator.Common
{
    public class MigrationData
    {
        private readonly static Encoding encoding = Encoding.Unicode;


        public required string ProviderName { get; init; }

        public required Mailbox Mailbox { get; init; }

        public required IMail Mail { get; init; }


        public byte[] Serialize()
        {
            var serialized = JsonConvert.SerializeObject(this, GetSerializerOptions());
            var bytes = encoding.GetBytes(serialized);
            return bytes;
        }

        public static MigrationData? Deserialize(byte[] data)
        {
            var serialized = encoding.GetString(data);
            var deserialized = JsonConvert.DeserializeObject<MigrationData>(serialized, GetSerializerOptions());
            return deserialized;
        }

        private static JsonSerializerSettings GetSerializerOptions()
        {
            return new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }
    }
}
