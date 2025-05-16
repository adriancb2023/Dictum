using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace TFG_V0._01.Supabase.Models
{
    [Table("expedientes")]
    public class Expediente : BaseModel
    {
        [PrimaryKey("id", true)]
        public int id { get; set; }
        public int id_caso { get; set; }
        public string num_expediente { get; set; }
        public string juzgado { get; set; }
        public string jurisdiccion { get; set; }
        public DateTime fecha_inicio { get; set; }
        public string observaciones { get; set; }

        [Reference(typeof(Caso))]
        public Caso Caso { get; set; }
    }
}
