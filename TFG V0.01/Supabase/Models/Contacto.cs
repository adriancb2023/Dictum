using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace TFG_V0._01.Supabase.Models
{
    [Table("contactos")]
    public class Contacto : BaseModel
    {
        [PrimaryKey("id", true)]
        public int id { get; set; }
        public int id_caso { get; set; }
        public string nombre { get; set; }
        public string tipo { get; set; }
        public string telefono { get; set; }
        public string email { get; set; }

        [Reference(typeof(Caso))]
        public Caso Caso { get; set; }
    }
}
