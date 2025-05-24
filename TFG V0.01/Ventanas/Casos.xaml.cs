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
        private List<Documento> _documentosDelCaso;
        private List<Tarea> _tareasDelCaso;
        private Documento _documentoSeleccionado;
        private Tarea _tareaSeleccionada;

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
                _casoSeleccionado = caso;
                CargarDocumentosDelCaso(caso.id);
                CargarTareasDelCaso(caso.id);
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
                // Asegurar que el calendario tenga seleccionada la fecha actual
                var calendar = this.FindName("calendar") as CalendarControl;
                if (calendar != null)
                {
                    calendar.SelectedDate = DateTime.Today;
                    _fechaSeleccionada = DateTime.Today;
                }

                // Cargar datos del caso y eventos
                await CargarDatosDelCaso(CasoSeleccionado.id);
                await CargarEventosDelDia();

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

            if (MainWindow.isDarkTheme)
            {
                // Aplicar modo oscuro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/sol.png", UriKind.Relative));
                }
                if (backgroundFondo != null)
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
                if (backgroundFondo != null)
                    backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/TFG V0.01;component/Recursos/Background/claro/main.png") as ImageSource;
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

        #region Cargar Tareas
        private async Task CargarTareasDelCaso(int casoId)
        {
            try
            {
                await _supabaseTareas.InicializarAsync();
                var tareas = await _supabaseTareas.ObtenerTareasDelCaso(casoId);
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
                        FechaInicio = e.FechaInicio
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
            var ventana = new EditarEventoWindow(estados);
            if (ventana.ShowDialog() == true)
            {
                var selectedTime = ventana.HoraMinuto ?? DateTime.Now;
                // Crear la fecha completa con la fecha seleccionada y la hora elegida
                var fechaEvento = new DateTime(
                    _fechaSeleccionada.Year,
                    _fechaSeleccionada.Month,
                    _fechaSeleccionada.Day,
                    selectedTime.Hour,
                    selectedTime.Minute,
                    0
                );
                var nuevoEvento = new EventoCita
                {
                    Titulo = ventana.TituloEvento,
                    Descripcion = ventana.DescripcionEvento,
                    Fecha = fechaEvento, // Asignar la fecha completa
                    FechaInicio = new TimeSpan(selectedTime.Hour, selectedTime.Minute, 0),
                    IdEstado = ventana.EstadoSeleccionado.Id,
                    IdCaso = CasoSeleccionado?.id ?? 0
                };
                await _eventosCitasService.InsertarEventoCita(nuevoEvento);
                await CargarEventosDelDia(); // Recargar la lista tras añadir
            }
        }

        private async void ModificarEvento_Click(object sender, RoutedEventArgs e)
        {
            if (EventosList.SelectedItem is EventoViewModel vm)
            {
                var evento = await _eventosCitasService.ObtenerEventoCita(vm.Id);
                var estados = await ObtenerEstadosEventosAsync();
                var ventana = new EditarEventoWindow(
                    estados,
                    evento.Titulo,
                    evento.Descripcion,
                    DateTime.Today.Add(evento.FechaInicio),
                    evento.IdEstado
                );
                if (ventana.ShowDialog() == true)
                {
                    evento.Titulo = ventana.TituloEvento;
                    evento.Descripcion = ventana.DescripcionEvento;

                    // Get the selected time or use current time as fallback
                    var selectedTime = ventana.HoraMinuto ?? DateTime.Now;

                    // Create a TimeSpan with just the hours and minutes
                    evento.FechaInicio = new TimeSpan(selectedTime.Hour, selectedTime.Minute, 0);

                    evento.IdEstado = ventana.EstadoSeleccionado.Id;
                    await _eventosCitasService.ActualizarEventoCita(evento);
                    await CargarEventosDelDia();
                }
            }
            else
            {
                MessageBox.Show("Selecciona un evento para modificar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
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

            var ventana = new EditarNotaWindow(_casoSeleccionado.id);
            if (ventana.ShowDialog() == true)
            {
                try
                {
                    await _notasService.InicializarAsync();
                    await CargarNotasDelCaso(_casoSeleccionado.id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al crear la nota: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ModificarNota_Click(object sender, RoutedEventArgs e)
        {
            if (_notaSeleccionada == null)
            {
                MessageBox.Show("Por favor, seleccione una nota para modificar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var ventana = new EditarNotaWindow(_casoSeleccionado.id, _notaSeleccionada);
            if (ventana.ShowDialog() == true)
            {
                try
                {
                    await _notasService.InicializarAsync();
                    await CargarNotasDelCaso(_casoSeleccionado.id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al modificar la nota: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
                    await _notasService.EliminarAsync(_notaSeleccionada.Id);
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
       
        private async void ModificarDocumento_Click(object sender, RoutedEventArgs e)
        {
            if (_documentoSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un documento para modificar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var ventana = new EditarDocumentoWindow(
                _documentoSeleccionado.nombre,
                _documentoSeleccionado.descripcion,
                _documentoSeleccionado.tipo_documento
            )
            {
                RutaArchivo = _documentoSeleccionado.ruta
            };

            if (ventana.ShowDialog() == true)
            {
                try
                {
                    _documentoSeleccionado.nombre = ventana.Nombre;
                    _documentoSeleccionado.descripcion = ventana.Descripcion;
                    _documentoSeleccionado.tipo_documento = ventana.TipoDocumento;
                    if (!string.IsNullOrEmpty(ventana.RutaArchivo))
                    {
                        _documentoSeleccionado.ruta = ventana.RutaArchivo;
                        _documentoSeleccionado.tamanio = new System.IO.FileInfo(ventana.RutaArchivo).Length.ToString();
                    }

                    await _supabaseDocumentos.ActualizarAsync(_documentoSeleccionado);
                    await CargarDocumentosDelCaso(_casoSeleccionado.id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al modificar el documento: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EliminarDocumento_Click(object sender, RoutedEventArgs e)
        {
            if (_documentoSeleccionado == null) return;

            var result = MessageBox.Show("¿Está seguro de que desea eliminar este documento?", "Confirmar eliminación",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _supabaseDocumentos.EliminarAsync(_documentoSeleccionado.id);
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
            var ventana = new EditarTareaWindow();
            if (ventana.ShowDialog() == true)
            {
                try
                {
                    var tarea = new Tarea
                    {
                        titulo = ventana.Titulo,
                        descripcion = ventana.Descripcion,
                        id_caso = _casoSeleccionado.id,
                        fecha_vencimiento = ventana.FechaVencimiento,
                        prioridad = ventana.Prioridad,
                        estado = ventana.EstadoSeleccionado
                    };

                    await _supabaseTareas.CrearTarea(tarea);
                    await CargarTareasDelCaso(_casoSeleccionado.id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al crear la tarea: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ModificarTarea_Click(object sender, RoutedEventArgs e)
        {
            if (_tareaSeleccionada == null) return;

            var ventana = new EditarTareaWindow(_tareaSeleccionada.titulo, _tareaSeleccionada.descripcion, _tareaSeleccionada.fecha_vencimiento, _tareaSeleccionada.prioridad, _tareaSeleccionada.estado);
            if (ventana.ShowDialog() == true)
            {
                try
                {
                    _tareaSeleccionada.titulo = ventana.Titulo;
                    _tareaSeleccionada.descripcion = ventana.Descripcion;
                    _tareaSeleccionada.fecha_vencimiento = ventana.FechaVencimiento;
                    _tareaSeleccionada.prioridad = ventana.Prioridad;
                    _tareaSeleccionada.estado = ventana.EstadoSeleccionado;

                    await _supabaseTareas.ActualizarTarea(_tareaSeleccionada);
                    await CargarTareasDelCaso(_casoSeleccionado.id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al modificar la tarea: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EliminarTarea_Click(object sender, RoutedEventArgs e)
        {
            if (_tareaSeleccionada == null) return;

            var result = MessageBox.Show("¿Está seguro de que desea eliminar esta tarea?", "Confirmar eliminación",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _supabaseTareas.EliminarTarea(_tareaSeleccionada.id);
                    await CargarTareasDelCaso(_casoSeleccionado.id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar la tarea: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void TareaCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Tarea tarea)
            {
                try
                {
                    await _supabaseTareas.ActualizarEstadoTarea(tarea.id, checkBox.IsChecked ?? false);
                    await CargarTareasDelCaso(_casoSeleccionado.id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al actualizar el estado de la tarea: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task CargarDocumentosDelCaso(int casoId)
        {
            try
            {
                await _supabaseDocumentos.InicializarAsync();
                var documentos = await _supabaseDocumentos.ObtenerPorCasoAsync(casoId);
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
                    FechaInicio = e.FechaInicio
                }).ToList();

                EventosList.ItemsSource = eventosConEstado;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los eventos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AgregarDocumento_Click(object sender, RoutedEventArgs e)
        {
            if (_casoSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un caso primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var ventana = new EditarDocumentoWindow();
            if (ventana.ShowDialog() == true)
            {
                try
                {
                    var documento = new Documento
                    {
                        nombre = ventana.Nombre,
                        descripcion = ventana.Descripcion,
                        tipo_documento = ventana.TipoDocumento,
                        ruta = ventana.RutaArchivo,
                        id_caso = _casoSeleccionado.id,
                        tamanio = !string.IsNullOrEmpty(ventana.RutaArchivo) ? new System.IO.FileInfo(ventana.RutaArchivo).Length.ToString() : null
                    };

                    await _supabaseDocumentos.InsertarAsync(documento);
                    await CargarDocumentosDelCaso(_casoSeleccionado.id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al crear el documento: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
