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

            CargarIdioma(MainWindow.idioma);
            //CargarIdiomaNavbar(MainWindow.idioma);

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
            //this.Loaded += async (s, e) => { await CargarDatosDashboard(); CargarScoreCasos(); CargarCasosRecientes(); };
        }
        #endregion

        #region Patalla de carga
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadingPanel.Visibility = Visibility.Visible;
            CargarDatosDashboard();
            CargarScoreCasos();
            CargarCasosRecientes();
            CargarScoreDocumentos();
            LoadingPanel.Visibility = Visibility.Collapsed;
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

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.isDarkTheme = !MainWindow.isDarkTheme;
            AplicarModoSistema();
            var fadeAnimation = CrearFadeAnimation(0.7, 0.9, 0.3, true);
            backgroundFondo.BeginAnimation(OpacityProperty, fadeAnimation);
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
            fadeInStoryboard = new Storyboard();
            var fadeIn = CrearFadeAnimation(0, 1, 0.5);
            Storyboard.SetTarget(fadeIn, this);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(nameof(Opacity)));
            fadeInStoryboard.Children.Add(fadeIn);

            shakeStoryboard = new Storyboard();
            var shakeAnimation = CrearShakeAnimation();
            shakeStoryboard.Children.Add(shakeAnimation);
        }

        private DoubleAnimation CrearFadeAnimation(double from, double to, double durationSeconds, bool autoReverse = false)
        {
            return new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                AutoReverse = autoReverse
            };
        }

        private DoubleAnimation CrearShakeAnimation()
        {
            return new DoubleAnimation
            {
                From = 0,
                To = 5,
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3),
                Duration = TimeSpan.FromSeconds(0.05)
            };
        }

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

        #region Cargar datos
        private async Task CargarDatosDashboard()
        {
            try
            {
                await _clientesService.InicializarAsync();
                var clientes = await _clientesService.ObtenerClientesAsync();
                ClientCount = clientes?.Count ?? 0;
                _previousClientCount = 0;
                UpdateClientCountChange();

                var casosService = new SupabaseCasos();
                await casosService.InicializarAsync();
                var casos = await casosService.ObtenerTodosAsync();
                CasosActivos = casos?.Count ?? 0;

                var documentosService = new SupabaseDocumentos();
                await documentosService.InicializarAsync();
                var documentos = await documentosService.ObtenerTodosAsync();
                Documentos = documentos?.Count ?? 0;

                var tareasService = new SupabaseTareas();
                await tareasService.InicializarAsync();
                var tareas = await tareasService.ObtenerTodosAsync();
                var tareasPendientes = tareas.Where(t => t.estado != "Finalizado").ToList();
                TareasPendientes = tareasPendientes.Count;

                TareasPendientesLista.Clear();
                foreach (var tarea in tareasPendientes)
                    TareasPendientesLista.Add(tarea);

                var recienteService = new SupabaseReciente();
                await recienteService.InicializarAsync();
                var recientes = await recienteService.ObtenerTodosAsync();
                CasosRecientesLista.Clear();

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
                ClientCountChange = $"+{change}";
            else if (change < 0)
                ClientCountChange = change.ToString();
            else
                ClientCountChange = "+0";
        }

        private async void CheckBox_TareaFinalizada(object sender, RoutedEventArgs e)
        {
            try
            {
                var checkBox = sender as CheckBox;
                if (checkBox?.DataContext is Tarea tarea)
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
                    return "#43A047";
                case "en proceso":
                    return "#1976D2";
                case "cerrado":
                    return "#D32F2F";
                case "pendiente":
                    return "#FFB300";
                case "revisado":
                    return "#5C6BC0";
                default:
                    return "#BDBDBD";
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

        #region ScoreCasos
        private async void CargarScoreCasos()
        {
            try
            {
                await _supabaseCasos.InicializarAsync();
                var todos = await _supabaseCasos.ObtenerTodosAsync();

                var primerDiaMesActual = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var primerDiaMesAnterior = primerDiaMesActual.AddMonths(-1);
                var primerDiaMesSiguiente = primerDiaMesActual.AddMonths(1);

                Func<Caso, bool> esActivo = c =>
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

        #region ScoreDocumentos
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

        #region Idioma
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

        
    }
}
