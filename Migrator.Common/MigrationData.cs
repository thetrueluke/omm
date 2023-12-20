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
        public required string ProviderName { get; init; }

        public required Mailbox Mailbox { get; init; }

        public required IMail Mail { get; init; }


        public byte[] Serialize()
        {            
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this, GetSerializerOptions()));
        }

        public static MigrationData? Deserialize(byte[] data)
        {
            return JsonConvert.DeserializeObject<MigrationData>(Encoding.UTF8.GetString(data), GetSerializerOptions());
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
