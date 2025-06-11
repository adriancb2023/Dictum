using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TFG_V0._01.Models
{
    public class Provincia : CheckableItem
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string CodigoComunidad { get; set; }
        public ObservableCollection<Sede> Sedes { get; set; } = new ObservableCollection<Sede>();

        public override bool IsChecked
        {
            get => base.IsChecked;
            set
            {
                if (base.IsChecked != value)
                {
                    base.IsChecked = value;
                    // Marcar/desmarcar todas las sedes
                    foreach (var sede in Sedes)
                    {
                        sede.IsChecked = value;
                    }
                }
            }
        }

        public Provincia()
        {
            Sedes.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (Sede sede in e.NewItems)
                    {
                        sede.PropertyChanged += Sede_PropertyChanged;
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (Sede sede in e.OldItems)
                    {
                        sede.PropertyChanged -= Sede_PropertyChanged;
                    }
                }
            };
            foreach (var sede in Sedes)
            {
                sede.PropertyChanged += Sede_PropertyChanged;
            }
        }

        private void Sede_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsChecked))
            {
                if (Sedes.All(s => s.IsChecked))
                    base.IsChecked = true;
                else if (Sedes.All(s => !s.IsChecked))
                    base.IsChecked = false;
                OnPropertyChanged(nameof(IsChecked));
            }
        }
    }
} 