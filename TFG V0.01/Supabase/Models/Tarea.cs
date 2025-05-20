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
    [Table("tareas")]
    public class Tarea : BaseModel, INotifyPropertyChanged
    {
        [PrimaryKey("id", true)]
        public int id { get; set; }
        public string titulo { get; set; }
        public string descripcion { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime? fecha_vencimiento { get; set; }
        public bool completada { get; set; }
        public int id_caso { get; set; }
        public string prioridad { get; set; } // Alta, Media, Baja
        public string estado { get; set; } // Pendiente, En Progreso, Completada

        [Reference(typeof(Caso))]
        public Caso Caso { get; set; }

        public string TiempoRestante
        {
            get
            {
                if (fecha_vencimiento == null)
                    return "Sin fecha";
                var dias = (fecha_vencimiento.Value.Date - DateTime.Now.Date).Days;
                if (dias > 0)
                    return $"Faltan {dias} días";
                else if (dias == 0)
                    return "Vence hoy";
                else
                    return $"Venció hace {-dias} días";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
