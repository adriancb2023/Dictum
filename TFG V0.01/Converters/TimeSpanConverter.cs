using System;
using Newtonsoft.Json;

namespace TFG_V0._01.Converters
{
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
        {
            try
            {
                // Serializa como string en formato HH:mm:ss (24 horas)
                writer.WriteValue(value.ToString(@"hh\:mm\:ss"));
            }
            catch (Exception)
            {
                // Si hay algún error, escribir un valor por defecto
                writer.WriteValue("00:00:00");
            }
        }

        public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null || string.IsNullOrEmpty(reader.Value.ToString()))
            {
                return TimeSpan.Zero;
            }

            try
            {
                var str = reader.Value.ToString();
                if (TimeSpan.TryParse(str, out TimeSpan result))
                {
                    return result;
                }
                return TimeSpan.Zero;
            }
            catch (Exception)
            {
                return TimeSpan.Zero;
            }
        }
    }
} 