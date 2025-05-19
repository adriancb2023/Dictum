using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace TFG.Models
{
    [Table("notas")]
    public class Nota : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }

        [Column("id_caso")]
        public int IdCaso { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; }

        [Column("descripcion")]
        public string Descripcion { get; set; }
    }
} 