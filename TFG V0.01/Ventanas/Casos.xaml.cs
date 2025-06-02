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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using TFG_V0._01.Supabase;
using TFG_V0._01.Supabase.Models;
using TFG_V0._01.Ventanas.SubVentanas;
using System.Windows.Controls.Primitives;
using Supabase;
using Supabase.Gotrue;
using Supabase.Realtime;
using Supabase.Storage;
using Supabase.Postgrest;
using IOPath = System.IO.Path;
using CalendarControl = System.Windows.Controls.Calendar;
using TFG_V0._01.Supabase;
using TFG_V0._01.Supabase.Models;

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
        private readonly SupabaseEventosCitas _eventosCitasService;
        private readonly SupabaseEstadosEventos _estadosEventosService;
        private readonly SupabaseNotas _notasService;
        private readonly SupabaseDocumentos _supabaseDocumentos;
        private readonly SupabaseTareas _supabaseTareas;
        private ObservableCollection<Cliente> _clientes;
        private ObservableCollection<Caso> _todosLosCasos;
        private ObservableCollection<Caso> _casosFiltrados;
        private Cliente _clienteSeleccionado;
        private Caso _casoSeleccionado;
        private Nota _notaSeleccionada;
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
        private DateTime _fechaSeleccionada;
        private Dictionary<DateTime, string> DiasConEventoColor = new();
        private EventoCita eventoEditando = null;
        private bool esEdicion = false;
        private Nota notaEditando = null;
        private bool esEdicionNota = false;
        private List<Documento> _documentosDelCaso;
        private List<Tarea> _tareasDelCaso;
        private Documento _documentoSeleccionado;
        private Tarea _tareaSeleccionada;
        private Tarea tareaEditando = null;
        private bool esEdicionTarea = false;
        private bool esEdicionDocumento = false;

        // Brushes y fondo animado (añadido de Home)
        private RadialGradientBrush mesh1Brush;
        private RadialGradientBrush mesh2Brush;

        private List<TipoDocumento> _tiposDocumentoCache = new List<TipoDocumento>();

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
                if (value != null && value.id.HasValue)
                {
                    CargarCasosDelCliente(value.id.Value);
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
            CrearFondoAnimado(); // Llamar al método para crear el fondo
            AplicarModoSistema(); // Asegurarse de que el Tag se establece aquí y se actualizan colores/animación

            _clientesService = new SupabaseClientes();
            _casosService = new SupabaseCasos();
            _eventosCitasService = new SupabaseEventosCitas();
            _estadosEventosService = new SupabaseEstadosEventos();
            _notasService = new SupabaseNotas();

            Clientes = new ObservableCollection<Cliente>();
            TodosLosCasos = new ObservableCollection<Caso>();
            CasosFiltrados = new ObservableCollection<Caso>();
            _fechaSeleccionada = DateTime.Today;
            var calendar = this.FindName("calendar") as CalendarControl;
            CargarEventosDelDia();

            // Cargar datos iniciales
            CargarDatosIniciales();

            _supabaseDocumentos = new SupabaseDocumentos();
            _supabaseTareas = new SupabaseTareas();
            _documentosDelCaso = new List<Documento>();
            _tareasDelCaso = new List<Tarea>();
        }

        public Casos(int idCaso)
        {
            InitializeComponent();
            // Lógica para cargar y mostrar el caso directamente
            MostrarCasoPorId(idCaso);
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
            if (combo?.SelectedItem is Caso caso)
                combo.Text = caso.ReferenciaTituloEstado;
            else
                combo.Text = "";
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
            if (combo?.SelectedItem is Caso caso)
                combo.Text = caso.ReferenciaTituloEstado;
            else
                combo.Text = "";
            if (combo == null || _casosFiltradosView == null) return;

            if (combo.SelectedItem is Caso caso2)
            {
                _casoSeleccionado = caso2;
                CargarDocumentosDelCaso(caso2.id);
                CargarTareasDelCaso(caso2.id);
            }
            _casosFiltradosView.Filter = null;
            _casosFiltradosView.Refresh();
        }

        private void ComboTodosLosCasos_Loaded(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo?.SelectedItem is Caso caso)
                combo.Text = caso.ReferenciaTituloEstado;
            else
                combo.Text = "";
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
            if (combo?.SelectedItem is Caso caso)
                combo.Text = caso.ReferenciaTituloEstado;
            else
                combo.Text = "";
            if (combo == null || _todosLosCasosView == null) return;
            _todosLosCasosView.Filter = null;
            _todosLosCasosView.Refresh();
        }

        private async Task CargarDatosDelCaso(int idCaso)
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
                    var eventosList = this.FindName("EventosList") as System.Windows.Controls.ListView;
                    if (eventosList != null) eventosList.ItemsSource = eventos;

                    // Notas
                    await CargarNotasDelCaso(idCaso);

                    // Documentos
                    await CargarDocumentosDelCaso(idCaso);

                    // Tareas
                    await CargarTareasDelCaso(idCaso);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos del caso: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnBuscar1_Click(object sender, RoutedEventArgs e)
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
                // Iniciar animación de fade out para el buscador
                var fadeOut = (Storyboard)FindResource("FadeOutAnimation");
                fadeOut.Completed += async (s, ev) =>
                {
                    // Una vez que el fade out termina, ocultar el buscador y mostrar el contenido del caso con fade in
                    Buscador.Visibility = Visibility.Collapsed;
                    ContenidoCasos.Opacity = 0; // Establecer opacidad a 0 para la animación de fade in
                    ContenidoCasos.Visibility = Visibility.Visible;

                    var fadeIn = (Storyboard)FindResource("FadeInAnimation");
                    fadeIn.Begin(ContenidoCasos);

                     // Asegurar que el calendario tenga seleccionada la fecha actual
                    var calendar = this.FindName("calendar") as CalendarControl;
                    if (calendar != null)
                    {
                        calendar.SelectedDate = DateTime.Today;
                        _fechaSeleccionada = DateTime.Today;
                    }

                    // Cargar datos del caso y eventos (ya son async, no es necesario await extra aquí)
                    await CargarDatosDelCaso(CasoSeleccionado.id);
                    await CargarEventosDelDia();
                };
                fadeOut.Begin(Buscador);
            }
        }

        #region Aplicar modo oscuro/claro cargado por sistema
        private void AplicarModoSistema()
        {
            // Establecer el Tag de la ventana basado en el tema global
            this.Tag = MainWindow.isDarkTheme;

            var button = this.FindName("ThemeButton") as Button;
            var icon = button?.Template.FindName("ThemeIcon", button) as System.Windows.Controls.Image;

            // Actualizar colores de los brushes de malla
            if (mesh1Brush != null && mesh2Brush != null)
            {
                if (MainWindow.isDarkTheme)
                {
                    // Colores para modo oscuro
                    mesh1Brush.GradientStops.Clear();
                    mesh1Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#8C7BFF"), 0));
                    mesh1Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#08a693"), 1));

                    mesh2Brush.GradientStops.Clear();
                    mesh2Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3a4d5f"), 0));
                    mesh2Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#272c3f"), 1));
                }
                else
                {
                    // Colores para modo claro
                    mesh1Brush.GradientStops.Clear();
                    mesh1Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#de9cb8"), 0));
                    mesh1Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#9dcde1"), 1));

                    mesh2Brush.GradientStops.Clear();
                    mesh2Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#dc8eb8"), 0));
                    mesh2Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#98d3ec"), 1));
                }
                // No reiniciar la animación aquí, ya se inicia en CrearFondoAnimado y al cambiar el tema
                // IniciarAnimacionMesh();
            }

            // Actualizar icono del tema
            if (MainWindow.isDarkTheme)
            {
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/sol.png", UriKind.Relative));
                }
                 // Asegurarse de que el Navbar se actualice
                navbar.ActualizarTema(true);
            }
            else
            {
                // Aplicar modo claro (resto de elementos si los hubiera)
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna.png", UriKind.Relative));
                }
                 // Asegurarse de que el Navbar se actualice
                 navbar.ActualizarTema(false);
            }
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
            // Animación de entrada con fade de la ventana (ya existente)
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

            // Animación de shake para error (ya existente)
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

        #region 🎨 Fondo Animado (copiado y adaptado de Home)
        private void CrearFondoAnimado()
        {
            // Crear los brushes
            mesh1Brush = new RadialGradientBrush();
            mesh1Brush.Center = new Point(0.3, 0.3);
            mesh1Brush.RadiusX = 0.5;
            mesh1Brush.RadiusY = 0.5;
            mesh1Brush.GradientStops = new GradientStopCollection(); // Inicializar la colección

            mesh2Brush = new RadialGradientBrush();
            mesh2Brush.Center = new Point(0.7, 0.7);
            mesh2Brush.RadiusX = 0.6;
            mesh2Brush.RadiusY = 0.6;
            mesh2Brush.GradientStops = new GradientStopCollection(); // Inicializar la colección

            // Crear el DrawingGroup y el DrawingBrush
            var drawingGroup = new DrawingGroup();
            drawingGroup.Children.Add(new GeometryDrawing(mesh1Brush, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
            drawingGroup.Children.Add(new GeometryDrawing(mesh2Brush, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));

            var meshGradientBrush = new DrawingBrush(drawingGroup) { Stretch = Stretch.Fill };

            // Asignar el DrawingBrush al Background del Grid principal
            if (this.Content is Grid mainGrid)
            {
                mainGrid.Background = meshGradientBrush;
            }

            // Llamar a AplicarModoSistema para establecer los colores iniciales y iniciar la animación
            AplicarModoSistema();
        }

        private void IniciarAnimacionMesh()
        {
             if (mesh1Brush == null || mesh2Brush == null) return; // Asegurarse de que los brushes existan

            // Crear un nuevo Storyboard para la animación
            var meshAnimStoryboard = new Storyboard();

            // Animación para mesh1Brush Center
            var anim1 = new PointAnimation
            {
                From = mesh1Brush.Center,
                To = new Point(0.7, 0.5), // Usar mismos puntos de animación que Home
                Duration = TimeSpan.FromSeconds(8), // Duración total de la secuencia
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new System.Windows.Media.Animation.SineEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(anim1, mesh1Brush);
            Storyboard.SetTargetProperty(anim1, new PropertyPath(RadialGradientBrush.CenterProperty));
            meshAnimStoryboard.Children.Add(anim1);

            // Animación para mesh2Brush Center
            var anim2 = new PointAnimation
            {
                From = mesh2Brush.Center,
                To = new Point(0.4, 0.4), // Usar mismos puntos de animación que Home
                Duration = TimeSpan.FromSeconds(8), // Duración total de la secuencia
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new System.Windows.Media.Animation.SineEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(anim2, mesh2Brush);
            Storyboard.SetTargetProperty(anim2, new PropertyPath(RadialGradientBrush.CenterProperty));
            meshAnimStoryboard.Children.Add(anim2);

            // Iniciar la animación
            meshAnimStoryboard.Begin();
        }
        #endregion

        #region Cargar Tareas
        private async Task CargarTareasDelCaso(int casoId)
        {
            try
            {
                await _supabaseTareas.InicializarAsync();
                var tareas = await _supabaseTareas.ObtenerTareasDelCaso(casoId);
                // Inicializar la propiedad 'completada' según el estado
                foreach (var tarea in tareas)
                {
                    tarea.completada = tarea.estado == "Completada";
                }
                _tareasDelCaso = tareas;
                TareasList.ItemsSource = tareas;
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

        private async void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is CalendarControl calendar && calendar.SelectedDate.HasValue)
            {
                _fechaSeleccionada = calendar.SelectedDate.Value;
                await CargarEventosDelDia();
            }
        }

        private async Task CargarEventosDelDia()
        {
            try
            {
                if (CasoSeleccionado == null)
                {
                    return;
                }

                await _eventosCitasService.InicializarAsync();
                await _estadosEventosService.InicializarAsync();

                var eventos = await _eventosCitasService.ObtenerEventosCitasPorCaso(CasoSeleccionado.id);
                var estados = await _estadosEventosService.ObtenerEstadosEventos();

                // Actualizar el diccionario de días con evento y color
                DiasConEventoColor.Clear();
                var eventosPorDia = eventos
                    .GroupBy(e => e.Fecha.Date)
                    .ToDictionary(g => g.Key, g => ObtenerColorEstado(estados.FirstOrDefault(s => s.Id == g.First().IdEstado)?.Nombre));
                foreach (var kv in eventosPorDia)
                    DiasConEventoColor[kv.Key] = kv.Value;

                var eventosDelDia = eventos
                    .Where(e => e.Fecha.Date == _fechaSeleccionada.Date)
                    .OrderBy(e => e.Fecha)
                    .Select(e => new EventoViewModel
                    {
                        Id = e.Id,
                        Titulo = e.Titulo,
                        Descripcion = e.Descripcion,
                        Fecha = e.Fecha,
                        EstadoNombre = estados.FirstOrDefault(s => s.Id == e.IdEstado)?.Nombre ?? "Sin estado",
                        EstadoColor = ObtenerColorEstado(estados.FirstOrDefault(s => s.Id == e.IdEstado)?.Nombre),
                        FechaInicio = e.FechaInicio,
                        IdCaso = e.IdCaso
                    })
                    .ToList();

                EventosList.ItemsSource = eventosDelDia;
                var tituloEventos = this.FindName("TituloEventos") as TextBlock;
                if (tituloEventos != null)
                    tituloEventos.Text = $"Eventos del {_fechaSeleccionada:dd/MM/yyyy}";

                // Forzar refresco visual del calendario
                var calendar = this.FindName("calendar") as CalendarControl;
                if (calendar != null)
                    calendar.InvalidateVisual();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los eventos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ObtenerColorEstado(string estado)
        {
            string color = estado?.ToLower() switch
            {
                "finalizado" => "#F44336",   // Rojo
                "programado" => "#FFA726",   // Naranja
                "cancelado" => "#757575",    // Gris
                _ => "#BDBDBD"
            };
            return color;
        }

        private async void AñadirEvento_Click(object sender, RoutedEventArgs e)
        {
            var estados = await ObtenerEstadosEventosAsync();
            // Mostrar el grid deslizante en lugar de la ventana
            MostrarGridEditarEvento(null, estados);
        }

        private async void ModificarEvento_Click(object sender, RoutedEventArgs e)
        {
            if (EventosList.SelectedItem is EventoViewModel vm)
            {
                var evento = await _eventosCitasService.ObtenerEventoCita(vm.Id);
                var estados = await ObtenerEstadosEventosAsync();
                // Mostrar el grid deslizante en lugar de la ventana
                MostrarGridEditarEvento(evento, estados);
            }
            else
            {
                MessageBox.Show("Selecciona un evento para modificar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MostrarGridEditarEvento(EventoCita evento = null, List<EstadoEvento> estados = null)
        {
            // Configurar el grid
            EditarEventoGrid.Visibility = Visibility.Visible;
            OverlayPanel.Visibility = Visibility.Visible;
            cbEstadoEvento.ItemsSource = estados;

            if (evento != null)
            {
                // Modo edición
                txtTituloEvento.Text = evento.Titulo;
                txtDescripcionEvento.Text = evento.Descripcion;
                timePickerEvento.SelectedTime = DateTime.Today.Add(evento.FechaInicio);
                cbEstadoEvento.SelectedValue = evento.IdEstado;
                eventoEditando = evento;
                esEdicion = true;
            }
            else
            {
                // Modo creación
                txtTituloEvento.Text = "";
                txtDescripcionEvento.Text = "";
                timePickerEvento.SelectedTime = DateTime.Now;
                if (estados?.Count > 0)
                    cbEstadoEvento.SelectedIndex = 0;
                eventoEditando = null;
                esEdicion = false;
            }

            // Animar la entrada
            var animation = new DoubleAnimation
            {
                From = 400,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            EditarEventoTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void CerrarEditarEvento_Click(object sender, RoutedEventArgs e)
        {
            CerrarGridEditarEvento();
        }

        private void CancelarEvento_Click(object sender, RoutedEventArgs e)
        {
            CerrarGridEditarEvento();
        }

        private void OverlayPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (EditarEventoGrid.Visibility == Visibility.Visible)
            {
                CerrarGridEditarEvento();
            } 
            else if (EditarNotaGrid.Visibility == Visibility.Visible)
            {
                CerrarGridEditarNota();
            }
            else if (EditarTareaGrid.Visibility == Visibility.Visible)
            {
                CerrarGridEditarTarea();
            }
        }

        private void CerrarGridEditarEvento()
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 400,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            animation.Completed += (s, e) => 
            {
                EditarEventoGrid.Visibility = Visibility.Collapsed;
                OverlayPanel.Visibility = Visibility.Collapsed;
            };
            EditarEventoTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private async void GuardarEvento_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTituloEvento.Text))
            {
                MessageBox.Show("El título es obligatorio.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (cbEstadoEvento.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un estado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedTime = timePickerEvento.SelectedTime ?? DateTime.Now;
                var fechaEvento = new DateTime(
                    _fechaSeleccionada.Year,
                    _fechaSeleccionada.Month,
                    _fechaSeleccionada.Day,
                    selectedTime.Hour,
                    selectedTime.Minute,
                    0
                );

                if (esEdicion && eventoEditando != null)
                {
                    // Modificar evento existente
                    eventoEditando.Titulo = txtTituloEvento.Text;
                    eventoEditando.Descripcion = txtDescripcionEvento.Text;
                    eventoEditando.Fecha = fechaEvento;
                    eventoEditando.FechaInicio = new TimeSpan(selectedTime.Hour, selectedTime.Minute, 0);
                    eventoEditando.IdEstado = ((EstadoEvento)cbEstadoEvento.SelectedItem).Id;
                    await _eventosCitasService.ActualizarEventoCita(eventoEditando);
                }
                else
                {
                    // Crear nuevo evento
                    var nuevoEvento = new EventoCita
                    {
                        Titulo = txtTituloEvento.Text,
                        Descripcion = txtDescripcionEvento.Text,
                        Fecha = fechaEvento,
                        FechaInicio = new TimeSpan(selectedTime.Hour, selectedTime.Minute, 0),
                        IdEstado = ((EstadoEvento)cbEstadoEvento.SelectedItem).Id,
                        IdCaso = CasoSeleccionado?.id ?? 0
                    };
                    await _eventosCitasService.InsertarEventoCita(nuevoEvento);
                }

                await CargarEventosDelDia();
                CerrarGridEditarEvento();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el evento: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<List<EstadoEvento>> ObtenerEstadosEventosAsync()
        {
            try
            {
                return await _estadosEventosService.ObtenerEstadosEventos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener los estados de eventos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<EstadoEvento>();
            }
        }

        // Exponer el diccionario para el XAML
        public Dictionary<DateTime, string> GetDiasConEventoColor() => DiasConEventoColor;

        private async void EliminarEvento_Click(object sender, RoutedEventArgs e)
        {
            if (EventosList.SelectedItem is EventoViewModel vm)
            {
                var result = MessageBox.Show("¿Seguro que quieres eliminar este evento?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    await _eventosCitasService.EliminarEventoCita(vm.Id);
                    await CargarEventosDelDia();
                }
            }
            else
            {
                MessageBox.Show("Selecciona un evento para eliminar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #region Gestión de Notas
        private async Task CargarNotasDelCaso(int casoId)
        {
            try
            {
                await _notasService.InicializarAsync();
                var notas = await _notasService.ObtenerNotasPorCaso(casoId);
                NotasList.ItemsSource = notas;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las notas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AgregarNota_Click(object sender, RoutedEventArgs e)
        {
            if (_casoSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un caso primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Mostrar el grid deslizante en lugar de la ventana
            MostrarGridEditarNota(null);
        }

        private async void ModificarNota_Click(object sender, RoutedEventArgs e)
        {
            if (_notaSeleccionada == null)
            {
                MessageBox.Show("Por favor, seleccione una nota para modificar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Mostrar el grid deslizante en lugar de la ventana
            MostrarGridEditarNota(_notaSeleccionada);
        }

        private async void EliminarNota_Click(object sender, RoutedEventArgs e)
        {
            if (_notaSeleccionada == null)
            {
                MessageBox.Show("Por favor, seleccione una nota para eliminar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show("¿Está seguro de que desea eliminar esta nota?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _notasService.InicializarAsync();
                    await _notasService.EliminarAsync(_notaSeleccionada.Id.Value);
                    await CargarNotasDelCaso(_casoSeleccionado.id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar la nota: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void NotasList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _notaSeleccionada = NotasList.SelectedItem as Nota;
        }
        #endregion
       
        private void MostrarGridEditarNota(Nota nota = null)
        {
            if (_casoSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un caso primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Configurar el grid
            EditarNotaGrid.Visibility = Visibility.Visible;
            OverlayPanel.Visibility = Visibility.Visible;

            if (nota != null)
            {
                // Modo edición
                txtTituloNota.Text = nota.Nombre;
                txtDescripcionNota.Text = nota.Descripcion;
                notaEditando = nota;
                esEdicionNota = true;
            }
            else
            {
                // Modo creación
                txtTituloNota.Text = "";
                txtDescripcionNota.Text = "";
                notaEditando = null;
                esEdicionNota = false;
            }

            // Animar la entrada
            var animation = new DoubleAnimation
            {
                From = 400,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            EditarNotaTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void CerrarEditarNota_Click(object sender, RoutedEventArgs e)
        {
            CerrarGridEditarNota();
        }

        private void CancelarNota_Click(object sender, RoutedEventArgs e)
        {
            CerrarGridEditarNota();
        }

        private void CerrarGridEditarNota()
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 400,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            animation.Completed += (s, e) =>
            {
                EditarNotaGrid.Visibility = Visibility.Collapsed;
                OverlayPanel.Visibility = Visibility.Collapsed;
            };
            EditarNotaTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private async void GuardarNota_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTituloNota.Text))
            {
                MessageBox.Show("El título es obligatorio.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await _notasService.InicializarAsync();

                if (esEdicionNota && notaEditando != null)
                {
                    // Modificar nota existente
                    notaEditando.Nombre = txtTituloNota.Text;
                    notaEditando.Descripcion = txtDescripcionNota.Text;
                    await _notasService.ActualizarAsync(notaEditando);
                }
                else
                {
                    // Crear nueva nota
                    var nuevaNota = new NotaInsertDto
                    {
                        IdCaso = _casoSeleccionado.id,
                        Nombre = txtTituloNota.Text,
                        Descripcion = txtDescripcionNota.Text
                    };
                    await _notasService.InsertarAsync(nuevaNota);
                }

                await CargarNotasDelCaso(_casoSeleccionado.id);
                CerrarGridEditarNota();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la nota: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MostrarGridEditarTarea(Tarea tarea = null)
        {
            if (_casoSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un caso primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Configurar el grid
            EditarTareaGrid.Visibility = Visibility.Visible;
            OverlayPanel.Visibility = Visibility.Visible;

            // Configurar prioridades
            cbPrioridadTarea.ItemsSource = new[] { "Alta", "Media", "Baja" };
            cbPrioridadTarea.SelectedIndex = 1; // Media por defecto

            // Configurar estados
            cbEstadoTarea.ItemsSource = new[] { "Pendiente", "En progreso", "Completada" };
            cbEstadoTarea.SelectedIndex = 0; // Pendiente por defecto

            if (tarea != null)
            {
                // Modo edición
                txtTituloTarea.Text = tarea.titulo;
                txtDescripcionTarea.Text = tarea.descripcion;
                cbPrioridadTarea.SelectedItem = tarea.prioridad;
                dpFechaVencimientoTarea.SelectedDate = tarea.fecha_fin;
                cbEstadoTarea.SelectedItem = tarea.estado;
                tareaEditando = tarea;
                esEdicionTarea = true;
            }
            else
            {
                // Modo creación
                txtTituloTarea.Text = "";
                txtDescripcionTarea.Text = "";
                dpFechaVencimientoTarea.SelectedDate = DateTime.Now.AddDays(7); // Una semana por defecto
                tareaEditando = null;
                esEdicionTarea = false;
            }

            // Animar la entrada
            var animation = new DoubleAnimation
            {
                From = 400,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            EditarTareaTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void CerrarEditarTarea_Click(object sender, RoutedEventArgs e)
        {
            CerrarGridEditarTarea();
        }

        private void CancelarTarea_Click(object sender, RoutedEventArgs e)
        {
            CerrarGridEditarTarea();
        }

        private void CerrarGridEditarTarea()
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 400,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            animation.Completed += (s, e) =>
            {
                EditarTareaGrid.Visibility = Visibility.Collapsed;
                OverlayPanel.Visibility = Visibility.Collapsed;
            };
            EditarTareaTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private async void GuardarTarea_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTituloTarea.Text))
            {
                MessageBox.Show("El título es obligatorio.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpFechaVencimientoTarea.SelectedDate == null)
            {
                MessageBox.Show("La fecha de vencimiento es obligatoria.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (esEdicionTarea && tareaEditando != null)
                {
                    // Modificar tarea existente
                    tareaEditando.titulo = txtTituloTarea.Text;
                    tareaEditando.descripcion = txtDescripcionTarea.Text;
                    tareaEditando.prioridad = cbPrioridadTarea.SelectedItem.ToString();
                    tareaEditando.fecha_fin = dpFechaVencimientoTarea.SelectedDate.Value;
                    tareaEditando.estado = cbEstadoTarea.SelectedItem.ToString();
                    var updateDto = new TFG_V0._01.Supabase.Models.TareaUpdateDto
                    {
                        titulo = tareaEditando.titulo,
                        descripcion = tareaEditando.descripcion,
                        fecha_creacion = tareaEditando.fecha_creacion,
                        fecha_fin = tareaEditando.fecha_fin,
                        id_caso = tareaEditando.id_caso,
                        prioridad = tareaEditando.prioridad,
                        estado = tareaEditando.estado
                    };
                    await _supabaseTareas.ActualizarTarea(tareaEditando.id.Value, updateDto);
                }
                else
                {
                    // Crear nueva tarea
                    var nuevaTarea = new TFG_V0._01.Supabase.Models.TareaInsertDto
                    {
                        titulo = txtTituloTarea.Text,
                        descripcion = txtDescripcionTarea.Text,
                        prioridad = cbPrioridadTarea.SelectedItem.ToString(),
                        fecha_fin = dpFechaVencimientoTarea.SelectedDate.Value,
                        estado = cbEstadoTarea.SelectedItem.ToString(),
                        id_caso = _casoSeleccionado.id,
                        fecha_creacion = DateTime.Now
                    };
                    await _supabaseTareas.CrearTarea(nuevaTarea);
                }

                await CargarTareasDelCaso(_casoSeleccionado.id);
                CerrarGridEditarTarea();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la tarea: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MostrarGridEditarDocumento(Documento documento = null)
        {
            // Configurar el grid
            EditarDocumentoGrid.Visibility = Visibility.Visible;
            OverlayPanel.Visibility = Visibility.Visible;

            // Cargar tipos de documento dinámicamente
            await CargarTiposDocumentoAsync();

            if (documento != null)
            {
                // Modo edición
                txtNombreDocumento.Text = documento.nombre;
                cbTipoDocumento.SelectedValue = documento.tipo_documento;
                txtRutaArchivo.Text = documento.ruta;
                _documentoSeleccionado = documento;
                esEdicionDocumento = true;
            }
            else
            {
                // Modo creación
                txtNombreDocumento.Text = "";
                txtRutaArchivo.Text = "";
                cbTipoDocumento.SelectedIndex = 0;
                _documentoSeleccionado = null;
                esEdicionDocumento = false;
            }

            // Animar la entrada
            var animation = new DoubleAnimation
            {
                From = 400,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            EditarDocumentoTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private async Task CargarTiposDocumentoAsync()
        {
            try
            {
                var supabaseTiposDocumentos = new SupabaseTiposDocumentos();
                await supabaseTiposDocumentos.InicializarAsync();
                var tipos = await supabaseTiposDocumentos.ObtenerTiposDocumentos();
                cbTipoDocumento.ItemsSource = tipos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los tipos de documento: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CerrarEditarDocumento_Click(object sender, RoutedEventArgs e)
        {
            CerrarGridEditarDocumento();
        }

        private void CancelarDocumento_Click(object sender, RoutedEventArgs e)
        {
            CerrarGridEditarDocumento();
        }

        private void CerrarGridEditarDocumento()
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 400,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            animation.Completed += (s, e) =>
            {
                EditarDocumentoGrid.Visibility = Visibility.Collapsed;
                OverlayPanel.Visibility = Visibility.Collapsed;
            };
            EditarDocumentoTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private async void GuardarDocumento_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombreDocumento.Text))
            {
                MessageBox.Show("Por favor, ingrese un nombre para el documento.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtRutaArchivo.Text) || !System.IO.File.Exists(txtRutaArchivo.Text))
            {
                MessageBox.Show("Por favor, seleccione un archivo válido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1. Subir archivo a Supabase Storage con nombre único
                var storage = new TFG_V0._01.Supabase.SupaBaseStorage();
                await storage.InicializarAsync();
                string extension = System.IO.Path.GetExtension(txtRutaArchivo.Text);
                string uniqueName = $"{System.IO.Path.GetFileNameWithoutExtension(txtRutaArchivo.Text)}_{Guid.NewGuid()}{extension}";
                string storagePath = await storage.SubirArchivoAsync("documentos", txtRutaArchivo.Text, uniqueName);

                // 2. Guardar registro en la base de datos
                var documento = new TFG_V0._01.Supabase.Models.Documento.DocumentoInsertDto
                {
                    nombre = txtNombreDocumento.Text, // nombre original
                    ruta = storagePath,               // nombre/ruta en Storage
                    fecha_subid = DateTime.Now,
                    id_caso = _casoSeleccionado.id,
                    tipo_documento = (int)cbTipoDocumento.SelectedValue,
                    extension_archivo = extension
                };

                await _supabaseDocumentos.InicializarAsync();
                await _supabaseDocumentos.InsertarAsync(documento);

                await CargarDocumentosDelCaso(_casoSeleccionado.id);
                CerrarGridEditarDocumento();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el documento: {ex.Message}\n{ex.InnerException?.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                DropZone.Background = new SolidColorBrush(Color.FromArgb(50, 33, 150, 243));
            }
        }

        private void DropZone_DragLeave(object sender, DragEventArgs e)
        {
            DropZone.Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            DropZone.Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    txtRutaArchivo.Text = files[0];
                    if (string.IsNullOrEmpty(txtNombreDocumento.Text))
                    {
                        txtNombreDocumento.Text = System.IO.Path.GetFileNameWithoutExtension(files[0]);
                    }
                }
            }
        }

        private void ExaminarDocumento_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Todos los archivos|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                txtRutaArchivo.Text = openFileDialog.FileName;
                if (string.IsNullOrEmpty(txtNombreDocumento.Text))
                {
                    txtNombreDocumento.Text = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                }
            }
        }

        private void AgregarDocumento_Click(object sender, RoutedEventArgs e)
        {
            if (_casoSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un caso primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MostrarGridEditarDocumento();
        }

        private void ModificarDocumento_Click(object sender, RoutedEventArgs e)
        {
            if (_documentoSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un documento para modificar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MostrarGridEditarDocumento(_documentoSeleccionado);
        }

        private async void EliminarDocumento_Click(object sender, RoutedEventArgs e)
        {
            if (_documentoSeleccionado == null) return;

            if (!_documentoSeleccionado.id.HasValue)
            {
                MessageBox.Show("El documento no tiene un ID válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show("¿Está seguro de que desea eliminar este documento?", "Confirmar eliminación",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _supabaseDocumentos.EliminarAsync(_documentoSeleccionado.id.Value);
                    await CargarDocumentosDelCaso(_casoSeleccionado.id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar el documento: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void AgregarTarea_Click(object sender, RoutedEventArgs e)
        {
            MostrarGridEditarTarea();
        }

        private async void ModificarTarea_Click(object sender, RoutedEventArgs e)
        {
            if (_tareaSeleccionada == null)
            {
                MessageBox.Show("Por favor, seleccione una tarea para modificar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            MostrarGridEditarTarea(_tareaSeleccionada);
        }

        private async void EliminarTarea_Click(object sender, RoutedEventArgs e)
        {
            if (_tareaSeleccionada == null) return;

            if (_tareaSeleccionada.id.HasValue)
            {
                await _supabaseTareas.EliminarTarea(_tareaSeleccionada.id.Value);
                await CargarTareasDelCaso(_casoSeleccionado.id);
            }
            else
            {
                MessageBox.Show("La tarea seleccionada no tiene un ID válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarDocumentosDelCaso(int casoId)
        {
            try
            {
                await _supabaseDocumentos.InicializarAsync();
                if (_tiposDocumentoCache == null || !_tiposDocumentoCache.Any())
                    await CargarTiposDocumentoCacheAsync();
                var documentos = await _supabaseDocumentos.ObtenerPorCasoAsync(casoId);
                // Asociar el tipo de documento
                foreach (var doc in documentos)
                {
                    doc.TipoDocumento = _tiposDocumentoCache.FirstOrDefault(t => t.Id == doc.tipo_documento);
                }
                DocumentosList.ItemsSource = documentos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los documentos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DocumentosList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _documentoSeleccionado = DocumentosList.SelectedItem as Documento;
        }

        private void TareasList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _tareaSeleccionada = TareasList.SelectedItem as Tarea;
        }

        private void CasosList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo && combo.SelectedItem is Caso caso)
            {
                _casoSeleccionado = caso;
                if (_casoSeleccionado != null)
                {
                    CargarEventosDelCaso(_casoSeleccionado.id);
                    CargarNotasDelCaso(_casoSeleccionado.id);
                    CargarTareasDelCaso(_casoSeleccionado.id);
                    CargarDocumentosDelCaso(_casoSeleccionado.id);
                }
            }
        }

        private async Task CargarEventosDelCaso(int casoId)
        {
            try
            {
                await _eventosCitasService.InicializarAsync();
                var eventos = await _eventosCitasService.ObtenerEventosCitasPorCaso(casoId);
                await _estadosEventosService.InicializarAsync();
                var estados = await _estadosEventosService.ObtenerEstadosEventos();

                var eventosConEstado = eventos.Select(e => new EventoViewModel
                {
                    Id = e.Id,
                    Titulo = e.Titulo,
                    Descripcion = e.Descripcion,
                    Fecha = e.Fecha,
                    EstadoNombre = estados.FirstOrDefault(s => s.Id == e.IdEstado)?.Nombre ?? "Sin estado",
                    EstadoColor = ObtenerColorEstado(estados.FirstOrDefault(s => s.Id == e.IdEstado)?.Nombre),
                    FechaInicio = e.FechaInicio,
                    IdCaso = e.IdCaso
                }).ToList();

                EventosList.ItemsSource = eventosConEstado;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los eventos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MostrarCasoPorId(int idCaso)
        {
            try
            {
                // Cargar el caso desde Supabase
                await _casosService.InicializarAsync();
                var caso = await _casosService.ObtenerPorIdAsync(idCaso);

                if (caso != null)
                {
                    // Asignar el caso seleccionado
                    CasoSeleccionado = caso;

                    // Cargar datos relacionados
                    await CargarDatosDelCaso(caso.id);

                    // Seleccionar la fecha actual en el calendario
                    var calendar = this.FindName("calendar") as CalendarControl;
                    if (calendar != null)
                    {
                        calendar.SelectedDate = DateTime.Today;
                        _fechaSeleccionada = DateTime.Today;
                    }

                    // Mostrar el grid de detalle y ocultar el buscador
                    var contenidoCasos = this.FindName("ContenidoCasos") as UIElement;
                    var buscador = this.FindName("Buscador") as UIElement;
                    if (contenidoCasos != null) contenidoCasos.Visibility = Visibility.Visible;
                    if (buscador != null) buscador.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MessageBox.Show("No se encontró el caso solicitado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el caso: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarTiposDocumentoCacheAsync()
        {
            try
            {
                var supabaseTiposDocumentos = new SupabaseTiposDocumentos();
                await supabaseTiposDocumentos.InicializarAsync();
                _tiposDocumentoCache = await supabaseTiposDocumentos.ObtenerTiposDocumentos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el caché de tipos de documento: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void VerDocumento_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Documento doc)
            {
                try
                {
                    var storage = new TFG_V0._01.Supabase.SupaBaseStorage();
                    await storage.InicializarAsync();
                    // Descargar el archivo desde Supabase Storage usando solo el nombre del archivo
                    var fileBytes = await storage.DescargarArchivoAsync("documentos", IOPath.GetFileName(doc.ruta));
                    // Guardar en una ruta temporal
                    string tempPath = IOPath.Combine(IOPath.GetTempPath(), IOPath.GetFileName(doc.ruta));
                    await File.WriteAllBytesAsync(tempPath, fileBytes);
                    // Abrir el archivo
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = tempPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo abrir el documento: {ex.Message}");
                }
            }
        }

        private async void DescargarDocumento_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Documento doc)
            {
                try
                {
                    // Create a SaveFileDialog to let the user choose where to save the file
                    var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        FileName = doc.nombre + doc.extension_archivo,
                        DefaultExt = doc.extension_archivo,
                        Filter = $"Archivos {doc.extension_archivo}|*{doc.extension_archivo}|Todos los archivos|*.*"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        // Initialize Supabase Storage
                        var storage = new TFG_V0._01.Supabase.SupaBaseStorage();
                        await storage.InicializarAsync();

                        // Download the file from Supabase Storage usando solo el nombre del archivo
                        var fileBytes = await storage.DescargarArchivoAsync("documentos", IOPath.GetFileName(doc.ruta));

                        // Save the file to the selected location
                        await File.WriteAllBytesAsync(saveFileDialog.FileName, fileBytes);

                        MessageBox.Show("Documento descargado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al descargar el documento: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void TareaCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Tarea tarea)
            {
                try
                {
                    var updateDto = new TFG_V0._01.Supabase.Models.TareaUpdateDto
                    {
                        titulo = tarea.titulo,
                        descripcion = tarea.descripcion,
                        fecha_creacion = tarea.fecha_creacion,
                        fecha_fin = tarea.fecha_fin,
                        id_caso = tarea.id_caso,
                        prioridad = tarea.prioridad,
                        estado = tarea.estado
                    };
                    await _supabaseTareas.ActualizarTarea(tarea.id.Value, updateDto);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al actualizar el estado de la tarea: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Revertir el cambio en caso de error
                    tarea.completada = !tarea.completada;
                }
            }
        }

        private void ComboBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo != null && !combo.IsDropDownOpen)
            {
                combo.IsDropDownOpen = true;
                e.Handled = true;
            }
        }

    }

    public class EventoViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public string EstadoNombre { get; set; }
        public string EstadoColor { get; set; }
        public TimeSpan FechaInicio { get; set; }
        public int IdCaso { get; set; }
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
