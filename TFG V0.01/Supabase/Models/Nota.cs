using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.ComponentModel;

namespace TFG_V0._01.Supabase.Models
{
    [Table("notas")]
    public class Nota : BaseModel, INotifyPropertyChanged
    {
        [PrimaryKey("id", true)]
        public int Id { get; set; }

        [Column("id_caso")]
        public int IdCaso { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; }

        [Column("descripcion")]
        public string Descripcion { get; set; }

        [Column("fecha_creacion")]
        public DateTime? FechaCreacion { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 