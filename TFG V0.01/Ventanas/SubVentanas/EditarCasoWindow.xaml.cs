using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TFG_V0._01.Supabase.Models;
using TFG_V0._01.Supabase;
using System.Diagnostics;
using TFG_V0._01.Helpers;

namespace TFG_V0._01.Ventanas
{
    /// <summary>
    /// Interaction logic for EditarCasoWindow.xaml
    /// </summary>
    public partial class EditarCasoWindow : Window, INotifyPropertyChanged
    {
        // Implementa INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Propiedades para binding
        public DateTime? FechaInicio { get => _fechaInicio; set { _fechaInicio = value; OnPropertyChanged(nameof(FechaInicio)); } }
        private DateTime? _fechaInicio;

        public string Titulo { get => _titulo; set { _titulo = value; OnPropertyChanged(nameof(Titulo)); } }
        private string _titulo;

        public string Descripcion { get => _descripcion; set { _descripcion = value; OnPropertyChanged(nameof(Descripcion)); } }
        private string _descripcion;

        private ObservableCollection<Estado> _estadosDisponibles;
        public ObservableCollection<Estado> EstadosDisponibles 
        { 
            get => _estadosDisponibles;
            set 
            { 
                _estadosDisponibles = value;
                OnPropertyChanged(nameof(EstadosDisponibles));
            }
        }
        public Estado EstadoSeleccionado { get => _estadoSeleccionado; set { _estadoSeleccionado = value; OnPropertyChanged(nameof(EstadoSeleccionado)); } }
        private Estado _estadoSeleccionado;

        public string MensajeError { get => _mensajeError; set { _mensajeError = value; OnPropertyChanged(nameof(MensajeError)); } }
        private string _mensajeError;

        private readonly Caso _casoOriginal;
        private readonly SupabaseCasos _supabaseCasos = new SupabaseCasos();
        public ObservableCollection<Documento> DocumentosCaso { get => _documentosCaso; set { _documentosCaso = value; OnPropertyChanged(nameof(DocumentosCaso)); } }
        private ObservableCollection<Documento> _documentosCaso = new ObservableCollection<Documento>();
        public ICommand VerDocumentoCommand { get; }
        public ICommand DescargarDocumentoCommand { get; }
        private readonly SupabaseDocumentos _supabaseDocumentos = new SupabaseDocumentos();

        public EditarCasoWindow(Caso caso, ObservableCollection<Estado> estadosDisponibles)
        {
            InitializeComponent();
            DataContext = this;
            this.Opacity = 0;

            _casoOriginal = caso;
            FechaInicio = caso.fecha_inicio;
            Titulo = caso.titulo;
            Descripcion = caso.descripcion;
            EstadosDisponibles = new ObservableCollection<Estado>(estadosDisponibles);
            EstadoSeleccionado = EstadosDisponibles.FirstOrDefault(e => e.id == caso.id_estado);

            VerDocumentoCommand = new RelayCommand<Documento>(VerDocumento);
            DescargarDocumentoCommand = new RelayCommand<Documento>(DescargarDocumento);
            _ = CargarDocumentosDelCasoAsync(caso.id);
        }

        private async Task CargarDocumentosDelCasoAsync(int casoId)
        {
            await _supabaseDocumentos.InicializarAsync();
            var docs = await _supabaseDocumentos.ObtenerPorCasoAsync(casoId);
            DocumentosCaso = new ObservableCollection<Documento>(docs);
        }

        private void VerDocumento(Documento doc)
        {
            if (doc == null) return;
            // Aquí puedes abrir el documento, por ejemplo, abrir la ruta con el visor predeterminado
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = doc.ruta,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir el documento: {ex.Message}");
            }
        }

        private void DescargarDocumento(Documento doc)
        {
            if (doc == null) return;
            // Aquí puedes implementar la lógica de descarga, por ejemplo, copiar el archivo a otra ubicación
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = doc.nombre,
                    Filter = "Todos los archivos|*.*"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    System.IO.File.Copy(doc.ruta, saveFileDialog.FileName, true);
                    MessageBox.Show("Documento descargado correctamente.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo descargar el documento: {ex.Message}");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(600))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            this.BeginAnimation(Window.OpacityProperty, fadeIn);
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (Validar())
            {
                // Actualiza el caso original
                _casoOriginal.fecha_inicio = FechaInicio ?? DateTime.Now;
                _casoOriginal.titulo = Titulo;
                _casoOriginal.descripcion = Descripcion;
                _casoOriginal.Estado = EstadoSeleccionado;
                _casoOriginal.id_estado = EstadoSeleccionado?.id ?? 0;

                // Guarda en la base de datos
                await _supabaseCasos.InicializarAsync();
                await _supabaseCasos.ActualizarAsync(_casoOriginal);

                this.DialogResult = true;
                this.Close();
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private bool Validar()
        {
            MensajeError = "";
            if (FechaInicio == null)
                MensajeError = "La fecha de inicio es obligatoria.";
            else if (string.IsNullOrWhiteSpace(Titulo))
                MensajeError = "El título es obligatorio.";
            else if (EstadoSeleccionado == null)
                MensajeError = "Debes seleccionar un estado.";

            return string.IsNullOrEmpty(MensajeError);
        }

        private void VerDetalles(Caso caso)
        {
            var ventana = new EditarCasoWindow(caso, EstadosDisponibles)
            {
                Owner = this
            };
            ventana.ShowDialog();
        }

        // Permite mover la ventana arrastrando el borde
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
