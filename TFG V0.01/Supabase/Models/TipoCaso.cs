using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace TFG_V0._01.Supabase.Models
{
    [Table("tipos_caso")]
    public class TipoCaso : BaseModel
    {
        [PrimaryKey("id", true)]
        public int id { get; set; }
        public string nombre { get; set; }
        public string abreviatura { get; set; }
    }
}
