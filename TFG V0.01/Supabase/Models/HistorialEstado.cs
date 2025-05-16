using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace TFG_V0._01.Supabase.Models
{
    [Table("historial_estados")]
    public class HistorialEstado : BaseModel
    {
        [PrimaryKey("id", true)]
        public int id { get; set; }
        public int id_caso { get; set; }
        public int id_estado { get; set; }
        public DateTime fecha_cambio { get; set; }
        public string observaciones { get; set; }

        [Reference(typeof(Caso))]
        public Caso Caso { get; set; }

        [Reference(typeof(Estado))]
        public Estado Estado { get; set; }
    }
}
