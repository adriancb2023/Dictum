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
using Supabase;
using TFG_V0._01.Supabase;
using Supabase.Storage;
using System.IO;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TFG_V0._01.Supabase.Models;

namespace TFG_V0._01.Ventanas
{
    /// <summary>
    /// Lógica de interacción para Documentos.xaml
    /// </summary>
    public partial class Documentos : Window, INotifyPropertyChanged
    {
        #region variables animacion
        private Storyboard fadeInStoryboard;
        private Storyboard shakeStoryboard;
        #endregion

        #region ☁ SUPABASE
        private readonly SupaBaseStorage _supaBaseStorage;
        private readonly SupabaseClientes _supabaseClientes = new SupabaseClientes();
        private readonly SupabaseCasos _supabaseCasos = new SupabaseCasos();
        private readonly SupabaseDocumentos _supabaseDocumentos = new SupabaseDocumentos();

        public ObservableCollection<Cliente> ListaClientes { get; set; } = new ObservableCollection<Cliente>();
        public ObservableCollection<Caso> ListaCasosFiltrados { get; set; } = new ObservableCollection<Caso>();
        private Cliente _selectedCliente;
        public Cliente SelectedCliente
        {
            get => _selectedCliente;
            set
            {
                _selectedCliente = value;
                OnPropertyChanged();
                CargarCasosFiltrados();
            }
        }
        private Caso _selectedCaso;
        public Caso SelectedCaso
        {
            get => _selectedCaso;
            set { _selectedCaso = value; OnPropertyChanged(); _ = CargarArchivosPorCasoAsync(); }
        }
        private ICollectionView _clientesView;
        private ICollectionView _casosView;
        private string _textoComboCliente;
        public string TextoComboCliente
        {
            get => _textoComboCliente;
            set { _textoComboCliente = value; OnPropertyChanged(); }
        }
        private string _textoComboCaso;
        public string TextoComboCaso
        {
            get => _textoComboCaso;
            set { _textoComboCaso = value; OnPropertyChanged(); }
        }
        public ObservableCollection<Documento> ArchivosPdf { get; set; } = new ObservableCollection<Documento>();
        public ObservableCollection<Documento> ArchivosImagen { get; set; } = new ObservableCollection<Documento>();
        public ObservableCollection<Documento> ArchivosVideo { get; set; } = new ObservableCollection<Documento>();
        public ObservableCollection<Documento> ArchivosAudio { get; set; } = new ObservableCollection<Documento>();
        public ObservableCollection<Documento> ArchivosOtros { get; set; } = new ObservableCollection<Documento>();

        private async Task CargarClientesAsync()
        {
            try
            {
                await _supabaseClientes.InicializarAsync();
                var clientes = await _supabaseClientes.ObtenerClientesAsync();
                ListaClientes.Clear();
                foreach (var c in clientes)
                    ListaClientes.Add(c);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CargarCasosFiltrados()
        {
            try
            {
                ListaCasosFiltrados.Clear();
                if (SelectedCliente == null) return;
                await _supabaseCasos.InicializarAsync();
                var casos = await _supabaseCasos.ObtenerTodosAsync();
                foreach (var caso in casos.Where(c => c.id_cliente == SelectedCliente.id))
                    ListaCasosFiltrados.Add(caso);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar casos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarArchivosPorCasoAsync()
        {
            try
            {
                if (SelectedCaso == null) return;

                ArchivosPdf.Clear();
                ArchivosImagen.Clear();
                ArchivosVideo.Clear();
                ArchivosAudio.Clear();
                ArchivosOtros.Clear();

                var docs = await _supabaseDocumentos.ObtenerPorCasoAsync(SelectedCaso.id);

                foreach (var doc in docs)
                {
                    var ext = System.IO.Path.GetExtension(doc.nombre).ToLower();
                    if (ext == ".pdf")
                        ArchivosPdf.Add(doc);
                    else if (new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(ext, StringComparer.OrdinalIgnoreCase))
                        ArchivosImagen.Add(doc);
                    else if (new[] { ".mp4", ".avi", ".mov", ".wmv" }.Contains(ext, StringComparer.OrdinalIgnoreCase))
                        ArchivosVideo.Add(doc);
                    else if (new[] { ".mp3", ".wav", ".ogg", ".m4a" }.Contains(ext, StringComparer.OrdinalIgnoreCase))
                        ArchivosAudio.Add(doc);
                    else
                        ArchivosOtros.Add(doc);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar archivos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SubirArchivoYGuardar(string bucket)
        {
            try
            {
                if (SelectedCaso == null)
                {
                    MessageBox.Show("Selecciona un caso primero.");
                    return;
                }
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Todos los archivos|*.*"
                };
                if (dialog.ShowDialog() == true)
                {
                    var filePath = dialog.FileName;
                    var fileName = System.IO.Path.GetFileName(filePath);

                    // Subir a Storage
                    var ruta = await _supaBaseStorage.SubirArchivoAsync(bucket, filePath, fileName);

                    // Guardar en tabla
                    var doc = new Documento
                    {
                        id_caso = SelectedCaso.id,
                        nombre = fileName,
                        ruta = ruta,
                        fecha_subid = DateTime.Now
                    };
                    await _supabaseDocumentos.InsertarAsync(doc);

                    await CargarArchivosPorCasoAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al subir archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EliminarArchivo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as Button)?.DataContext is Documento doc)
                {
                    var ext = System.IO.Path.GetExtension(doc.nombre).ToLower();
                    string bucket = _supaBaseStorage.ObtenerCuboPorTipoArchivo(doc.nombre);

                    // Eliminar de Storage
                    await _supaBaseStorage.EliminarArchivoAsync(bucket, doc.nombre);

                    // Eliminar de tabla
                    await _supabaseDocumentos.EliminarAsync(doc.id);

                    await CargarArchivosPorCasoAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Documentos()
        {
            InitializeComponent();
            _supaBaseStorage = new SupaBaseStorage();
            InitializeAnimations();
            AplicarModoSistema();
            this.DataContext = this;
            _ = CargarClientesAsync();
        }

        #region Aplicar modo oscuro/claro cargado por sistema
        private void AplicarModoSistema()
        {
            var button = this.FindName("ThemeButton") as Button;
            var icon = button?.Template.FindName("ThemeIcon", button) as System.Windows.Controls.Image;

            if (MainWindow.isDarkTheme)
            {
                // Aplicar modo oscuro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/sol.png", UriKind.Relative));
                }
                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/TFG V0.01;component/Recursos/Background/oscuro/main.png") as ImageSource;
                CambiarIconosAClaros();
                CambiarTextosBlanco();
                backgroun_menu.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(48, 255, 255, 255)); // Fondo semitransparente
            }
            else
            {
                // Aplicar modo claro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna.png", UriKind.Relative));
                }
                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/TFG V0.01;component/Recursos/Background/claro/main.png") as ImageSource;
                CambiarIconosAOscuros();
                CambiarTextosNegro();
                backgroun_menu.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(48, 128, 128, 128)); // Gris semitransparente
            }
        }
        #endregion

        #region modo oscuro/claro + navbar
        private void CambiarIconosAOscuros()
        {
            CambiarIcono("imagenHome2", "/TFG V0.01;component/Recursos/Iconos/home.png");
            CambiarIcono("imagenDocumentos2", "/TFG V0.01;component/Recursos/Iconos/documentos.png");
            CambiarIcono("imagenClientes2", "/TFG V0.01;component/Recursos/Iconos/clientes.png");
            CambiarIcono("imagenCasos2", "/TFG V0.01;component/Recursos/Iconos/casos.png");
            CambiarIcono("imagenAyuda2", "/TFG V0.01;component/Recursos/Iconos/ayuda.png");
            CambiarIcono("imagenAgenda2", "/TFG V0.01;component/Recursos/Iconos/agenda.png");
            CambiarIcono("imagenAjustes2", "/TFG V0.01;component/Recursos/Iconos/ajustes.png");
            CambiarIcono("imagenBuscar2", "/TFG V0.01;component/Recursos/Iconos/buscar.png");
        }

        private void CambiarIconosAClaros()
        {
            CambiarIcono("imagenHome2", "/TFG V0.01;component/Recursos/Iconos/home2.png");
            CambiarIcono("imagenDocumentos2", "/TFG V0.01;component/Recursos/Iconos/documentos2.png");
            CambiarIcono("imagenClientes2", "/TFG V0.01;component/Recursos/Iconos/clientes2.png");
            CambiarIcono("imagenCasos2", "/TFG V0.01;component/Recursos/Iconos/casos2.png");
            CambiarIcono("imagenAyuda2", "/TFG V0.01;component/Recursos/Iconos/ayuda2.png");
            CambiarIcono("imagenAgenda2", "/TFG V0.01;component/Recursos/Iconos/agenda2.png");
            CambiarIcono("imagenAjustes2", "/TFG V0.01;component/Recursos/Iconos/ajustes2.png");
            CambiarIcono("imagenBuscar2", "/TFG V0.01;component/Recursos/Iconos/buscar2.png");
        }

        private void CambiarIcono(string nombreElemento, string rutaIcono)
        {
            var imagen = this.FindName(nombreElemento) as System.Windows.Controls.Image;
            if (imagen != null)
            {
                imagen.Source = new BitmapImage(new Uri(rutaIcono, UriKind.Relative));
            }
        }

        private void CambiarTextosNegro()
        {
            CambiarColorTexto("btnAgenda", System.Windows.Media.Colors.Black);
            CambiarColorTexto("btnAjustes", System.Windows.Media.Colors.Black);
            CambiarColorTexto("btnAyuda", System.Windows.Media.Colors.Black);
            CambiarColorTexto("btnCasos", System.Windows.Media.Colors.Black);
            CambiarColorTexto("btnClientes", System.Windows.Media.Colors.Black);
            CambiarColorTexto("btnDocumentos", System.Windows.Media.Colors.Black);
            CambiarColorTexto("btnHome", System.Windows.Media.Colors.Black);
            CambiarColorTexto("btnBuscar", System.Windows.Media.Colors.Black);

            CambiarColorTexto("titulo", System.Windows.Media.Colors.Black);
            CambiarColorTexto("subtitulo", System.Windows.Media.Colors.Black);
            CambiarColorTexto("selecCliente", System.Windows.Media.Colors.Black);
            CambiarColorTexto("selecCaso", System.Windows.Media.Colors.Black);
            CambiarColorTexto("documentos1", System.Windows.Media.Colors.Black);
        }

        private void CambiarTextosBlanco()
        {
            CambiarColorTexto("btnAgenda", System.Windows.Media.Colors.White);
            CambiarColorTexto("btnAjustes", System.Windows.Media.Colors.White);
            CambiarColorTexto("btnAyuda", System.Windows.Media.Colors.White);
            CambiarColorTexto("btnCasos", System.Windows.Media.Colors.White);
            CambiarColorTexto("btnClientes", System.Windows.Media.Colors.White);
            CambiarColorTexto("btnDocumentos", System.Windows.Media.Colors.White);
            CambiarColorTexto("btnHome", System.Windows.Media.Colors.White);
            CambiarColorTexto("btnBuscar", System.Windows.Media.Colors.White);

            CambiarColorTexto("titulo", System.Windows.Media.Colors.White);
            CambiarColorTexto("subtitulo", System.Windows.Media.Colors.White);
            CambiarColorTexto("selecCliente", System.Windows.Media.Colors.White);
            CambiarColorTexto("selecCaso", System.Windows.Media.Colors.White);
            CambiarColorTexto("documentos1", System.Windows.Media.Colors.White);
        }

        private void CambiarColorTexto(string nombreElemento, System.Windows.Media.Color color)
        {
            var boton = this.FindName(nombreElemento) as Button;
            if (boton != null)
            {
                boton.Foreground = new SolidColorBrush(color);
            }
        }
        #endregion

        #region navbar animacion
        private void Menu_MouseEnter(object sender, MouseEventArgs e)
        {
            inicio.Visibility = Visibility.Visible;
            buscar.Visibility = Visibility.Visible;
            documentos.Visibility = Visibility.Visible;
            clientes.Visibility = Visibility.Visible;
            casos.Visibility = Visibility.Visible;
            agenda.Visibility = Visibility.Visible;
            ajustes.Visibility = Visibility.Visible;
        }

        private void Menu_MouseLeave(object sender, MouseEventArgs e)
        {
            inicio.Visibility = Visibility.Collapsed;
            buscar.Visibility = Visibility.Collapsed;
            documentos.Visibility = Visibility.Collapsed;
            clientes.Visibility = Visibility.Collapsed;
            casos.Visibility = Visibility.Collapsed;
            agenda.Visibility = Visibility.Collapsed;
            ajustes.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region boton cambiar tema
        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            // Alternar el estado del tema
            MainWindow.isDarkTheme = !MainWindow.isDarkTheme;

            // Obtener el botón y el icono
            var button = sender as Button;
            var icon = button?.Template.FindName("ThemeIcon", button) as System.Windows.Controls.Image;

            if (MainWindow.isDarkTheme)
            {
                // Cambiar a modo oscuro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/sol.png", UriKind.Relative));
                }

                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString(
                    @"C:\Users\Harvie\Documents\TFG\V 0.1\TFG\TFG V0.01\Recursos\Background\oscuro\main.png") as ImageSource;

                CambiarIconosAClaros();
                CambiarTextosBlanco();
                backgroun_menu.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(48, 255, 255, 255)); // Fondo semitransparente
            }
            else
            {
                // Cambiar a modo claro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna.png", UriKind.Relative));
                }

                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString(
                    @"C:\Users\Harvie\Documents\TFG\V 0.1\TFG\TFG V0.01\Recursos\Background\claro\main.png") as ImageSource;

                CambiarIconosAOscuros();
                CambiarTextosNegro();
                backgroun_menu.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(48, 128, 128, 128)); // Gris semitransparente
            }
        }
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
        private void irHome(object sender, RoutedEventArgs e)
        {
            Home home = new Home();
            home.Show();
            this.Close();
        }

        private void irJurisprudencia(object sender, RoutedEventArgs e)
        {
            BusquedaJurisprudencia busquedaJurisprudencia = new BusquedaJurisprudencia();
            busquedaJurisprudencia.Show();
            this.Close();
        }

        private void irDocumentos(object sender, RoutedEventArgs e)
        {
            Documentos documentos = new Documentos();
            documentos.Show();
            this.Close();
        }

        private void irClientes(object sender, RoutedEventArgs e)
        {
            Clientes clientes = new Clientes();
            clientes.Show();
            this.Close();
        }

        private void irCasos(object sender, RoutedEventArgs e)
        {
            Casos casos = new Casos();
            casos.Show();
            this.Close();
        }

        private void irAyuda(object sender, RoutedEventArgs e)
        {
            Ayuda ayuda = new Ayuda();
            ayuda.Show();
            this.Close();
        }

        private void irAgenda(object sender, RoutedEventArgs e)
        {
            Agenda agenda = new Agenda();
            agenda.Show();
            this.Close();
        }

        private void irAjustes(object sender, RoutedEventArgs e)
        {
            Ajustes ajustes = new Ajustes();
            ajustes.Show();
            this.Close();
        }
        #endregion

        #region Drop de archivos
        private async void DopAutomatico(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    string fileBucket = _supaBaseStorage.ObtenerCuboPorTipoArchivo(file);
                    await _supaBaseStorage.SubirArchivoAsync(fileBucket, file, fileName);
                }
            }
        }
        #endregion

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
        private void ComboCasos_Loaded(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo != null)
            {
                _casosView = CollectionViewSource.GetDefaultView(combo.ItemsSource);
                combo.IsTextSearchEnabled = false;
            }
        }
        private void ComboCasos_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null || _casosView == null) return;
            var text = combo.Text?.ToLower() ?? "";
            _casosView.Filter = item =>
            {
                var caso = item as Caso;
                return caso != null && (
                    (caso.titulo?.ToLower().Contains(text) ?? false) ||
                    (caso.Estado?.nombre?.ToLower().Contains(text) ?? false)
                );
            };
            _casosView.Refresh();
            combo.IsDropDownOpen = true;
        }
        private void ComboCasos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null || _casosView == null) return;
            if (combo.SelectedItem is Caso caso)
            {
                TextoComboCaso = caso.titulo;
            }
            _casosView.Filter = null;
            _casosView.Refresh();
        }
        private void ClearComboBox_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.TemplatedParent is ComboBox combo)
            {
                combo.SelectedItem = null;
            }
        }

        private async void SubirPdf_Click(object sender, RoutedEventArgs e) => await SubirArchivoYGuardar("pdfs");
        private async void SubirImagen_Click(object sender, RoutedEventArgs e) => await SubirArchivoYGuardar("imagenes");
        private async void SubirVideo_Click(object sender, RoutedEventArgs e) => await SubirArchivoYGuardar("videos");
        private async void SubirAudio_Click(object sender, RoutedEventArgs e) => await SubirArchivoYGuardar("audios");
        private async void SubirOtro_Click(object sender, RoutedEventArgs e) => await SubirArchivoYGuardar("otros");
    }
}
