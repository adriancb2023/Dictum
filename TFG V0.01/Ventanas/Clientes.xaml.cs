using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TFG_V0._01.Supabase;
using System.Collections.ObjectModel;
using TFG_V0._01.Supabase.Models;
using System.Globalization;
using System.ComponentModel;
using TFG_V0._01.Helpers;
using System.IO;

namespace TFG_V0._01.Ventanas
{
    public partial class Clientes : Window, INotifyPropertyChanged
    {
        #region variables animacion
        private Storyboard fadeInStoryboard;
        private Storyboard shakeStoryboard;
        #endregion

        #region variables
        private readonly SupabaseClientes _supabaseClientes;
        private readonly SupabaseCasos _supabaseCasos;
        private readonly SupabaseCasoEtiquetas _supabaseCasoEtiquetas;
        private readonly SupabaseEstados _supabaseEstados;
        private readonly SupabaseDocumentos _supabaseDocumentos;
        public ObservableCollection<Cliente> ListaClientes { get; set; } = new ObservableCollection<Cliente>();
        private Cliente _selectedCliente;
        public ObservableCollection<Documento> DocumentosCliente { get; set; } = new ObservableCollection<Documento>();
        public ObservableCollection<Caso> HistorialCasos { get; set; } = new ObservableCollection<Caso>();
        public ObservableCollection<Caso> CasosActivos
        {
            get { return _casosActivos; }
            set
            {
                _casosActivos = value;
                OnPropertyChanged(nameof(CasosActivos));
            }
        }
        public Cliente SelectedCliente
        {
            get => _selectedCliente;
            set
            {
                _selectedCliente = value;
                if (_selectedCliente != null)
                {
                    CargarDocumentosCliente(_selectedCliente.id);
                }
                DataContext = null;
                DataContext = this;
                _ = CargarCasosActivosAsync();
                _ = CargarHistorialCasosAsync();
            }
        }
        public string EditNombre { get; set; }
        public string EditApellido1 { get; set; }
        public string EditApellido2 { get; set; }
        public string EditDNI { get; set; }
        public string EditTelefono1 { get; set; }
        public string EditTelefono2 { get; set; }
        public string EditEmail1 { get; set; }
        public string EditEmail2 { get; set; }
        public string EditDireccion { get; set; }
        public DateTime EditFechaContrato { get; set; }
        private bool isEditPersonalInfoVisible = false;
        public bool IsEditPersonalInfoVisible
        {
            get => isEditPersonalInfoVisible;
            set
            {
                isEditPersonalInfoVisible = value;
                DataContext = null;
                DataContext = this;
            }
        }
        private ObservableCollection<Caso> _casosActivos;
        private ObservableCollection<Estado> _estadosDisponibles = new ObservableCollection<Estado>();
        public ICommand VerDetallesCommand { get; }
        private ICollectionView _clientesView;
        private string _textoComboCliente;
        public string TextoComboCliente
        {
            get => _textoComboCliente;
            set { _textoComboCliente = value; OnPropertyChanged(nameof(TextoComboCliente)); }
        }
        #endregion

        #region Constructor
        public Clientes()
        {
            InitializeComponent();
            _supabaseClientes = new SupabaseClientes();
            _supabaseCasos = new SupabaseCasos();
            _supabaseCasoEtiquetas = new SupabaseCasoEtiquetas();
            _supabaseEstados = new SupabaseEstados();
            _supabaseDocumentos = new SupabaseDocumentos();
            AplicarModoSistema();
            InitializeAnimations();
            _ = CargarClientesAsync();
            this.DataContext = this;
            _casosActivos = new ObservableCollection<Caso>();
            _ = CargarCasosActivosAsync();
            VerDetallesCommand = new RelayCommand<Caso>(VerDetalles);
            _ = CargarEstadosAsync();
        }
        #endregion

        #region Eventos
        private async Task CargarClientesAsync()
        {
            try
            {
                await _supabaseClientes.InicializarAsync();
                var clientes = await _supabaseClientes.ObtenerClientesAsync();
                ListaClientes.Clear();
                foreach (var cliente in clientes)
                    ListaClientes.Add(cliente);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}");
            }
        }

        private async Task AgregarClienteAsync(Cliente nuevoCliente)
        {
            try
            {
                await _supabaseClientes.InicializarAsync();
                await _supabaseClientes.InsertarClienteAsync(nuevoCliente);
                await CargarClientesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar cliente: {ex.Message}");
            }
        }

        private async Task ActualizarClienteAsync(Cliente cliente)
        {
            try
            {
                await _supabaseClientes.InicializarAsync();
                await _supabaseClientes.ActualizarClienteAsync(cliente);
                await CargarClientesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar cliente: {ex.Message}");
            }
        }

        private async Task EliminarClienteAsync(int id)
        {
            try
            {
                await _supabaseClientes.InicializarAsync();
                await _supabaseClientes.EliminarClienteAsync(id);
                await CargarClientesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar cliente: {ex.Message}");
            }
        }
        #endregion

        #region Eventos de la interfaz

        private void EditarInformacion_Click(object sender, RoutedEventArgs e)
        {
            EditNombre = SelectedCliente.nombre;
            EditApellido1 = SelectedCliente.apellido1;
            EditApellido2 = SelectedCliente.apellido2;
            EditDNI = SelectedCliente.id.ToString();
            EditTelefono1 = SelectedCliente.telf1;
            EditTelefono2 = SelectedCliente.telf2;
            EditEmail1 = SelectedCliente.email1;
            EditEmail2 = SelectedCliente.email2;
            EditDireccion = SelectedCliente.direccion;
            EditFechaContrato = SelectedCliente.fecha_contrato;
            IsEditPersonalInfoVisible = true;
        }


        private async void GuardarInformacion_Click(object sender, RoutedEventArgs e)
        {
            // Comprobar si hay cambios
            bool hayCambios =
                SelectedCliente.nombre != EditNombre ||
                SelectedCliente.apellido1 != EditApellido1 ||
                SelectedCliente.apellido2 != EditApellido2 ||
                SelectedCliente.email1 != EditEmail1 ||
                SelectedCliente.email2 != EditEmail2 ||
                SelectedCliente.telf1 != EditTelefono1 ||
                SelectedCliente.telf2 != EditTelefono2 ||
                SelectedCliente.direccion != EditDireccion ||
                SelectedCliente.id.ToString() != EditDNI;

            if (!hayCambios)
            {
                IsEditPersonalInfoVisible = false;
                return;
            }

            // Si hay cambios, actualiza el cliente
            SelectedCliente.nombre = EditNombre;
            SelectedCliente.apellido1 = EditApellido1;
            SelectedCliente.apellido2 = EditApellido2;
            SelectedCliente.email1 = EditEmail1;
            SelectedCliente.email2 = EditEmail2;
            SelectedCliente.telf1 = EditTelefono1;
            SelectedCliente.telf2 = EditTelefono2;
            SelectedCliente.direccion = EditDireccion;

            // Actualiza el DNI solo si es un número válido
            if (int.TryParse(EditDNI, out int nuevoId))
                SelectedCliente.id = nuevoId;

            IsEditPersonalInfoVisible = false;
            await ActualizarClienteAsync(SelectedCliente);
        }

        private void CancelarInformacion_Click(object sender, RoutedEventArgs e)
        {
            EditNombre = SelectedCliente.nombre;
            EditApellido1 = SelectedCliente.apellido1;
            EditApellido2 = SelectedCliente.apellido2;
            EditDNI = SelectedCliente.id.ToString();
            EditTelefono1 = SelectedCliente.telf1;
            EditTelefono2 = SelectedCliente.telf2;
            EditEmail1 = SelectedCliente.email1;
            EditEmail2 = SelectedCliente.email2;
            EditDireccion = SelectedCliente.direccion;
            EditFechaContrato = SelectedCliente.fecha_contrato;
            IsEditPersonalInfoVisible = false;
            DataContext = null;
            DataContext = this;
        }


        private async void CargarDocumentosCliente(int clienteId)
        {
            try
            {
                await _supabaseDocumentos.InicializarAsync();
                var documentos = await _supabaseDocumentos.ObtenerDocumentosPorClienteAsync(clienteId);
                DocumentosCliente.Clear();
                foreach (var documento in documentos)
                {
                    DocumentosCliente.Add(documento);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar documentos del cliente: {ex.Message}");
            }
        }

        #endregion

        #region Aplicar modo oscuro/claro cargado por sistema
        private void AplicarModoSistema()
        {
            var button = this.FindName("ThemeButton") as Button;
            var icon = button?.Template.FindName("ThemeIcon", button) as Image;

            if (MainWindow.isDarkTheme)
            {
                // Aplicar modo oscuro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/sol.png", UriKind.Relative));
                }
                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/TFG V0.01;component/Recursos/Background/oscuro/main.png") as ImageSource;
                navbar.ActualizarTema(true);
            }
            else
            {
                // Aplicar modo claro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna.png", UriKind.Relative));
                }
                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/TFG V0.01;component/Recursos/Background/claro/main.png") as ImageSource;
                navbar.ActualizarTema(false);
            }
        }
        #endregion

        #region modo oscuro/claro + navbar



        #endregion



        #region boton cambiar tema
        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            // Alternar el estado del tema
            MainWindow.isDarkTheme = !MainWindow.isDarkTheme;

            // Obtener el botón y el icono
            var button = sender as Button;
            var icon = button?.Template.FindName("ThemeIcon", button) as Image;

            if (MainWindow.isDarkTheme)
            {
                // Cambiar a modo oscuro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/sol.png", UriKind.Relative));
                }

                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString(
                    "pack://application:,,,/TFG V0.01;component/Recursos/Background/oscuro/main.png") as ImageSource;

                navbar.ActualizarTema(true);
            }
            else
            {
                // Cambiar a modo claro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna.png", UriKind.Relative));
                }

                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString(
                    "pack://application:,,,/TFG V0.01;component/Recursos/Background/claro/main.png") as ImageSource;

                navbar.ActualizarTema(false);
            }
        }

        #endregion

        #region Control de ventana sin bordes
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
            }
            else
            {
                this.DragMove();
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Navbar botones



        #endregion

        #region Animaciones
        private void InitializeAnimations()
        {
            // Animación de entrada con fade
            fadeInStoryboard = new Storyboard();
            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            Storyboard.SetTarget(fadeIn, this);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity"));
            fadeInStoryboard.Children.Add(fadeIn);

            // Animación de shake para error
            shakeStoryboard = new Storyboard();
            DoubleAnimation shakeAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3),
                Duration = TimeSpan.FromSeconds(0.05)
            };

            shakeStoryboard.Children.Add(shakeAnimation);
        }

        private void BeginFadeInAnimation()
        {
            this.Opacity = 0;
            fadeInStoryboard.Begin();
        }

        private void ShakeElement(FrameworkElement element)
        {
            TranslateTransform trans = new TranslateTransform();
            element.RenderTransform = trans;

            DoubleAnimation anim = new DoubleAnimation
            {
                From = 0,
                To = 5,
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3),
                Duration = TimeSpan.FromSeconds(0.05)
            };

            trans.BeginAnimation(TranslateTransform.XProperty, anim);
        }
        #endregion

        #region eventos primer dialogo
        private void clientecheck(object sender, RoutedEventArgs e)
        {
            ClientSelectorGrid.Visibility = Visibility.Collapsed;
            ClientDetailsGrid.Visibility = Visibility.Visible;
        }
        #endregion

        private async Task CargarCasosActivosAsync()
        {
            if (SelectedCliente == null)
                return;

            try
            {
                await _supabaseCasos.InicializarAsync();
                var todosLosCasos = (await _supabaseCasos.ObtenerTodosAsync())
                    .Where(c => c.id_cliente == SelectedCliente.id)
                    .ToList();

                var casosActivos = todosLosCasos
                    .Where(caso => caso.Estado != null && !string.Equals(caso.Estado.nombre, "Cerrado", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                CasosActivos = new ObservableCollection<Caso>(casosActivos);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar casos activos: {ex.Message}");
            }
        }

        private async Task CargarHistorialCasosAsync()
        {
            if (SelectedCliente == null)
                return;

            try
            {
                await _supabaseCasos.InicializarAsync();
                var todosLosCasos = (await _supabaseCasos.ObtenerTodosAsync())
                    .Where(c => c.id_cliente == SelectedCliente.id)
                    .ToList();

                HistorialCasos = new ObservableCollection<Caso>(todosLosCasos);
                OnPropertyChanged(nameof(HistorialCasos));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar historial de casos: {ex.Message}");
            }
        }

        private void VerDetalles(Caso caso)
        {
            // Asegura que la referencia de Estado sea la misma que la de la lista
            var estadoCorrecto = _estadosDisponibles.FirstOrDefault(e => e.id == caso.id_estado);
            caso.Estado = estadoCorrecto;

            var ventana = new EditarCasoWindow(caso, _estadosDisponibles);
            if (ventana.ShowDialog() == true)
            {
                // Aquí puedes guardar el caso actualizado en la base de datos
                // await _supabaseCasos.ActualizarAsync(caso);
            }
        }

        private async Task CargarEstadosAsync()
        {
            try
            {
                await _supabaseEstados.InicializarAsync();
                var estados = await _supabaseEstados.ObtenerTodosAsync();
                _estadosDisponibles.Clear();
                foreach (var estado in estados)
                {
                    _estadosDisponibles.Add(estado);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar estados: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ComboClientes_Loaded(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo != null)
            {
                _clientesView = CollectionViewSource.GetDefaultView(combo.ItemsSource);
                combo.IsTextSearchEnabled = false;
            }
        }

        private void ComboClientes_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null || _clientesView == null) return;

            var text = combo.Text?.ToLower() ?? "";
            _clientesView.Filter = item =>
            {
                var cliente = item as Cliente;
                return cliente != null && (
                    (cliente.nombre_cliente?.ToLower().Contains(text) ?? false) ||
                    (cliente.email1?.ToLower().Contains(text) ?? false)
                );
            };
            _clientesView.Refresh();
            combo.IsDropDownOpen = true;
        }

        private void ComboClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null || _clientesView == null) return;

            if (combo.SelectedItem is Cliente cliente)
            {
                TextoComboCliente = cliente.nombre_cliente;
            }
            _clientesView.Filter = null;
            _clientesView.Refresh();
        }

        private void ClearComboBox_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.TemplatedParent is ComboBox combo)
            {
                combo.SelectedItem = null;
            }
        }

        // Evento para ver documento
        private void VerDocumento_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.DataContext is Documento doc)
                {
                    var detallesWindow = new SubVentanas.DetallesDocumentoWindow(doc)
                    {
                        Owner = this
                    };
                    detallesWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir detalles: " + ex.Message);
            }
        }

        // Evento para descargar documento
        private async void DescargarDocumento_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Documento doc)
            {
                try
                {
                    var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        FileName = doc.nombre,
                        Filter = "Todos los archivos|*.*",
                        Title = "Guardar documento"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        // Verificar si el archivo existe
                        if (!File.Exists(doc.ruta))
                        {
                            MessageBox.Show("El archivo original no se encuentra disponible.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Verificar si el directorio de destino existe
                        var directory = System.IO.Path.GetDirectoryName(saveFileDialog.FileName);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        // Copiar el archivo
                        File.Copy(doc.ruta, saveFileDialog.FileName, true);
                        MessageBox.Show("Documento descargado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("No tiene permisos para guardar el archivo en la ubicación seleccionada.", "Error de permisos", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error al acceder al archivo: {ex.Message}", "Error de E/S", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al descargar el documento: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
