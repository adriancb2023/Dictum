using System;
using Newtonsoft.Json;

namespace TFG_V0._01.Converters
{
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
        {
            // Serializa como string en formato HH:mm:ss
            writer.WriteValue(value.ToString(@"hh\:mm\:ss"));
        }

        public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null || string.IsNullOrEmpty(reader.Value.ToString()))
            {
                return TimeSpan.Zero;
            }
            var str = (string)reader.Value;
            return TimeSpan.Parse(str);
        }
    }
} 