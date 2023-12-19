using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Migrator.Common
{
    public class MigrationData
    {
        public required string ProviderName { get; init; }

        public required IMail Mail { get; init; }


        public byte[] Serialize()
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this));
        }

        public static MigrationData? Deserialize(byte[] data)
        {
            return JsonSerializer.Deserialize<MigrationData>(Encoding.UTF8.GetString(data));
        }

        private static JsonSerializerOptions GetSerializerOptions()
        {
            return new JsonSerializerOptions()
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };
        }
    }
}
