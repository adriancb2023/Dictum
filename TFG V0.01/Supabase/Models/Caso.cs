using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace TFG_V0._01.Supabase.Models
{
    [Table("casos")]
    public class Caso : BaseModel, INotifyPropertyChanged
    {
        [PrimaryKey("id", true)]
        [Column("id")]
        public int id { get; set; }

        [Column("id_cliente")]
        public int id_cliente { get; set; }

        [Column("titulo")]
        public string titulo { get; set; }

        [Column("descripcion")]
        public string descripcion { get; set; }

        [Column("fecha_inicio")]
        public DateTime fecha_inicio { get; set; }

        [Column("id_estado")]
        public int id_estado { get; set; }

        [Column("id_tipo_caso")]
        public int id_tipo_caso { get; set; }

        [Column("referencia")]
        public string referencia { get; set; }

        [Reference(typeof(Cliente))]
        public Cliente Cliente { get; set; }

        private Estado _estado;
        [Reference(typeof(Estado))]
        public Estado Estado
        {
            get => _estado;
            set
            {
                if (_estado != value)
                {
                    _estado = value;
                    OnPropertyChanged(nameof(Estado));
                    OnPropertyChanged(nameof(estado_nombre));
                }
            }
        }

        [Reference(typeof(TipoCaso))]
        public TipoCaso TipoCaso { get; set; }

        [Reference(typeof(Alerta))]
        public List<Alerta> Alertas { get; set; }

        [Reference(typeof(Documento))]
        public List<Documento> Documentos { get; set; }

        [Reference(typeof(Tarea))]
        public List<Tarea> Tareas { get; set; }

        public string nombre_cliente => Cliente?.nombre ?? "Sin cliente";
        public string estado_nombre => Estado?.nombre ?? "Sin estado";
        public string tipo_nombre => TipoCaso?.nombre ?? "Sin tipo";
        public string tipo_abreviatura => TipoCaso?.abreviatura ?? "--";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{referencia} - {titulo} - {Estado?.nombre}";
        }
    }
}
