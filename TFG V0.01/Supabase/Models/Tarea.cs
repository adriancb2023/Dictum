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
        public int id_caso { get; set; }
        public string titulo { get; set; }
        public string descripcion { get; set; }
        public DateTime fecha_fin { get; set; }
        private string _estado;
        public string estado
        {
            get => _estado;
            set
            {
                if (_estado != value)
                {
                    _estado = value;
                    OnPropertyChanged(nameof(estado));
                }
            }
        }

        [Reference(typeof(Caso))]
        public Caso Caso { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
