using Microsoft.EntityFrameworkCore;
using Polly;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TFG_V0._01.Helpers;
using TFG_V0._01.Supabase;
using TFG_V0._01.Supabase.Models;
using TFG_V0._01.Ventanas.SubVentanas;
using SupabaseCaso = TFG_V0._01.Supabase.Models.Caso;
using SupabaseTarea = TFG_V0._01.Supabase.Models.Tarea;

namespace TFG_V0._01.Ventanas
{
    public partial class Home : Window, INotifyPropertyChanged
    {
        #region 🎬 variables animacion
        private Storyboard fadeInStoryboard;
        private Storyboard shakeStoryboard;
        #endregion

        #region 📊 variables
        private readonly SupabaseAutentificacion _authService;

        private readonly SupabaseClientes _clientesService;

        private readonly SupabaseCasos _supabaseCasos;

        private readonly SupabaseEventosCitas _eventosCitasService;

        private readonly SupabaseDocumentos _documentosService;

        private readonly SupabaseTareas _tareasService;

        private readonly SupabaseReciente _recienteService;

        public ICommand VerDetallesCommand { get; }

        private int _clientCount;

        private int _previousClientCount;

        private string _clientCountChange;

        private int _casosActivos;

        private int _documentos;

        private int _tareasPendientes;

        private int _casosRecientes;

        private ObservableCollection<Estado> _estadosDisponibles = new ObservableCollection<Estado>();

        public ObservableCollection<SupabaseTarea> TareasPendientesLista { get; set; } = new ObservableCollection<SupabaseTarea>();

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

        private DateOnly fechaActual = DateOnly.FromDateTime(DateTime.Now);

        private string mesText;

        private string anio;

        private int _eventosProximos;

        public ObservableCollection<EventoCita> EventosProximosLista { get; set; } = new ObservableCollection<EventoCita>();

        public int EventosProximos
        {
            get => _eventosProximos;
            set { _eventosProximos = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ⚡ Inicializacion
        public Home()
        {
            InitializeComponent();
            DataContext = this;

            CargarIdioma(MainWindow.idioma);

            diaSemana();

            InitializeAnimations();

            AplicarModoSistema();

            _authService = new SupabaseAutentificacion();
            _clientesService = new SupabaseClientes();
            _supabaseCasos = new SupabaseCasos();
            _documentosService = new SupabaseDocumentos();
            _tareasService = new SupabaseTareas();
            _recienteService = new SupabaseReciente();
            _eventosCitasService = new SupabaseEventosCitas();
            CasosRecientesLista = new ObservableCollection<CasoViewModel>();

            // Inicializar valores
            ClientCount = 0;
            ClientCountChange = "+0";
            CasosActivos = 0;
            Documentos = 0;
            TareasPendientes = 0;
            CasosRecientes = 0;
            mesText = string.Empty;
            anio = string.Empty;



            _eventosCitasService = new SupabaseEventosCitas();
            EventosProximos = 0;

            // Cargar datos después de que la ventana esté completamente inicializada
            //this.Loaded += async (s, e) => { await CargarDatosDashboard(); CargarScoreCasos(); CargarCasosRecientes(); };
        }
        #endregion

        #region ⌛ Patalla de carga
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (MainWindow.tipoBBDD)
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
                await CargarDatosDashboard();
                CargarScoreCasos();
                CargarCasosRecientes();

                CargarScoreDocumentos();

                LoadingPanel.Visibility = Visibility.Collapsed;
            }
            else
            {

            }
        }
        #endregion

        #region 🌓 Aplicar modo oscuro/claro cargado por sistema
        private void AplicarModoSistema()
        {
            var button = FindName("ThemeButton") as Button;
            var icon = button?.Template.FindName("ThemeIcon", button) as Image;

            if (icon != null)
                icon.Source = new BitmapImage(new Uri(GetIconoTema(), UriKind.Relative));

            backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString(GetBackgroundPath()) as ImageSource;

            navbar.ActualizarTema(MainWindow.isDarkTheme);
        }

        private string GetIconoTema() =>
            MainWindow.isDarkTheme
                ? "/TFG V0.01;component/Recursos/Iconos/sol.png"
                : "/TFG V0.01;component/Recursos/Iconos/luna.png";

        private string GetBackgroundPath() =>
            MainWindow.isDarkTheme
                ? "pack://application:,,,/TFG V0.01;component/Recursos/Background/oscuro/main.png"
                : "pack://application:,,,/TFG V0.01;component/Recursos/Background/claro/main.png";

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.isDarkTheme = !MainWindow.isDarkTheme;
            AplicarModoSistema();
            var fadeAnimation = CrearFadeAnimation(0.7, 0.9, 0.3, true);
            backgroundFondo.BeginAnimation(OpacityProperty, fadeAnimation);
        }
        #endregion

        #region 🎬  Animaciones

        private void InitializeAnimations()
        {
            fadeInStoryboard = CrearStoryboard(this, OpacityProperty, CrearFadeAnimation(0, 1, 0.5));
            shakeStoryboard = CrearStoryboard(null, TranslateTransform.XProperty, CrearShakeAnimation());
        }

        private Storyboard CrearStoryboard(DependencyObject target, DependencyProperty property, DoubleAnimation animation)
        {
            var storyboard = new Storyboard();
            if (target != null && property != null)
            {
                Storyboard.SetTarget(animation, target);
                Storyboard.SetTargetProperty(animation, new PropertyPath(property));
            }
            storyboard.Children.Add(animation);
            return storyboard;
        }

        private DoubleAnimation CrearFadeAnimation(double from, double to, double durationSeconds, bool autoReverse = false) =>
            new()
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                AutoReverse = autoReverse
            };

        private DoubleAnimation CrearShakeAnimation() =>
            new()
            {
                From = 0,
                To = 5,
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3),
                Duration = TimeSpan.FromSeconds(0.05)
            };

        private void BeginFadeInAnimation()
        {
            this.Opacity = 0;
            fadeInStoryboard.Begin();
        }

        private void ShakeElement(FrameworkElement element)
        {
            var trans = new TranslateTransform();
            element.RenderTransform = trans;
            trans.BeginAnimation(TranslateTransform.XProperty, CrearShakeAnimation());
        }
        #endregion

        #region 📅 Dia Actual
        private void diaSemana()
        {
            var diaSemana = fechaActual.DayOfWeek;
            switch (diaSemana)
            {
                case DayOfWeek.Monday:
                    lunes.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case DayOfWeek.Tuesday:
                    martes.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case DayOfWeek.Wednesday:
                    miercoles.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case DayOfWeek.Thursday:
                    jueves.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case DayOfWeek.Friday:
                    viernes.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case DayOfWeek.Saturday:
                    sabado.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case DayOfWeek.Sunday:
                    domingo.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 📅 Cambiar de fecha Calendario
        private static readonly string[] NombresMeses = { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

        private void CambiarMesCalendario(int incremento)
        {
            int nuevoMes = fechaActual.Month + incremento;
            int nuevoAnio = fechaActual.Year;

            if (nuevoMes < 1)
            {
                nuevoMes = 12;
                nuevoAnio--;
            }
            else if (nuevoMes > 12)
            {
                nuevoMes = 1;
                nuevoAnio++;
            }

            fechaActual = new DateOnly(nuevoAnio, nuevoMes, 1);
            ActualizarTextoFechaElegida();
        }

        private void mesAnterior(object sender, RoutedEventArgs e)
        {
            CambiarMesCalendario(-1);
        }

        private void mesSiguiente(object sender, RoutedEventArgs e)
        {
            CambiarMesCalendario(1);
        }

        private void mesActual(object sender, RoutedEventArgs e)
        {
            var fechaHoy = DateOnly.FromDateTime(DateTime.Now);

            if (fechaActual.Equals(fechaHoy))
            {
                if (sender is Button btn)
                    ShakeElement(btn);
            }
            else
            {
                fechaActual = fechaHoy;
                ActualizarTextoFechaElegida();
            }
        }

        private void ActualizarTextoFechaElegida()
        {
            mesText = NombresMeses[fechaActual.Month - 1];
            anio = fechaActual.Year.ToString();
            FechaElegida.Text = $"{mesText} {anio}";

            //actualizar lista en local 
            if (MainWindow.tipoBBDD)
            {

            }
            else
            {
            }
        }
        #endregion

        #region 🈳 Idioma
        private void CargarIdioma(int idioma)
        {
            navbar.ActualizarIdioma(idioma);

            var idiomas = new (string Titulo, string Subtitulo, string ResumenCasos, string ResumenClientes, string ResumenDocumentos, string ResumenEventos,
                string Lunes, string Martes, string Miercoles, string Jueves, string Viernes, string Sabado, string Domingo,
                string ListaTareas, string BtnAñadirTarea, string BtnVerTodosCasos, string CasosRecientes, string NCasos, string CCliente, string CTipo, string CEstado, string CAcciones, string Version, string Hoy)[]
            {
                ("Panel de control.", "Bienvenido a la aplicación de gestión de casos. Se encuentra en el Dashboard de la aplicacion.",
                 "Casos Activos:", "Clientes:", "Documentos:", "Eventos Póximos:",
                 "Lun", "Mar", "Mie", "Jue", "Vie", "Sab", "Dom",
                 "Tareas Pendientes", "Añadir Tarea", "Ver todos los casos", "Casos Recientes", "Nº Caso", "Cliente", "Tipo", "Estado", "Acciones", "Versión: ", "Hoy"),
                ("Dashboard", "Welcome to the case management application. You are on the application's dashboard.",
                 "Active Cases:", "Clients:", "Documents:", "Upcoming Events:",
                 "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun",
                 "Pending Tasks", "Add Task", "View All Cases", "Recent Cases", "Case No.", "Client", "Type", "Status", "Actions", "Version: ", "Today"),
                ("Panell de control", "Benvingut a l'aplicació de gestió de casos. Estàs al panell de l'aplicació.",
                 "Casos Actius:", "Clients:", "Documents:", "Esdeveniments propers:",
                 "Dll", "Dt", "Dc", "Dj", "Dv", "Ds", "Dg",
                 "Tasques pendents", "Afegir tasca", "Veure tots els casos", "Casos recents", "Nº Cas", "Client", "Tipus", "Estat", "Accions", "Versió: ", "Avui"),
                ("Panel de control", "Benvido á aplicación de xestión de casos. Estás no panel da aplicación.",
                 "Casos activos:", "Clientes:", "Documentos:", "Eventos próximos:",
                 "Lun", "Mar", "Mér", "Xov", "Ven", "Sáb", "Dom",
                 "Tarefas pendentes", "Engadir tarefa", "Ver todos os casos", "Casos recentes", "Nº Caso", "Cliente", "Tipo", "Estado", "Accións", "Versión: ", "Hoxe"),
                ("Kontrol panela", "Ongi etorri kasuen kudeaketa aplikaziora. Aplikazioaren panel nagusian zaude.",
                 "Kasuan aktiboak:", "Bezeroak:", "Dokumentuak:", "Hurrengo ekitaldiak:",
                 "Al", "Ar", "Az", "Og", "Or", "La", "Ig",
                 "Zain dauden zereginak", "Zeregina gehitu", "Kasu guztiak ikusi", "Azken kasuak", "Kasua Nº", "Bezeroa", "Mota", "Egoera", "Ekintzak", "Bertsioa: ", "Gaur")
            };

            if (idioma < 0 || idioma >= idiomas.Length)
                idioma = 0;

            var t = idiomas[idioma];

            titulo.Text = t.Titulo;
            subtitulo.Text = t.Subtitulo;
            resumenCasos.Text = t.ResumenCasos;
            resumenClientes.Text = t.ResumenClientes;
            resumenDocumentos.Text = t.ResumenDocumentos;
            resumenEventos.Text = t.ResumenEventos;
            lunes.Text = t.Lunes;
            martes.Text = t.Martes;
            miercoles.Text = t.Miercoles;
            jueves.Text = t.Jueves;
            viernes.Text = t.Viernes;
            sabado.Text = t.Sabado;
            domingo.Text = t.Domingo;
            listaTareas.Text = t.ListaTareas;
            btnAñadirTarea.Content = t.BtnAñadirTarea;
            btnVerTodosCasos.Content = t.BtnVerTodosCasos;
            casosRecientes.Text = t.CasosRecientes;
            ncasos.Text = t.NCasos;
            Ccliente.Text = t.CCliente;
            Ctipo.Text = t.CTipo;
            Cestado.Text = t.CEstado;
            Cacciones.Text = t.CAcciones;
            Version.Text = t.Version;
            hoy.Text = t.Hoy;
        }
        #endregion

        #region ☁ SUPABASE
        private async Task InicializarServiciosSupabase()
        {
            try
            {
                await Task.WhenAll(
                    _documentosService.InicializarAsync(),
                    _tareasService.InicializarAsync(),
                    _recienteService.InicializarAsync(),
                    _eventosCitasService.InicializarAsync()
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar servicios de Supabase: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarDatosDashboard()
        {
            try
            {
                await InicializarServiciosSupabase();

                // Usar los métodos manuales para traer todos los registros
                var clientesTask = _clientesService.ObtenerTodosClientesManualAsync();
                var casosTask = _supabaseCasos.ObtenerTodosAsync();
                var documentosTask = _documentosService.ObtenerTodosDocumentosManualAsync();
                var tareasTask = _tareasService.ObtenerTodosAsync();
                var recientesTask = _recienteService.ObtenerTodosAsync();
                var eventosCitasTask = _eventosCitasService.ObtenerTodosEventosManualAsync();

                // Esperar a que todas las tareas se completen
                await Task.WhenAll(clientesTask, casosTask, documentosTask, tareasTask, recientesTask, eventosCitasTask);

                // Procesar clientes
                var clientes = await clientesTask;
                ClientCount = clientes?.Count ?? 0;
                _previousClientCount = 0;
                UpdateClientCountChange();

                // Procesar casos
                var casos = await casosTask;
                CasosActivos = casos?.Count ?? 0;

                // Procesar documentos (sin límite)
                var documentos = await documentosTask;
                Documentos = documentos?.Count ?? 0;

                // Procesar tareas
                var tareas = await tareasTask;
                var tareasPendientes = tareas.Where(t => t.estado != "Finalizado").ToList();
                TareasPendientes = tareasPendientes.Count();

                TareasPendientesLista.Clear();
                foreach (var tarea in tareasPendientes)
                    TareasPendientesLista.Add(tarea);

                // Procesar casos recientes
                var recientes = await recientesTask;
                CasosRecientesLista.Clear();
                var fechaLimite = DateTime.Now.Date.AddDays(-28);

                // Cargar casos recientes en paralelo
                var casosRecientes = await Task.WhenAll(
                    recientes.Select(async reciente =>
                    {
                        var caso = await _supabaseCasos.ObtenerPorIdAsync(reciente.id_caso);
                        return (caso, reciente);
                    })
                );

                foreach (var (caso, _) in casosRecientes)
                {
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

                // Procesar eventos del día actual (comparando el string de la fecha)
                var eventosCitas = await eventosCitasTask;
                var hoyStr = DateTime.Now.ToString("yyyy-MM-dd");
                var eventosHoy = eventosCitas
                    .Where(e => e.FechaString == hoyStr)
                    .Count();
                EventosProximos = eventosHoy;
                OnPropertyChanged(nameof(EventosProximos));

                if (ProxEventos != null)
                {
                    ProxEventos.Text = EventosProximos.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos del dashboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarScoreCasos()
        {
            try
            {
                await _supabaseCasos.InicializarAsync();
                var casos = await _supabaseCasos.ObtenerTodosAsync();
                var fechaLimite = DateTime.Now.AddDays(-28);

                // Casos activos creados en los últimos 28 días y que no estén cerrados
                var casosActivosPeriodo = casos
                    .Where(c => c.fecha_inicio >= fechaLimite && c.estado_nombre != "Cerrado")
                    .ToList();

                CasosActivos = casosActivosPeriodo.Count;

                // Contador de nuevos casos activos (mismo filtro)
                UpdateCasosCountChange(casosActivosPeriodo.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar casos: {ex.Message}");
            }
        }

        private async Task CargarScoreDocumentos()
        {
            try
            {
                await _documentosService.InicializarAsync();
                var documentos = await _documentosService.ObtenerTodosDocumentosManualAsync();
                var fechaLimite = DateTime.Now.AddDays(-28);

                // Filtrar documentos subidos en los últimos 28 días
                var documentosNuevos = documentos.Where(d => d.fecha_subid >= fechaLimite).ToList();

                // Obtener todos los casos para verificar el estado
                await _supabaseCasos.InicializarAsync();
                var casos = await _supabaseCasos.ObtenerTodosAsync();
                var casosNoCerrados = casos.Where(c => c.estado_nombre != "Cerrado").Select(c => c.id).ToHashSet();

                // Solo contar documentos cuyo caso asociado no esté cerrado
                var documentosNuevosActivos = documentosNuevos.Where(d => casosNoCerrados.Contains(d.id_caso)).ToList();

                Documentos = documentosNuevosActivos.Count;
                UpdateDocumentosCountChange(documentosNuevosActivos.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar documentos: {ex.Message}");
            }
        }

        private async Task CargarCasosRecientes()
        {
            try
            {
                await _supabaseCasos.InicializarAsync();
                var casos = await _supabaseCasos.ObtenerTodosAsync();

                var fechaLimite = DateTime.Now.Date.AddDays(-28);

                var casosRecientes = casos
                    .Where(c => c.fecha_inicio.Date >= fechaLimite)
                    .GroupBy(c => c.id)
                    .Select(g => g.First())
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

        private async Task ActualizarTarea(SupabaseTarea tarea)
        {
            try
            {
                await _tareasService.InicializarAsync();
                await _tareasService.ActualizarAsync(tarea);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar la tarea: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task EliminarCaso(int id)
        {
            try
            {
                await _supabaseCasos.EliminarAsync(id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar el caso: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateClientCountChange()
        {
            var fechaLimite = DateTime.Now.AddDays(-28);
            int diferencia = _clientCount - _previousClientCount;

            // Solo mostrar la diferencia si es reciente (últimos 28 días)
            if (diferencia > 0)
            {
                ClientCountChange = $"+{diferencia}";
            }
            else
            {
                ClientCountChange = "+0";
            }
        }

        private void UpdateCasosCountChange(int nuevosCasos)
        {
            if (nuevosCasos > 0)
            {
                scoreCasos.Text = $"+{nuevosCasos}";
            }
            else
            {
                scoreCasos.Text = "+0";
            }
        }

        private void UpdateDocumentosCountChange(int nuevosDocumentos)
        {
            if (nuevosDocumentos > 0)
            {
                scoreDocumentos.Text = $"+{nuevosDocumentos}";
            }
            else
            {
                scoreDocumentos.Text = "+0";
            }
        }

        private string ObtenerColorEstadoCaso(string estado)
        {
            return estado?.ToLower() switch
            {
                "abierto" => "#2196F3", // Azul
                "en proceso" => "#4CAF50", // Verde
                "cerrado" => "#F44336", // Rojo
                "pendiente" => "#FF9800", // Naranja
                "revisado" => "#9E9E9E", // Gris
                _ => "#9E9E9E"  // Gris por defecto
            };
        }

        private async void CheckBox_TareaFinalizada(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is SupabaseTarea tarea)
            {
                try
                {
                    tarea.estado = checkBox.IsChecked ?? false ? "Finalizado" : "Pendiente";
                    await ActualizarTarea(tarea);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al actualizar la tarea: {ex.Message}");
                    checkBox.IsChecked = !checkBox.IsChecked; // Revertir el cambio
                }
            }
        }

        private void VerTodosCasos_Click(object sender, RoutedEventArgs e)
        {
            var casosWindow = new Casos();
            casosWindow.Show();
            this.Close();
        }

        private void EditarCaso_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CasoViewModel caso)
            {
                var casosWindow = new Casos(caso.id);
                casosWindow.Show();
                this.Close();
            }
        }
        private async void MostrarDetallesCaso_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CasoViewModel casoVM)
            {
                try
                {
                    await _supabaseCasos.InicializarAsync();
                    // Obtener el caso con sus relaciones
                    var caso = await _supabaseCasos.ObtenerPorIdAsync(casoVM.id);
                    if (caso == null)
                    {
                        MessageBox.Show("El caso no se encontró en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Actualizar la información básica del caso en el popup
                    PopupTitulo.Text = $"Caso #{caso.referencia}";
                    PopupDescripcion.Text = caso.descripcion;
                    PopupCliente.Text = $"Cliente: {caso.Cliente?.nombre ?? "No especificado"}";
                    PopupTipo.Text = $"Tipo: {caso.TipoCaso?.nombre ?? "No especificado"}";
                    PopupEstado.Text = $"Estado: {caso.Estado?.nombre ?? "No especificado"}";
                    PopupFecha.Text = $"Fecha de inicio: {caso.fecha_inicio:dd/MM/yyyy}";

                    // Obtener y mostrar los documentos relacionados
                    await _documentosService.InicializarAsync();
                    var documentos = await _documentosService.ObtenerPorCasoAsync(caso.id);
                    PopupDocumentos.ItemsSource = documentos;

                    // Obtener y filtrar los próximos eventos
                    await _eventosCitasService.InicializarAsync();
                    var eventos = await _eventosCitasService.ObtenerEventosCitasPorCaso(caso.id);
                    var proximosEventos = eventos
                        .Where(e => e.Fecha >= DateTime.Now)
                        .OrderBy(e => e.Fecha)
                        .Take(3)
                        .Select(e => new
                        {
                            titulo = e.Titulo,
                            fecha = e.Fecha.ToString("dd/MM/yyyy HH:mm"),
                            descripcion = e.Descripcion
                        })
                        .ToList();
                    PopupEventos.ItemsSource = proximosEventos;

                    // Mostrar el popup
                    PopupDetallesCaso.IsOpen = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar los detalles del caso: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private async void EliminarCaso_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CasoViewModel caso)
            {
                var result = MessageBox.Show("¿Estás seguro de que deseas eliminar este caso?", "Confirmar eliminación",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await EliminarCaso(caso.id);
                        await CargarCasosRecientes();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar el caso: {ex.Message}");
                    }
                }
            }
        }

        private async void btnAddCaso_Click(object sender, RoutedEventArgs e)
        {
            var window = new SubVentanas.AñadirCasoWindow();
            if (window.ShowDialog() == true)
            {
                // Refresh the cases list if needed
                await CargarCasosRecientes();
            }
        }

        private void btnAddCliente_Click(object sender, RoutedEventArgs e)
        {
            var window = new SubVentanas.AñadirClienteWindow();
            if (window.ShowDialog() == true)
            {
                // Refresh the client list if needed
            }
        }

        /*
        public async Task<Caso> ObtenerPorIdAsync(int id)
        {
            return await _context.Casos
                .Include(c => c.Documentos)
                .Include(c => c.Cliente)
                .Include(c => c.Tipo)
                .Include(c => c.Estado)
                // Agrega más Includes si tienes más relaciones
                .FirstOrDefaultAsync(c => c.id == id);
        }
        */

        #endregion

        #region 🐞 Resvisiones Bugs 
        //revisar funcion CheckBox_TareaFinalizada => no funciona al 100% en local.
        #endregion


    }
}