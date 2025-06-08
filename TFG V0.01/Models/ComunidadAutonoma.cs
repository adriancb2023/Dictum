using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Collections.ObjectModel;

namespace TFG_V0._01.Models
{
    public class ComunidadAutonoma : CheckableItem
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public ObservableCollection<Provincia> Provincias { get; set; } = new ObservableCollection<Provincia>();

        public override bool IsChecked
        {
            get => base.IsChecked;
            set
            {
                if (base.IsChecked != value)
                {
                    base.IsChecked = value;
                    // Marcar/desmarcar todas las provincias
                    foreach (var provincia in Provincias)
                    {
                        provincia.IsChecked = value;
                    }
                }
            }
        }

        public ComunidadAutonoma()
        {
            foreach (var provincia in Provincias)
            {
                provincia.PropertyChanged += Provincia_PropertyChanged;
            }
        }

        private void Provincia_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsChecked))
            {
                // Si todas las provincias están marcadas, marcar la comunidad; si alguna no, desmarcar
                if (Provincias.All(p => p.IsChecked))
                    base.IsChecked = true;
                else
                    base.IsChecked = false;
                OnPropertyChanged(nameof(IsChecked));
            }
        }
    }
} 