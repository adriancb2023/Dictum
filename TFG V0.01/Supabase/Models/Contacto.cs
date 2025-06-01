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
    [Table("contactos")]
    public class Contacto : BaseModel
    {
        [PrimaryKey("id", true)]
        [Column("id")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? id { get; set; }

        [Column("id_caso")]
        public int id_caso { get; set; }

        [Column("nombre")]
        public string nombre { get; set; }

        [Column("tipo")]
        public string tipo { get; set; }

        [Column("telefono")]
        public string telefono { get; set; }

        [Column("email")]
        public string email { get; set; }

        [Reference(typeof(Caso))]
        public Caso Caso { get; set; }
    }

    [Table("contactos")]
    public class ContactoInsertDto : BaseModel
    {
        [Column("id_caso")]
        public int id_caso { get; set; }
        [Column("nombre")]
        public string nombre { get; set; }
        [Column("tipo")]
        public string tipo { get; set; }
        [Column("telefono")]
        public string telefono { get; set; }
        [Column("email")]
        public string email { get; set; }
    }
}
