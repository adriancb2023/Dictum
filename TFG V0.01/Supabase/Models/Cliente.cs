using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using Newtonsoft.Json;

namespace TFG_V0._01.Supabase.Models
{
    [Table("clientes")]
    public class Cliente : BaseModel
    {
        [PrimaryKey("id", true)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? id { get; set; }
        public string nombre { get; set; }
        public string apellido1 { get; set; }
        public string apellido2 { get; set; }
        public string email1 { get; set; }
        public string email2 { get; set; }
        public string telf1 { get; set; }
        public string telf2 { get; set; }
        public string direccion { get; set; }
        public DateTime fecha_contrato { get; set; }

        [JsonIgnore]
        public string nombre_cliente => $"{nombre} {apellido1} {apellido2}";

        public override string ToString()
        {
            return nombre_cliente;
        }
    }
}
