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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TFG_V0._01.Supabase;
using TFG_V0._01.Supabase.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TFG_V0._01.Ventanas
{
    /// <summary>
    /// Lógica de interacción para Casos.xaml
    /// </summary>
    public partial class Casos : Window, INotifyPropertyChanged
    {
        #region variables animacion
        private Storyboard fadeInStoryboard;
        private Storyboard shakeStoryboard;
        #endregion

        #region variables
        private readonly SupabaseClientes _clientesService;
        private readonly SupabaseCasos _casosService;
        private ObservableCollection<Cliente> _clientes;
        private ObservableCollection<Caso> _todosLosCasos;
        private ObservableCollection<Caso> _casosFiltrados;
        private Cliente _clienteSeleccionado;
        private Caso _casoSeleccionado;
        private ICollectionView _clientesView;
        private ICollectionView _casosFiltradosView;
        private ICollectionView _todosLosCasosView;
        private string _textoComboCliente;
        public string TextoComboCliente
        {
            get => _textoComboCliente;
            set { _textoComboCliente = value; OnPropertyChanged(nameof(TextoComboCliente)); }
        }
        private string _textoComboCasoFiltrado;
        public string TextoComboCasoFiltrado
        {
            get => _textoComboCasoFiltrado;
            set { _textoComboCasoFiltrado = value; OnPropertyChanged(nameof(TextoComboCasoFiltrado)); }
        }
        private string _textoComboTodosLosCasos;
        public string TextoComboTodosLosCasos
        {
            get => _textoComboTodosLosCasos;
            set { _textoComboTodosLosCasos = value; OnPropertyChanged(nameof(TextoComboTodosLosCasos)); }
        }

        public ObservableCollection<Cliente> Clientes
        {
            get => _clientes;
            set
            {
                _clientes = value;
                OnPropertyChanged(nameof(Clientes));
            }
        }

        public ObservableCollection<Caso> TodosLosCasos
        {
            get => _todosLosCasos;
            set
            {
                _todosLosCasos = value;
                OnPropertyChanged(nameof(TodosLosCasos));
            }
        }

        public ObservableCollection<Caso> CasosFiltrados
        {
            get => _casosFiltrados;
            set
            {
                _casosFiltrados = value;
                OnPropertyChanged(nameof(CasosFiltrados));
            }
        }

        public Cliente ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set
            {
                _clienteSeleccionado = value;
                OnPropertyChanged(nameof(ClienteSeleccionado));
                if (value != null)
                {
                    CargarCasosDelCliente(value.id);
                }
            }
        }

        public Caso CasoSeleccionado
        {
            get => _casoSeleccionado;
            set
            {
                _casoSeleccionado = value;
                OnPropertyChanged(nameof(CasoSeleccionado));
            }
        }
        #endregion

        public Casos()
        {
            InitializeComponent();
            this.DataContext = this;
            InitializeAnimations();
            AplicarModoSistema();
            
            _clientesService = new SupabaseClientes();
            _casosService = new SupabaseCasos();
            
            Clientes = new ObservableCollection<Cliente>();
            TodosLosCasos = new ObservableCollection<Caso>();
            CasosFiltrados = new ObservableCollection<Caso>();

            // Cargar datos iniciales
            CargarDatosIniciales();
        }

        private async void CargarDatosIniciales()
        {
            try
            {
                await _casosService.InicializarAsync();

                var clientes = await _clientesService.ObtenerClientesAsync();
                var casos = await _casosService.ObtenerTodosCasosManualAsync();
                var estados = await _casosService._estadosService.ObtenerTodosAsync();
                var tipos = await _casosService._tiposCasoService.ObtenerTodosAsync();

                Clientes.Clear();
                foreach (var cliente in clientes)
                {
                    Clientes.Add(cliente);
                }

                TodosLosCasos.Clear();
                foreach (var caso in casos)
                {
                    caso.Estado = estados.FirstOrDefault(e => e.id == caso.id_estado);
                    caso.Cliente = clientes.FirstOrDefault(c => c.id == caso.id_cliente);
                    caso.TipoCaso = tipos.FirstOrDefault(t => t.id == caso.id_tipo_caso);
                    TodosLosCasos.Add(caso);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos iniciales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CargarCasosDelCliente(int idCliente)
        {
            try
            {
                await _casosService.InicializarAsync();
                var casos = await _casosService.ObtenerTodosCasosManualAsync();
                var estados = await _casosService._estadosService.ObtenerTodosAsync();
                var clientes = await _clientesService.ObtenerClientesAsync();
                var tipos = await _casosService._tiposCasoService.ObtenerTodosAsync();
                var casosFiltrados = casos.Where(c => c.id_cliente == idCliente).ToList();

                CasosFiltrados.Clear();
                foreach (var caso in casosFiltrados)
                {
                    caso.Estado = estados.FirstOrDefault(e => e.id == caso.id_estado);
                    caso.Cliente = clientes.FirstOrDefault(c => c.id == caso.id_cliente);
                    caso.TipoCaso = tipos.FirstOrDefault(t => t.id == caso.id_tipo_caso);
                    CasosFiltrados.Add(caso);
                }
                _casosFiltradosView = CollectionViewSource.GetDefaultView(CasosFiltrados);
                var combo = this.FindName("comboCasosFiltrados") as ComboBox;
                if (combo != null)
                {
                    combo.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar casos del cliente: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void ComboCasosFiltrados_Loaded(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo != null)
            {
                combo.IsTextSearchEnabled = false;
                
            }
        }
        private void ComboCasosFiltrados_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null || _casosFiltradosView == null) return;

            var text = combo.Text?.ToLower() ?? "";
            _casosFiltradosView.Filter = item =>
            {
                var caso = item as Caso;
                return caso != null && (
                    (caso.titulo?.ToLower().Contains(text) ?? false) ||
                    (caso.referencia?.ToLower().Contains(text) ?? false) ||
                    (caso.Estado?.nombre?.ToLower().Contains(text) ?? false)
                );
            };
            _casosFiltradosView.Refresh();
            combo.IsDropDownOpen = true;
        }
        private void ComboCasosFiltrados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null || _casosFiltradosView == null) return;

            if (combo.SelectedItem is Caso caso)
            {
                combo.Text = caso.titulo;
            }
            _casosFiltradosView.Filter = null;
            _casosFiltradosView.Refresh();
        }

        private void ComboTodosLosCasos_Loaded(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo != null)
            {
                _todosLosCasosView = CollectionViewSource.GetDefaultView(combo.ItemsSource);
                combo.IsTextSearchEnabled = false;
            }
        }
        private void ComboTodosLosCasos_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null || _todosLosCasosView == null) return;

            var text = combo.Text?.ToLower() ?? "";
            _todosLosCasosView.Filter = item =>
            {
                var caso = item as Caso;
                return caso != null && (
                    (caso.titulo?.ToLower().Contains(text) ?? false) ||
                    (caso.referencia?.ToLower().Contains(text) ?? false) ||
                    (caso.Estado?.nombre?.ToLower().Contains(text) ?? false)
                );
            };
            _todosLosCasosView.Refresh();
            combo.IsDropDownOpen = true;
        }
        private void ComboTodosLosCasos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null || _todosLosCasosView == null) return;

            if (combo.SelectedItem is Caso caso)
            {
                TextoComboTodosLosCasos = $"{caso.referencia} - {caso.titulo} - {caso.Estado?.nombre}";
            }
            _todosLosCasosView.Filter = null;
            _todosLosCasosView.Refresh();
        }

        private void BtnBuscar1_Click(object sender, RoutedEventArgs e)
        {
            var comboClientes = this.FindName("comboClientes") as ComboBox;
            var comboCasosFiltrados = this.FindName("comboCasosFiltrados") as ComboBox;
            var comboTodosLosCasos = this.FindName("comboTodosLosCasos") as ComboBox;

            bool clienteSeleccionado = comboClientes?.SelectedItem != null;
            bool casoFiltradoSeleccionado = comboCasosFiltrados?.SelectedItem != null;
            bool casoDirectoSeleccionado = comboTodosLosCasos?.SelectedItem != null;

            if ((clienteSeleccionado && casoFiltradoSeleccionado && casoDirectoSeleccionado) ||
                ((clienteSeleccionado || casoFiltradoSeleccionado) && casoDirectoSeleccionado))
            {
                MessageBox.Show("Solo puedes buscar por cliente y caso, o directamente por caso. No selecciones ambos métodos a la vez.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (clienteSeleccionado && casoFiltradoSeleccionado)
            {
                CasoSeleccionado = (Caso)comboCasosFiltrados.SelectedItem;
            }
            else if (casoDirectoSeleccionado)
            {
                CasoSeleccionado = (Caso)comboTodosLosCasos.SelectedItem;
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un caso para buscar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (CasoSeleccionado != null)
            {
                CargarDatosDelCaso(CasoSeleccionado.id);
                var contenidoCasos = this.FindName("ContenidoCasos") as UIElement;
                var buscador = this.FindName("Buscador") as UIElement;
                if (contenidoCasos != null) contenidoCasos.Visibility = Visibility.Visible;
                if (buscador != null) buscador.Visibility = Visibility.Collapsed;
            }
        }

        #region Aplicar modo oscuro/claro cargado por sistema
        private void AplicarModoSistema()
        {
            var button = this.FindName("ThemeButton") as Button;
            var icon = button?.Template.FindName("ThemeIcon", button) as System.Windows.Controls.Image;
            var backgroundFondo = this.FindName("backgroundFondo") as ImageBrush;
            var backgroun_menu = this.FindName("backgroun_menu") as Border;

            if (MainWindow.isDarkTheme)
            {
                // Aplicar modo oscuro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/sol.png", UriKind.Relative));
                }
                if (backgroundFondo != null)
                    backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/TFG V0.01;component/Recursos/Background/oscuro/main.png") as ImageSource;
                CambiarIconosAClaros();
                CambiarTextosBlanco();
                if (backgroun_menu != null)
                    backgroun_menu.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(48, 255, 255, 255)); // Fondo semitransparente
            }
            else
            {
                // Aplicar modo claro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna.png", UriKind.Relative));
                }
                if (backgroundFondo != null)
                    backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/TFG V0.01;component/Recursos/Background/claro/main.png") as ImageSource;
                CambiarIconosAOscuros();
                CambiarTextosNegro();
                if (backgroun_menu != null)
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
            var inicio = this.FindName("inicio") as UIElement;
            var buscar = this.FindName("buscar") as UIElement;
            var documentos = this.FindName("documentos") as UIElement;
            var clientes = this.FindName("clientes") as UIElement;
            var casos = this.FindName("casos") as UIElement;
            var agenda = this.FindName("agenda") as UIElement;
            var ajustes = this.FindName("ajustes") as UIElement;
            if (inicio != null) inicio.Visibility = Visibility.Visible;
            if (buscar != null) buscar.Visibility = Visibility.Visible;
            if (documentos != null) documentos.Visibility = Visibility.Visible;
            if (clientes != null) clientes.Visibility = Visibility.Visible;
            if (casos != null) casos.Visibility = Visibility.Visible;
            if (agenda != null) agenda.Visibility = Visibility.Visible;
            if (ajustes != null) ajustes.Visibility = Visibility.Visible;
        }

        private void Menu_MouseLeave(object sender, MouseEventArgs e)
        {
            var inicio = this.FindName("inicio") as UIElement;
            var buscar = this.FindName("buscar") as UIElement;
            var documentos = this.FindName("documentos") as UIElement;
            var clientes = this.FindName("clientes") as UIElement;
            var casos = this.FindName("casos") as UIElement;
            var agenda = this.FindName("agenda") as UIElement;
            var ajustes = this.FindName("ajustes") as UIElement;
            if (inicio != null) inicio.Visibility = Visibility.Collapsed;
            if (buscar != null) buscar.Visibility = Visibility.Collapsed;
            if (documentos != null) documentos.Visibility = Visibility.Collapsed;
            if (clientes != null) clientes.Visibility = Visibility.Collapsed;
            if (casos != null) casos.Visibility = Visibility.Collapsed;
            if (agenda != null) agenda.Visibility = Visibility.Collapsed;
            if (ajustes != null) ajustes.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region boton cambiar tema
        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.isDarkTheme = !MainWindow.isDarkTheme;

            AplicarModoSistema();
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

        #region Rellenar Ventana CASOS
        private async void CargarDatosDelCaso(int idCaso)
        {
            try
            {
                await _casosService.InicializarAsync();
                var caso = await _casosService.ObtenerPorIdAsync(idCaso);

                if (caso != null)
                {
                    // Eventos y citas
                    var colores = new List<string>
                    {
                        "#FF5722", "#F4511E", "#E64A19", "#D84315",
                        "#009688", "#26A69A", "#00796B", "#004D40",
                        "#3F51B5", "#5C6BC0", "#3949AB", "#1A237E"
                    };

                    var eventos = new List<object>();
                    if (caso.Alertas != null)
                    {
                        eventos = caso.Alertas
                            .Select((a, i) => new
                            {
                                Titulo = a.titulo ?? "",
                                FechaDia = a.fecha_alerta != null ? a.fecha_alerta.Day.ToString() : "",
                                Descripcion = a.descripcion ?? "",
                                Estado = a.estado_alerta != null ? a.estado_alerta.ToString() : "",
                                Color = colores[i % colores.Count]
                            })
                            .Cast<object>()
                            .ToList();
                    }
                    var eventosList = this.FindName("EventosList") as ListView;
                    if (eventosList != null) eventosList.ItemsSource = eventos;

                    // Notas (usando la descripción del caso)
                    var notas = new List<string> { caso.descripcion };
                    var notasList = this.FindName("NotasList") as ListBox;
                    if (notasList != null) notasList.ItemsSource = notas;

                    // Documentos
                    var documentos = caso.Documentos?
                        .Select(d => d.nombre)
                        .ToList() ?? new List<string>();
                    var documentosList = this.FindName("DocumentosList") as ListBox;
                    if (documentosList != null) documentosList.ItemsSource = documentos;

                    // Tareas
                    await CargarTareasDelCaso(idCaso);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos del caso: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Cargar Tareas
        private async Task CargarTareasDelCaso(int idCaso)
        {
            try
            {
                await _casosService.InicializarAsync();
                var caso = await _casosService.ObtenerPorIdAsync(idCaso);
                var tareas = caso.Tareas?.OrderBy(t => t.fecha_fin).ToList() ?? new List<Tarea>();

                var tareasList = this.FindName("TareasList") as ListBox;
                if (tareasList != null)
                {
                    tareasList.Items.Clear();

                    foreach (var tarea in tareas)
                    {
                        Border border = new Border
                        {
                            Background = new SolidColorBrush(Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF)),
                            CornerRadius = new CornerRadius(10),
                            Padding = new Thickness(10),
                            Margin = new Thickness(0, 0, 0, 10),
                            HorizontalAlignment = HorizontalAlignment.Stretch
                        };

                        Grid grid = new Grid();
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                        CheckBox checkBox = new CheckBox
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(0, 0, 10, 0),
                            IsChecked = tarea.estado == "finalizado"
                        };
                        checkBox.Checked += async (s, e) =>
                        {
                            try
                            {
                                tarea.estado = "finalizado";
                                await _casosService.ActualizarAsync(caso);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error al actualizar la tarea: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        };
                        Grid.SetColumn(checkBox, 0);

                        StackPanel stack = new StackPanel();
                        Grid.SetColumn(stack, 1);

                        TextBlock titulo = new TextBlock
                        {
                            Text = tarea.titulo,
                            Foreground = Brushes.White,
                            TextWrapping = TextWrapping.Wrap,
                            FontSize = 16,
                            FontWeight = FontWeights.SemiBold
                        };

                        int diasRestantes = (tarea.fecha_fin - DateTime.Today).Days;
                        string vencimientoTexto = diasRestantes == 0 ? "Vence hoy" : $"Vence en {diasRestantes} días";

                        Brush color;
                        if (diasRestantes == 0)
                            color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5252")); // Rojo
                        else if (diasRestantes <= 2)
                            color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107")); // Amarillo
                        else
                            color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8BC34A")); // Verde

                        TextBlock fecha = new TextBlock
                        {
                            Text = vencimientoTexto,
                            Foreground = color,
                            FontSize = 12,
                            Margin = new Thickness(0, 5, 0, 0)
                        };

                        stack.Children.Add(titulo);
                        stack.Children.Add(fecha);

                        grid.Children.Add(checkBox);
                        grid.Children.Add(stack);

                        border.Child = grid;
                        tareasList.Items.Add(border);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las tareas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LimpiarComboClientes_Click(object sender, RoutedEventArgs e)
        {
            var comboClientes = this.FindName("comboClientes") as ComboBox;
            if (comboClientes != null) comboClientes.SelectedItem = null;
        }

        private void LimpiarComboCasosFiltrados_Click(object sender, RoutedEventArgs e)
        {
            var comboCasosFiltrados = this.FindName("comboCasosFiltrados") as ComboBox;
            if (comboCasosFiltrados != null) comboCasosFiltrados.SelectedItem = null;
        }

        private void LimpiarComboTodosLosCasos_Click(object sender, RoutedEventArgs e)
        {
            var comboTodosLosCasos = this.FindName("comboTodosLosCasos") as ComboBox;
            if (comboTodosLosCasos != null) comboTodosLosCasos.SelectedItem = null;
        }

        private void ClearComboBox_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.TemplatedParent is ComboBox combo)
            {
                combo.SelectedItem = null;
            }
        }
    }

    // Converter para usar en XAML
    public class NullToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value == null ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }
        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
