using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace TFG_V0._01.Ventanas.SubVentanas
{
    public partial class EditarTareaWindow : Window, INotifyPropertyChanged
    {
        private string _tituloVentana;
        private string _titulo;
        private string _descripcion;
        private DateTime? _fechaVencimiento;
        private string _prioridad;
        private ObservableCollection<string> _prioridades;
        private string _estadoSeleccionado;
        private ObservableCollection<string> _estadosDisponibles;

        public string TituloVentana
        {
            get => _tituloVentana;
            set { _tituloVentana = value; OnPropertyChanged(nameof(TituloVentana)); }
        }
        public string Titulo
        {
            get => _titulo;
            set { _titulo = value; OnPropertyChanged(nameof(Titulo)); }
        }
        public string Descripcion
        {
            get => _descripcion;
            set { _descripcion = value; OnPropertyChanged(nameof(Descripcion)); }
        }
        public DateTime? FechaVencimiento
        {
            get => _fechaVencimiento;
            set { _fechaVencimiento = value; OnPropertyChanged(nameof(FechaVencimiento)); }
        }
        public string Prioridad
        {
            get => _prioridad;
            set { _prioridad = value; OnPropertyChanged(nameof(Prioridad)); }
        }
        public ObservableCollection<string> Prioridades
        {
            get => _prioridades;
            set { _prioridades = value; OnPropertyChanged(nameof(Prioridades)); }
        }
        public string EstadoSeleccionado
        {
            get => _estadoSeleccionado;
            set { _estadoSeleccionado = value; OnPropertyChanged(nameof(EstadoSeleccionado)); }
        }
        public ObservableCollection<string> EstadosDisponibles
        {
            get => _estadosDisponibles;
            set { _estadosDisponibles = value; OnPropertyChanged(nameof(EstadosDisponibles)); }
        }

        public EditarTareaWindow(string titulo = "", string descripcion = "", DateTime? fechaVencimiento = null, string prioridad = "", string estado = "")
        {
            InitializeComponent();
            DataContext = this;
            TituloVentana = string.IsNullOrEmpty(titulo) ? "Nueva Tarea" : "Modificar Tarea";
            Titulo = titulo;
            Descripcion = descripcion;
            FechaVencimiento = fechaVencimiento;
            Prioridad = prioridad;
            Prioridades = new ObservableCollection<string> { "Alta", "Media", "Baja" };
            EstadosDisponibles = new ObservableCollection<string> { "Pendiente", "En progreso" };
            EstadoSeleccionado = string.IsNullOrEmpty(estado) ? EstadosDisponibles[0] : estado;
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Titulo))
            {
                MessageBox.Show("Por favor, ingrese un título para la tarea.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 