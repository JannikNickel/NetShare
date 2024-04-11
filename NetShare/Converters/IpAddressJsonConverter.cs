using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetShare.Converters
{
    public class IpAddressJsonConverter : JsonConverter<IPAddress>
    {
        public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }

        public override IPAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(IPAddress.TryParse(reader.GetString(), out IPAddress? address))
            {
                return address;
            }
            return null;
        }
    }
}
