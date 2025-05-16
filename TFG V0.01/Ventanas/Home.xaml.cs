using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TFG_V0._01.Supabase;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TFG_V0._01.Supabase.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System;

namespace TFG_V0._01.Ventanas
{
    public partial class Home : Window, INotifyPropertyChanged
    {
        #region variables animacion
        private Storyboard fadeInStoryboard;
        private Storyboard shakeStoryboard;
        #endregion

        #region variables
        private readonly SupabaseAutentificacion _authService;
        private readonly SupabaseClientes _clientesService;
        private readonly SupabaseCasos _supabaseCasos;
        private int _clientCount;
        private int _previousClientCount;
        private string _clientCountChange;
        private int _casosActivos;
        private int _documentos;
        private int _tareasPendientes;
        private int _casosRecientes;
        public ObservableCollection<Tarea> TareasPendientesLista { get; set; } = new ObservableCollection<Tarea>();
        public ObservableCollection<string> EstadosDisponibles { get; set; } = new ObservableCollection<string> { "Pendiente", "En progreso", "Finalizado" };
        private ObservableCollection<CasoViewModel> _casosRecientesLista;
        public ObservableCollection<CasoViewModel> CasosRecientesLista
        {
            get => _casosRecientesLista;
            set
            {
                _casosRecientesLista = value;
                OnPropertyChanged();
            }
        }

        public int ClientCount
        {
            get => _clientCount;
            set
            {
                if (_clientCount != value)
                {
                    _previousClientCount = _clientCount;
                    _clientCount = value;
                    OnPropertyChanged();
                    UpdateClientCountChange();
                }
            }
        }

        public string ClientCountChange
        {
            get => _clientCountChange;
            set
            {
                if (_clientCountChange != value)
                {
                    _clientCountChange = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CasosActivos
        {
            get => _casosActivos;
            set { _casosActivos = value; OnPropertyChanged(); }
        }

        public int Documentos
        {
            get => _documentos;
            set { _documentos = value; OnPropertyChanged(); }
        }

        public int TareasPendientes
        {
            get => _tareasPendientes;
            set { _tareasPendientes = value; OnPropertyChanged(); }
        }

        public int CasosRecientes
        {
            get => _casosRecientes;
            set { _casosRecientes = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Inicializacion
        public Home()
        {
            InitializeComponent();
            DataContext = this;
            InitializeAnimations();
            AplicarModoSistema();
            _authService = new SupabaseAutentificacion();
            _clientesService = new SupabaseClientes();
            _supabaseCasos = new SupabaseCasos();
            CasosRecientesLista = new ObservableCollection<CasoViewModel>();
            
            // Inicializar valores
            ClientCount = 0;
            ClientCountChange = "+0";
            CasosActivos = 0;
            Documentos = 0;
            TareasPendientes = 0;
            CasosRecientes = 0;

            // Cargar datos después de que la ventana esté completamente inicializada
            this.Loaded += async (s, e) => await CargarDatosDashboard();
            CargarCasosRecientes();
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
                CambiarIconosAClaros();
                CambiarTextosBlanco();
                backgroun_menu.Background = new SolidColorBrush(Color.FromArgb(48, 255, 255, 255)); // Fondo semitransparente
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
                backgroun_menu.Background = new SolidColorBrush(Color.FromArgb(48, 128, 128, 128)); // Gris semitransparente

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
            var imagen = this.FindName(nombreElemento) as Image;
            if (imagen != null)
            {
                imagen.Source = new BitmapImage(new Uri(rutaIcono, UriKind.Relative));
            }
        }

        private void CambiarTextosNegro()
        {
            CambiarColorTexto("btnAgenda", Colors.Black);
            CambiarColorTexto("btnAjustes", Colors.Black);
            CambiarColorTexto("btnAyuda", Colors.Black);
            CambiarColorTexto("btnCasos", Colors.Black);
            CambiarColorTexto("btnClientes", Colors.Black);
            CambiarColorTexto("btnDocumentos", Colors.Black);
            CambiarColorTexto("btnHome", Colors.Black);
            CambiarColorTexto("btnBuscar", Colors.Black);
        }

        private void CambiarTextosBlanco()
        {
            CambiarColorTexto("btnAgenda", Colors.White);
            CambiarColorTexto("btnAjustes", Colors.White);
            CambiarColorTexto("btnAyuda", Colors.White);
            CambiarColorTexto("btnCasos", Colors.White);
            CambiarColorTexto("btnClientes", Colors.White);
            CambiarColorTexto("btnDocumentos", Colors.White);
            CambiarColorTexto("btnHome", Colors.White);
            CambiarColorTexto("btnBuscar", Colors.White);
        }

        private void CambiarColorTexto(string nombreElemento, Color color)
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
            var icon = button?.Template.FindName("ThemeIcon", button) as Image;

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
                backgroun_menu.Background = new SolidColorBrush(Color.FromArgb(48, 255, 255, 255)); // Fondo semitransparente
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
                backgroun_menu.Background = new SolidColorBrush(Color.FromArgb(48, 128, 128, 128)); // Gris semitransparente
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

        #region Cargar datos
        private async Task CargarDatosDashboard()
        {
            try
            {
                // Clientes
                await _clientesService.InicializarAsync();
                var clientes = await _clientesService.ObtenerClientesAsync();
                ClientCount = clientes?.Count ?? 0;
                _previousClientCount = 0;
                UpdateClientCountChange();

                // Casos Activos
                var casosService = new SupabaseCasos();
                await casosService.InicializarAsync();
                var casos = await casosService.ObtenerTodosAsync();
                CasosActivos = casos?.Count ?? 0;

                // Documentos
                var documentosService = new SupabaseDocumentos();
                await documentosService.InicializarAsync();
                var documentos = await documentosService.ObtenerTodosAsync();
                Documentos = documentos?.Count ?? 0;

                // Tareas Pendientes
                var tareasService = new SupabaseTareas();
                await tareasService.InicializarAsync();
                var tareas = await tareasService.ObtenerTodosAsync();
                var tareasPendientes = tareas.Where(t => t.estado != "Finalizado").ToList();
                TareasPendientes = tareasPendientes.Count;

                TareasPendientesLista.Clear();
                foreach (var tarea in tareasPendientes)
                    TareasPendientesLista.Add(tarea);

                // Casos Recientes (opcional, si quieres mostrar el número)
                var recienteService = new SupabaseReciente();
                await recienteService.InicializarAsync();
                var recientes = await recienteService.ObtenerTodosAsync();
                CasosRecientesLista.Clear();

                // Calcular la fecha límite (hoy menos 28 días)
                var fechaLimite = DateTime.Now.Date.AddDays(-28);

                foreach (var reciente in recientes)
                {
                    var caso = await _supabaseCasos.ObtenerPorIdAsync(reciente.id_caso);
                    if (caso != null && caso.fecha_inicio.Date >= fechaLimite)
                    {
                        CasosRecientesLista.Add(new CasoViewModel
                        {
                            id = caso.id,
                            referencia = caso.referencia,
                            nombre_cliente = caso.nombre_cliente,
                            tipo_nombre = caso.tipo_nombre,
                            estado = caso.estado_nombre,
                            estado_color = ObtenerColorEstadoCaso(caso.estado_nombre)
                        });
                    }
                }
                CasosRecientes = CasosRecientesLista.Count;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos del dashboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateClientCountChange()
        {
            int change = ClientCount - _previousClientCount;
            if (change > 0)
            {
                ClientCountChange = $"+{change}";
            }
            else if (change < 0)
            {
                ClientCountChange = change.ToString();
            }
            else
            {
                ClientCountChange = "+0";
            }
        }

        private async void CheckBox_TareaFinalizada(object sender, RoutedEventArgs e)
        {
            try
            {
                var checkBox = sender as CheckBox;
                if (checkBox?.DataContext is Tarea tarea)
                {
                    // Cambia el estado a Finalizado y actualiza en la base de datos
                    tarea.estado = "Finalizado";
                    var tareasService = new SupabaseTareas();
                    await tareasService.InicializarAsync();
                    await tareasService.ActualizarAsync(tarea);

                    // NO elimines la tarea de la lista aquí
                    // TareasPendientesLista.Remove(tarea);
                    // TareasPendientes = TareasPendientesLista.Count;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar la tarea: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ComboBox_EstadoChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is Tarea tarea)
            {
                var nuevoEstado = comboBox.SelectedItem as string;
                if (!string.IsNullOrEmpty(nuevoEstado) && tarea.estado != nuevoEstado)
                {
                    tarea.estado = nuevoEstado;
                    var tareasService = new SupabaseTareas();
                    await tareasService.InicializarAsync();
                    await tareasService.ActualizarAsync(tarea);

                    // Si el estado es Finalizado, remover de la lista
                    if (nuevoEstado == "Finalizado")
                    {
                        TareasPendientesLista.Remove(tarea);
                        TareasPendientes = TareasPendientesLista.Count;
                    }
                }
            }
        }

        private async void CargarCasosRecientes()
        {
            try
            {
                await _supabaseCasos.InicializarAsync();
                var casos = await _supabaseCasos.ObtenerTodosAsync();

                // Calcular la fecha límite (hoy menos 28 días)
                var fechaLimite = DateTime.Now.Date.AddDays(-28);

                // Filtrar solo los casos cuya fecha_inicio sea >= fechaLimite
                var casosRecientes = casos
                    .Where(c => c.fecha_inicio.Date >= fechaLimite)
                    .OrderByDescending(c => c.fecha_inicio)
                    .ToList();

                CasosRecientesLista.Clear();
                foreach (var caso in casosRecientes)
                {
                    CasosRecientesLista.Add(new CasoViewModel
                    {
                        id = caso.id,
                        referencia = caso.referencia,
                        nombre_cliente = caso.nombre_cliente,
                        tipo_nombre = caso.tipo_nombre,
                        estado = caso.estado_nombre,
                        estado_color = ObtenerColorEstadoCaso(caso.estado_nombre)
                    });
                }
                CasosRecientes = CasosRecientesLista.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los casos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ObtenerColorEstado(string estado)
        {
            return estado.ToLower() switch
            {
                "activo" => "#4CAF50",
                "en proceso" => "#FF9800",
                "finalizado" => "#F44336",
                _ => "#757575"
            };
        }

        private string ObtenerColorEstadoCaso(string estado)
        {
            switch (estado.ToLower())
            {
                case "abierto":
                    return "#43A047"; // Verde fuerte
                case "en proceso":
                    return "#1976D2"; // Azul intenso
                case "cerrado":
                    return "#D32F2F"; // Rojo profesional
                case "pendiente":
                    return "#FFB300"; // Ámbar
                case "revisado":
                    return "#5C6BC0"; // Azul-violeta
                default:
                    return "#BDBDBD"; // Gris claro por defecto
            }
        }

        private void VerTodosCasos_Click(object sender, RoutedEventArgs e)
        {
            // Implementar navegación a la vista completa de casos
        }

        private void EditarCaso_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var caso = (CasoViewModel)button.DataContext;
            // Implementar lógica de edición
        }

        private async void EliminarCaso_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var caso = (CasoViewModel)button.DataContext;
            
            var result = MessageBox.Show(
                $"¿Está seguro que desea eliminar el caso {caso.referencia}?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _supabaseCasos.EliminarAsync(caso.id);
                    CasosRecientesLista.Remove(caso);
                    CasosRecientes = CasosRecientesLista.Count;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar el caso: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        
    }

    public class CasoViewModel
    {
        public int id { get; set; }
        public string referencia { get; set; }
        public string nombre_cliente { get; set; }
        public string tipo_nombre { get; set; }
        public string estado { get; set; }
        public string estado_color { get; set; }
    }
}
