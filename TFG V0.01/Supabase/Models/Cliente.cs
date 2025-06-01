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
        [Column("id")]
        public int? id { get; set; }
        [Column("nombre")]
        public string nombre { get; set; }
        [Column("apellido1")]
        public string apellido1 { get; set; }
        [Column("apellido2")]
        public string apellido2 { get; set; }
        [Column("email1")]
        public string email1 { get; set; }
        [Column("email2")]
        public string email2 { get; set; }
        [Column("telf1")]
        public string telf1 { get; set; }
        [Column("telf2")]
        public string telf2 { get; set; }
        [Column("direccion")]
        public string direccion { get; set; }
        [Column("fecha_contrato")]
        public DateTime fecha_contrato { get; set; }
        [Column("dni")]
        public string dni { get; set; }

        [JsonIgnore]
        public string nombre_cliente => $"{nombre} {apellido1} {apellido2}";

        public override string ToString()
        {
            return nombre_cliente;
        }
    }

    [Table("clientes")]
    public class ClienteInsertDto : BaseModel
    {
        [Column("nombre")]
        public string nombre { get; set; }
        [Column("apellido1")]
        public string apellido1 { get; set; }
        [Column("apellido2")]
        public string apellido2 { get; set; }
        [Column("email1")]
        public string email1 { get; set; }
        [Column("email2")]
        public string email2 { get; set; }
        [Column("telf1")]
        public string telf1 { get; set; }
        [Column("telf2")]
        public string telf2 { get; set; }
        [Column("direccion")]
        public string direccion { get; set; }
        [Column("fecha_contrato")]
        public DateTime fecha_contrato { get; set; }
        [Column("dni")]
        public string dni { get; set; }
    }
}
