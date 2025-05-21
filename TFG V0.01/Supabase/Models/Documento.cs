using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace TFG_V0._01.Supabase.Models
{
    [Table("documentos")]
    public class Documento : BaseModel
    {
        [PrimaryKey("id", true)]
        public int id { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string ruta { get; set; }
        public DateTime fecha_subid { get; set; }
        public int id_caso { get; set; }
        public string tipo_documento { get; set; }
        public string tamanio { get; set; }

        [Reference(typeof(Caso))]
        public Caso Caso { get; set; }
    }
}
