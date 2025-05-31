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
    [Table("documentos")]
    public class Documento : BaseModel
    {
        [PrimaryKey("id", true)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? id { get; set; }
        public int id_caso { get; set; }
        public string nombre { get; set; }
        public string ruta { get; set; }
        public DateTime fecha_subid { get; set; }
        public int tipo_documento { get; set; }
        public string extension_archivo { get; set; }

        [Reference(typeof(Caso))]
        public Caso Caso { get; set; }

        // Navigation property for TipoDocumento
        [Reference(typeof(TipoDocumento))]
        public TipoDocumento TipoDocumento { get; set; }

        // DTO para inserción sin id
        public class DocumentoInsertDto : BaseModel
        {
            [Column("id_caso")]
            public int id_caso { get; set; }
            [Column("nombre")]
            public string nombre { get; set; }
            [Column("ruta")]
            public string ruta { get; set; }
            [Column("fecha_subid")]
            public DateTime fecha_subid { get; set; }
            [Column("tipo_documento")]
            public int tipo_documento { get; set; }
            [Column("extension_archivo")]
            public string extension_archivo { get; set; }
        }
    }
}
