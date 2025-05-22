using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace TFG_V0._01.Ventanas.SubVentanas
{
    public partial class EditarDocumentoWindow : Window, INotifyPropertyChanged
    {
        private string _tituloVentana;
        private string _nombre;
        private string _descripcion;
        private string _tipoDocumento;
        private ObservableCollection<string> _tiposDocumento;
        private string _rutaArchivo;

        public string TituloVentana
        {
            get => _tituloVentana;
            set { _tituloVentana = value; OnPropertyChanged(nameof(TituloVentana)); }
        }
        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(nameof(Nombre)); }
        }
        public string Descripcion
        {
            get => _descripcion;
            set { _descripcion = value; OnPropertyChanged(nameof(Descripcion)); }
        }
        public string TipoDocumento
        {
            get => _tipoDocumento;
            set { _tipoDocumento = value; OnPropertyChanged(nameof(TipoDocumento)); }
        }
        public ObservableCollection<string> TiposDocumento
        {
            get => _tiposDocumento;
            set { _tiposDocumento = value; OnPropertyChanged(nameof(TiposDocumento)); }
        }
        public string RutaArchivo
        {
            get => _rutaArchivo;
            set { _rutaArchivo = value; OnPropertyChanged(nameof(RutaArchivo)); }
        }

        public EditarDocumentoWindow(string nombre = "", string descripcion = "", string tipoDocumento = "")
        {
            InitializeComponent();
            DataContext = this;
            TituloVentana = string.IsNullOrEmpty(nombre) ? "Nuevo Documento" : "Modificar Documento";
            Nombre = nombre;
            Descripcion = descripcion;
            TipoDocumento = tipoDocumento;
            TiposDocumento = new ObservableCollection<string> { "Contrato", "Factura", "Informe", "Otro" };
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                MessageBox.Show("Por favor, ingrese un nombre para el documento.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        #region Window Controls
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        #endregion

        private void Examinar_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                RutaArchivo = dialog.FileName;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 