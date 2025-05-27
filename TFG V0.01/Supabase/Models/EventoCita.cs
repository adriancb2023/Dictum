using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TFG_V0._01.Converters;

namespace TFG_V0._01.Supabase.Models
{
    [Table("eventos_citas")]
    public class EventoCita : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }

        [Column("id_caso")]
        public int IdCaso { get; set; }

        [Column("titulo")]
        public string Titulo { get; set; }

        [Column("descripcion")]
        public string Descripcion { get; set; }

        [Column("id_estado")]
        public int IdEstado { get; set; }

        [Column("fecha")]
        [JsonProperty("fecha")]
        public string FechaString { get; set; }

        [JsonIgnore]
        public DateTime Fecha
        {
            get
            {
                if (DateTime.TryParse(FechaString, out var dt))
                    return dt;
                return default;
            }
            set
            {
                FechaString = value.ToString("yyyy-MM-dd");
            }
        }

        [Column("fecha_inicio")]
        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan FechaInicio { get; set; }

        public string EstadoColor { get; set; }
    }
} 