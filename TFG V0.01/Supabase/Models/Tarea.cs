using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.ComponentModel;
using Newtonsoft.Json;

namespace TFG_V0._01.Supabase.Models
{
    [Table("tareas")]
    public class Tarea : BaseModel, INotifyPropertyChanged
    {
        [PrimaryKey("id", true)]
        [Column("id")]
        public int? id { get; set; }
        [Column("titulo")]
        public string titulo { get; set; }
        [Column("descripcion")]
        public string descripcion { get; set; }
        [Column("fecha_creacion")]
        public DateTime fecha_creacion { get; set; }
        [Column("fecha_fin")]
        public DateTime? fecha_fin { get; set; }
        [JsonIgnore]
        private bool _completada;
        [JsonIgnore]
        public bool completada 
        { 
            get => _completada;
            set
            {
                if (_completada != value)
                {
                    _completada = value;
                    estado = value ? "Completada" : "Pendiente";
                    OnPropertyChanged(nameof(completada));
                    OnPropertyChanged(nameof(estado));
                }
            }
        }
        [Column("id_caso")]
        public int? id_caso { get; set; }
        [Column("prioridad")]
        public string prioridad { get; set; } // Alta, Media, Baja
        private string _estado;
        [Column("estado")]
        public string estado 
        { 
            get => _estado;
            set
            {
                if (_estado != value)
                {
                    _estado = value;
                    _completada = value == "Completada";
                    OnPropertyChanged(nameof(estado));
                    OnPropertyChanged(nameof(completada));
                }
            }
        }

        [Reference(typeof(Caso))]
        public Caso Caso { get; set; }

        [JsonIgnore]
        public string TiempoRestante
        {
            get
            {
                if (fecha_fin == null)
                    return "Sin fecha";
                var dias = (fecha_fin.Value.Date - DateTime.Now.Date).Days;
                if (dias > 0)
                    return $"Faltan {dias} días";
                else if (dias == 0)
                    return "Vence hoy";
                else
                    return $"Venció hace {-dias} días";
            }
        }

        [JsonIgnore]
        public string estadoAnterior { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // DTO para inserción
    [Table("tareas")]
    public class TareaInsertDto : BaseModel
    {
        [Column("titulo")]
        public string titulo { get; set; }
        [Column("descripcion")]
        public string descripcion { get; set; }
        [Column("fecha_creacion")]
        public DateTime fecha_creacion { get; set; }
        [Column("fecha_fin")]
        public DateTime? fecha_fin { get; set; }
        [Column("id_caso")]
        public int? id_caso { get; set; }
        [Column("prioridad")]
        public string prioridad { get; set; }
        [Column("estado")]
        public string estado { get; set; }
    }

    public class TareaUpdateDto
    {
        [Column("titulo")]
        public string titulo { get; set; }
        [Column("descripcion")]
        public string descripcion { get; set; }
        [Column("fecha_creacion")]
        public DateTime fecha_creacion { get; set; }
        [Column("fecha_fin")]
        public DateTime? fecha_fin { get; set; }
        [Column("id_caso")]
        public int? id_caso { get; set; }
        [Column("prioridad")]
        public string prioridad { get; set; }
        [Column("estado")]
        public string estado { get; set; }
    }
}
