using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TFG_V0._01.Supabase.Models
{
    [Table("casos")]
    public class Caso : BaseModel
    {
        [PrimaryKey("id", true)]
        [Column("id")]
        public int id { get; set; }

        [Column("id_cliente")]
        public int id_cliente { get; set; }

        [Column("titulo")]
        public string titulo { get; set; }

        [Column("descripcion")]
        public string descripcion { get; set; }

        [Column("fecha_inicio")]
        public DateTime fecha_inicio { get; set; }

        [Column("id_estado")]
        public int id_estado { get; set; }

        [Column("id_tipo_caso")]
        public int id_tipo_caso { get; set; }

        [Column("referencia")]
        public string referencia { get; set; }

        // Propiedades de navegación y calculadas solo para uso en la app
        [JsonIgnore]
        public Cliente Cliente { get; set; }
        [JsonIgnore]
        public Estado Estado { get; set; }
        [JsonIgnore]
        public TipoCaso TipoCaso { get; set; }
        [JsonIgnore]
        public string nombre_cliente => Cliente?.nombre ?? "Sin cliente";
        [JsonIgnore]
        public string estado_nombre => Estado?.nombre ?? "Sin estado";
        [JsonIgnore]
        public string tipo_nombre => TipoCaso?.nombre ?? "Sin tipo";
        [JsonIgnore]
        public string tipo_abreviatura => TipoCaso?.abreviatura ?? "--";
        [JsonIgnore]
        public List<Alerta> Alertas { get; set; }
        [JsonIgnore]
        public string ReferenciaTituloEstado => $"{referencia ?? ""} - {titulo ?? ""} - {Estado?.nombre ?? "Sin estado"}";

        public override string ToString()
        {
            return ReferenciaTituloEstado;
        }
    }

    // DTO para inserción
    [Table("casos")]
    public class CasoInsertDto : BaseModel
    {
        [Column("id_cliente")]
        public int id_cliente { get; set; }
        [Column("titulo")]
        public string titulo { get; set; }
        [Column("descripcion")]
        public string descripcion { get; set; }
        [Column("fecha_inicio")]
        public DateTime fecha_inicio { get; set; }
        [Column("id_estado")]
        public int id_estado { get; set; }
        [Column("id_tipo_caso")]
        public int id_tipo_caso { get; set; }
        [Column("referencia")]
        public string referencia { get; set; }
    }
}
