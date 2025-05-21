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
using System.Windows.Documents;
using TFG_V0._01.BBDDLocal;
using Polly;
using SupabaseTarea = TFG_V0._01.Supabase.Models.Tarea;
using SupabaseCaso = TFG_V0._01.Supabase.Models.Caso;
using Microsoft.EntityFrameworkCore;

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
        private int _clientCount;
        private int _previousClientCount;
        private string _clientCountChange;
        private int _casosActivos;
        private int _documentos;
        private int _tareasPendientes;
        private int _casosRecientes;
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private DateOnly fechaActual = DateOnly.FromDateTime(DateTime.Now);

        private string mesText;

        private string anio;
        private readonly UIElement[] navbarItems;
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

            navbarItems = new UIElement[]
            {
                inicio, buscar, documentos, clientes, casos, agenda, ajustes
            };

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
                LoadingPanel.Visibility = Visibility.Collapsed;
                CargarCasosActibosLocal();
                CargarScoreCasosActivosLocal();
                CargarClientes();
                ScoreClientesNuevos();
                CargarDocumentos();
                ScoreDocumentosNuevos();
                CargarTareasPendientes();
                CargarTareasPendientesLista();


                LoadingPanel.Visibility = Visibility.Collapsed;
                if (string.IsNullOrWhiteSpace(mesText))
                    mesText = NombresMeses[fechaActual.Month - 1];
                if (string.IsNullOrWhiteSpace(anio))
                    anio = fechaActual.Year.ToString();
                cargarEventosCalendario(mesText, anio);
                cargarCasosRecientes();
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

            if (MainWindow.isDarkTheme)
            {
                CambiarIconosAClaros();
                CambiarTextosBlanco();
                backgroun_menu.Background = new SolidColorBrush(Color.FromArgb(48, 255, 255, 255)); // Fondo semitransparente
            }
            else
            {
                CambiarIconosAOscuros();
                CambiarTextosNegro();
                backgroun_menu.Background = new SolidColorBrush(Color.FromArgb(48, 128, 128, 128)); // Gris semitransparente
            }
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

        #region 🌓 modo oscuro/claro + navbar
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

        #region 🔄 navbar animacion
        private void CambiarVisibilidadNavbar(Visibility visibilidad)
        {
            foreach (var item in navbarItems)
                item.Visibility = visibilidad;
        }

        private void Menu_MouseEnter(object sender, MouseEventArgs e)
        {
            CambiarVisibilidadNavbar(Visibility.Visible);
        }

        private void Menu_MouseLeave(object sender, MouseEventArgs e)
        {
            CambiarVisibilidadNavbar(Visibility.Collapsed);
        }

        #endregion

        #region 🔄 Navbar botones
        private void AbrirVentana<T>() where T : Window, new()
        {
            var ventana = new T();
            ventana.Show();
            this.Close();
        }
        private void irHome(object sender, RoutedEventArgs e) => AbrirVentana<Home>();
        private void irJurisprudencia(object sender, RoutedEventArgs e) => AbrirVentana<BusquedaJurisprudencia>();
        private void irDocumentos(object sender, RoutedEventArgs e) => AbrirVentana<Documentos>();
        private void irClientes(object sender, RoutedEventArgs e) => AbrirVentana<Clientes>();
        private void irCasos(object sender, RoutedEventArgs e) => AbrirVentana<Casos>();
        private void irAyuda(object sender, RoutedEventArgs e) => AbrirVentana<Ayuda>();
        private void irAgenda(object sender, RoutedEventArgs e) => AbrirVentana<Agenda>();
        private void irAjustes(object sender, RoutedEventArgs e) => AbrirVentana<Ajustes>();
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
                cargarEventosCalendario(mesText, anio);
            }
        }
        #endregion

        #region 🈳 Idioma
        private void CargarIdioma(int idioma)
        {
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

        #region 📥 Cargar datos
        private async Task CargarDatosDashboard()
        {
            try
            {
                // Cargar clientes y actualizar contador
                var clientesTask = _clientesService.ObtenerClientesAsync();

                // Cargar casos, documentos y tareas en paralelo
                var casosTask = _supabaseCasos.ObtenerTodosAsync();
                var documentosService = new SupabaseDocumentos();
                var documentosInitTask = documentosService.InicializarAsync();
                var tareasService = new SupabaseTareas();
                var tareasInitTask = tareasService.InicializarAsync();
                var recienteService = new SupabaseReciente();
                var recienteInitTask = recienteService.InicializarAsync();

                await Task.WhenAll(documentosInitTask, tareasInitTask, recienteInitTask);

                var documentosTask = documentosService.ObtenerTodosAsync();
                var tareasTask = tareasService.ObtenerTodosAsync();
                var recientesTask = recienteService.ObtenerTodosAsync();

                var clientes = await clientesTask;
                ClientCount = clientes?.Count ?? 0;
                _previousClientCount = 0;
                UpdateClientCountChange();

                var casos = await casosTask;
                CasosActivos = casos?.Count ?? 0;

                var documentos = await documentosTask;
                Documentos = documentos?.Count ?? 0;

                var tareas = await tareasTask;
                var tareasPendientes = tareas.Where(t => t.estado != "Finalizado").ToList();
                TareasPendientes = tareasPendientes.Count;

                TareasPendientesLista.Clear();
                foreach (var tarea in tareasPendientes)
                    TareasPendientesLista.Add(tarea);

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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos del dashboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateClientCountChange()
        {
            int change = ClientCount - _previousClientCount;
            ClientCountChange = change > 0 ? $"+{change}" : change < 0 ? change.ToString() : "+0";
        }

        private async void CheckBox_TareaFinalizada(object sender, RoutedEventArgs e)
        {
            if (MainWindow.tipoBBDD)
            {
                try
                {
                    if (sender is CheckBox checkBox && checkBox.DataContext is SupabaseTarea tarea)
                    {
                        tarea.estado = "Finalizado";
                        var tareasService = new SupabaseTareas();
                        await tareasService.InicializarAsync();
                        await tareasService.ActualizarAsync(tarea);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al actualizar la tarea: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                try
                {
                    if (sender is CheckBox checkBox && checkBox.DataContext is SupabaseTarea tarea)
                    {
                        using (var db = new TfgContext())
                        {
                            var tareaLocal = db.Tareas.FirstOrDefault(t => t.Id == tarea.id);
                            if (tareaLocal != null)
                            {
                                tareaLocal.Estado = "Finalizado";
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al actualizar la tarea: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ComboBox_EstadoChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is SupabaseTarea tarea)
            {
                var nuevoEstado = comboBox.SelectedItem as string;
                if (!string.IsNullOrEmpty(nuevoEstado) && tarea.estado != nuevoEstado)
                {
                    tarea.estado = nuevoEstado;
                    var tareasService = new SupabaseTareas();
                    await tareasService.InicializarAsync();
                    await tareasService.ActualizarAsync(tarea);

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
            casosrecientesLocal.Visibility = Visibility.Collapsed;
            casosrecientesSupa.Visibility = Visibility.Visible;
            try
            {
                await _supabaseCasos.InicializarAsync();
                var casos = await _supabaseCasos.ObtenerTodosAsync();

                var fechaLimite = DateTime.Now.Date.AddDays(-3);

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

        private string ObtenerColorEstado(string estado) => estado.ToLower() switch
        {
            "activo" => "#4CAF50",
            "en proceso" => "#FF9800",
            "finalizado" => "#F44336",
            _ => "#757575"
        };

        private string ObtenerColorEstadoCaso(string estado)
        {
            return estado.ToLower() switch
            {
                "abierto" => "#43A047",
                "en proceso" => "#1976D2",
                "cerrado" => "#D32F2F",
                "pendiente" => "#FFB300",
                "revisado" => "#5C6BC0",
                _ => "#BDBDBD"
            };
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

        #region 📥 ScoreCasos
        private async void CargarScoreCasos()
        {
            try
            {
                await _supabaseCasos.InicializarAsync();
                var todos = await _supabaseCasos.ObtenerTodosAsync();

                var primerDiaMesActual = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var primerDiaMesAnterior = primerDiaMesActual.AddMonths(-1);
                var primerDiaMesSiguiente = primerDiaMesActual.AddMonths(1);

                Func<SupabaseCaso, bool> esActivo = c =>
                    c.estado_nombre.Equals("abierto", StringComparison.OrdinalIgnoreCase) ||
                    c.estado_nombre.Equals("en proceso", StringComparison.OrdinalIgnoreCase);

                int casosMesAnterior = todos
                    .Where(esActivo)
                    .Count(c => c.fecha_inicio >= primerDiaMesAnterior && c.fecha_inicio < primerDiaMesActual);

                int casosMesActual = todos
                    .Where(esActivo)
                    .Count(c => c.fecha_inicio >= primerDiaMesActual && c.fecha_inicio < primerDiaMesSiguiente);

                int diferencia = casosMesActual - casosMesAnterior;

                if (scoreCasos != null)
                {
                    if (diferencia > 0)
                        scoreCasos.Text = $"+{diferencia}";
                    else
                        scoreCasos.Text = diferencia.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al calcular el score de casos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region 📥 ScoreDocumentos
        private async void CargarScoreDocumentos()
        {
            try
            {
                var documentosService = new SupabaseDocumentos();
                await documentosService.InicializarAsync();
                var documentos = await documentosService.ObtenerTodosAsync();

                var ayer = DateTime.Now.Date.AddDays(-1);

                int documentosNuevos = documentos.Count(d => d.fecha_subid.Date == ayer);

                if (scoreDocumentos != null)
                {
                    scoreDocumentos.Text = $"+{documentosNuevos}";
                }
                else
                {
                    scoreDocumentos.Text = $"0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al calcular el score de documentos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region 📥 Proximos eventos
        /*
        private async void CargarProximosEventos()
        {
            try
            {
                var eventosService = new SupabaseEventos();
                await eventosService.InicializarAsync();
                var eventos = await eventosService.ObtenerTodosAsync();
                var proximosEventos = eventos
                    .Where(e => e.fecha_evento.Date >= DateTime.Now.Date)
                    .OrderBy(e => e.fecha_evento)
                    .ToList();
                // Aquí puedes mostrar los próximos eventos en la interfaz de usuario
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los próximos eventos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        */
        #endregion

        #endregion



        #region 💾 BBDD LOCAL

        #region 📥 cargar total de casos activos Local
        private void CargarCasosActibosLocal()
        {
            try
            {
                using (var db = new TfgContext())
                {
                    var estadosActivos = new[] { 1, 2, 4, 5 };
                    var casosActivos = db.Casos
                    .Count(c => estadosActivos.Contains(c.IdEstado));
                    totalCActivos.Text = casosActivos.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los casos activos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region 📥 ScoreCasosActivos 
        private void CargarScoreCasosActivosLocal()
        {
            try
            {
                using (var db = new TfgContext())
                {
                    var casosActivos = db.Casos
                        .Count(c => c.IdEstado == 1 || c.IdEstado == 2 || c.IdEstado == 4 || c.IdEstado == 5);
                    scoreCasos.Text = casosActivos.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el score de casos activos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region 📥 Cargar Clientes totales
        private void CargarClientes()
        {
            try
            {
                using (var db = new TfgContext())
                {
                    var clientes = db.Clientes.Count();
                    txtClientCount.Text = clientes.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los clientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region 📥 Cargar Score Clientes Nuevos este mes
        private void ScoreClientesNuevos()
        {
            try
            {
                using (var db = new TfgContext())
                {
                    // Convertir los límites a DateOnly
                    var primerDiaMesActual = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1);
                    var primerDiaMesSiguiente = primerDiaMesActual.AddMonths(1);

                    // Contar clientes cuyo contrato es de este mes
                    int clientesNuevos = db.Clientes
                        .Count(c => c.FechaContrato >= primerDiaMesActual && c.FechaContrato < primerDiaMesSiguiente);

                    if (txtClientCountChange != null)
                        txtClientCountChange.Text = $"+{clientesNuevos}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el score de clientes nuevos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region 📥 Cargar Todos los documentos
        private void CargarDocumentos()
        {
            try
            {
                using (var db = new TfgContext())
                {
                    var documentos = db.Documentos.Count();
                    DocTotales.Text = documentos.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los documentos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region 📥 Cargar Score Documentos Nuevos
        private void ScoreDocumentosNuevos()
        {
            try
            {
                using (var db = new TfgContext())
                {
                    var hoy = DateOnly.FromDateTime(DateTime.Now);
                    var documentosNuevos = db.Documentos
                        .Count(d => d.FechaSubid == hoy);
                    scoreDocumentos.Text = $"+{documentosNuevos}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el score de documentos nuevos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region 📥 Tareas pendientes para hoy
        private void CargarTareasPendientes()
        {
            try
            {
                using (var db = new TfgContext())
                {
                    var hoy = DateOnly.FromDateTime(DateTime.Now);
                    var tareasPendientes = db.Tareas
                        .Count(t => t.FechaFin == hoy && t.Estado != "Finalizado");
                    ProxEventos.Text = tareasPendientes.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las tareas pendientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region 📥 Cargar lista de tareas pendientes
        private void CargarTareasPendientesLista()
        {
            try
            {
                using (var db = new TfgContext())
                {
                    var tareasPendientes = db.Tareas
                        .Where(t => t.Estado != "Finalizado")
                        .OrderBy(t => t.FechaFin)
                        .ToList();

                    TareasPendientesLista.Clear();
                    foreach (var tarea in tareasPendientes)
                    {
                        // Si SupabaseTarea y la entidad local Tarea no son iguales, mapea los campos necesarios
                        TareasPendientesLista.Add(new SupabaseTarea
                        {
                            id = tarea.Id,
                            titulo = tarea.Titulo,
                            descripcion = tarea.Descripcion,
                            fecha_vencimiento = tarea.FechaFin.ToDateTime(TimeOnly.MinValue),
                            estado = tarea.Estado
                            // Añade aquí otros campos si es necesario
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la lista de tareas pendientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region 📥 Cargar Eventos Citas 
        private void cargarEventosCalendario(string mesText, string anio)
        {
            var colores = new List<string>
    {
        "#FF5722", "#F4511E", "#E64A19", "#D84315",
        "#009688", "#26A69A", "#00796B", "#004D40",
        "#3F51B5", "#5C6BC0", "#3949AB", "#1A237E"
    };

            // Usar el array NombresMeses ya definido en la clase
            int mes = Array.FindIndex(NombresMeses, m =>
                m.Equals(mesText?.Trim(), StringComparison.OrdinalIgnoreCase)
            ) + 1;

            if (mes == 0)
            {
                MessageBox.Show($"Mes no válido: '{mesText}'", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(anio, out int anioInt))
            {
                MessageBox.Show("Año no válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new TfgContext())
            {
                var tareas = context.Tareas
                    .Where(t => t.FechaFin.Month == mes && t.FechaFin.Year == anioInt && t.Estado != "finalizado")
                    .ToList();

                var stackPanel = this.FindName("PanelEventos") as StackPanel;

                if (stackPanel != null)
                {
                    stackPanel.Children.Clear();

                    var random = new Random();
                    int offset = random.Next(colores.Count);

                    for (int i = 0; i < tareas.Count; i++)
                    {
                        var tarea = tareas[i];
                        string colorHex = colores[(i + offset) % colores.Count];

                        var border = new Border
                        {
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                            CornerRadius = new CornerRadius(10),
                            Padding = new Thickness(10),
                            Margin = new Thickness(0, 5, 0, 5)
                        };

                        var innerGrid = new Grid();
                        innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                        var fechaBorder = new Border
                        {
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30FFFFFF")),
                            CornerRadius = new CornerRadius(5),
                            Padding = new Thickness(8, 5, 8, 5),
                            Margin = new Thickness(0, 0, 10, 0)
                        };
                        var fechaText = new TextBlock
                        {
                            Text = tarea.FechaFin.Day.ToString(),
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.White
                        };
                        fechaBorder.Child = fechaText;

                        var detallesStack = new StackPanel();
                        var tituloText = new TextBlock
                        {
                            Text = tarea.Titulo,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = Brushes.White
                        };
                        var descripcionText = new TextBlock
                        {
                            Text = tarea.Descripcion,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCFFFFFF")),
                            FontSize = 12
                        };

                        detallesStack.Children.Add(tituloText);
                        detallesStack.Children.Add(descripcionText);

                        Grid.SetColumn(fechaBorder, 0);
                        Grid.SetColumn(detallesStack, 1);
                        innerGrid.Children.Add(fechaBorder);
                        innerGrid.Children.Add(detallesStack);

                        border.Child = innerGrid;
                        stackPanel.Children.Add(border);
                    }
                }
            }
        }
        #endregion

        #region Casos Recientas
        private void cargarCasosRecientes()
        {
            casosrecientesLocal.Visibility = Visibility.Visible;
            casosrecientesSupa.Visibility = Visibility.Collapsed;
            using (var context = new TfgContext())
            {
                var recienteIds = context.Recientes
                    .Where(r => r.FechaHora <= DateTime.Now)
                    .OrderByDescending(r => r.FechaHora)
                    .Take(5)
                    .Select(r => r.IdCaso)
                    .ToList();

                var casosRecientes = context.Casos
                    .Include(c => c.IdClienteNavigation)
                    .Include(c => c.IdEstadoNavigation)
                    .Where(c => recienteIds.Contains(c.Id))
                    .ToList();

                CasosContainer.Children.Clear();

                foreach (var caso in casosRecientes)
                {
                    var cliente = caso.IdClienteNavigation;
                    var estado = caso.IdEstadoNavigation;


                    var border = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF)),
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(10),
                        Margin = new Thickness(0, 0, 0, 10)
                    };

                    var grid = new Grid();
                    for (int i = 0; i < 5; i++)
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = (i == 4 ? GridLength.Auto : new GridLength(1, GridUnitType.Star)) });

                    grid.Children.Add(CreateTextBlock(caso.Titulo, 0));
                    grid.Children.Add(CreateTextBlock($"{cliente.Nombre} {cliente.Apellido1} {cliente.Apellido2}", 1));
                    grid.Children.Add(CreateTextBlock(caso.Descripcion, 2));


                    var estadoPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                    var estadoBorder = new Border
                    {
                        Background = new SolidColorBrush(GetColorForEstado(estado.Nombre)),
                        CornerRadius = new CornerRadius(5),
                        Padding = new Thickness(8, 3, 8, 3)
                    };
                    estadoBorder.Child = new TextBlock
                    {
                        Text = estado.Nombre,
                        Foreground = Brushes.White,
                        FontSize = 12
                    };
                    estadoPanel.Children.Add(estadoBorder);
                    Grid.SetColumn(estadoPanel, 3);
                    grid.Children.Add(estadoPanel);

                    //Botones sin definir aun 
                    var btnPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    btnPanel.Children.Add(CreateIconButton("luna.png"));
                    btnPanel.Children.Add(CreateIconButton("sol.png"));
                    Grid.SetColumn(btnPanel, 4);
                    grid.Children.Add(btnPanel);

                    border.Child = grid;
                    CasosContainer.Children.Add(border);
                }
            }
        }

        private TextBlock CreateTextBlock(string text, int column)
        {
            var tb = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(tb, column);
            return tb;
        }

        private Button CreateIconButton(string imageName)
        {
            return new Button
            {
                Width = 35,
                Height = 35,
                Background = new SolidColorBrush(Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF)),
                Margin = new Thickness(0, 0, 5, 0),
                Style = (Style)this.FindResource("RoundedButtonStyle"),
                Content = new Image
                {
                    Source = new BitmapImage(new Uri($"pack://application:,,,/TFG V0.01;component/Recursos/Iconos/{imageName}")),
                    Width = 15,
                    Height = 15
                }
            };
        }

        private Color GetColorForEstado(string estado)
        {
            return estado switch
            {
                "Abierto" => (Color)ColorConverter.ConvertFromString("#64B5F6"),   // Azul claro
                "En Proceso" => (Color)ColorConverter.ConvertFromString("#FFB300"), // Amarillo
                "Cerrado" => (Color)ColorConverter.ConvertFromString("#F44336"),   // Rojo
                "Pendiente" => (Color)ColorConverter.ConvertFromString("#FF9800"), // Naranja
                "Revisado" => (Color)ColorConverter.ConvertFromString("#4CAF50"),  // Verde
                _ => Colors.Gray
            };
        }
        #endregion

        #endregion




        #region 🐞 Resvisiones Bugs 
        //revisar funcion CheckBox_TareaFinalizada => no funciona al 100% en local.

        #endregion
    }
}