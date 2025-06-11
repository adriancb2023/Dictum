using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace JurisprudenciaApi.Models
{
    public class JurisprudenciaSearchParameters
    {
        public string? TextoLibre { get; set; }
        public string? Jurisdiccion { get; set; } // Ejemplo: "Civil", "Penal"
        public List<string>? TiposResolucion { get; set; }
        public List<string>? OrganosJudiciales { get; set; }
        public List<string>? ComunidadesAutonomas { get; set; }
        public List<string>? Provincias { get; set; }
        public string? Seccion { get; set; }
        public string? Idioma { get; set; }
        public string? Legislacion { get; set; }
        public string? Roj { get; set; } // Número ROJ
        public string? Ecli { get; set; } // Identificador ECLI
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? NumeroResolucion { get; set; }
        public string? NumeroRecurso { get; set; }
        public string? Ponente { get; set; }

        // Nuevas propiedades para paginación
        public int PaginaActual { get; set; } = 1;
        public int RegistrosPorPagina { get; set; } = 10; // Default 10, como lo hace CENDOJ

        // Añadir más propiedades según sea necesario para cubrir todos los campos del formulario CENDOJ
    }

    public class CheckableItem : INotifyPropertyChanged
    {
        private bool _isChecked;
        public virtual bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class ComunidadAutonoma : CheckableItem
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public List<Provincia> Provincias { get; set; } = new List<Provincia>();

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

    public class Provincia : CheckableItem
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string CodigoComunidad { get; set; }
        public List<Sede> Sedes { get; set; } = new List<Sede>();
    }

    public class Sede
    {
        public string Nombre { get; set; }
    }
} 