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
using System.Runtime.CompilerServices;
using TFG_V0._01.Supabase.Models;
using System.Windows.Threading;

namespace TFG_V0._01.Ventanas
{
    /// <summary>
    /// Lógica de interacción para Agenda.xaml
    /// </summary>
    public partial class Agenda : Window, INotifyPropertyChanged
    {
        #region variables animacion
        private Storyboard fadeInStoryboard;
        private Storyboard shakeStoryboard;
        private Storyboard meshAnimStoryboard;

        // Brushes y fondo animado
        private RadialGradientBrush mesh1Brush;
        private RadialGradientBrush mesh2Brush;
        #endregion

        #region variables
        private readonly SupabaseEventosCitas _eventosCitasService;
        private readonly SupabaseEstadosEventos _estadosEventosService;
        private readonly SupabaseCasos _casosService;
        private ObservableCollection<EventoViewModel> _eventosDelDia;
        private ObservableCollection<EventoViewModel> _eventosDeHoy;
        private DateTime _fechaSeleccionada;
        private Dictionary<DateTime, string> _diasConEventoColor;
        private System.Windows.Threading.DispatcherTimer _timerActualizacion;

        public ObservableCollection<EventoViewModel> EventosDelDia
        {
            get => _eventosDelDia;
            set { _eventosDelDia = value; OnPropertyChanged(); }
        }

        public ObservableCollection<EventoViewModel> EventosDeHoy
        {
            get => _eventosDeHoy;
            set { _eventosDeHoy = value; OnPropertyChanged(); }
        }

        public Dictionary<DateTime, string> DiasConEventoColor
        {
            get => _diasConEventoColor;
            set { _diasConEventoColor = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Inicializacion
        public Agenda()
        {
            InitializeComponent();
            InitializeAnimations();
            AplicarModoSistema();
            InitializeTimeComboBox();

            _eventosCitasService = new SupabaseEventosCitas();
            _estadosEventosService = new SupabaseEstadosEventos();
            _casosService = new SupabaseCasos();

            EventosDelDia = new ObservableCollection<EventoViewModel>();
            EventosDeHoy = new ObservableCollection<EventoViewModel>();
            DiasConEventoColor = new Dictionary<DateTime, string>();
            _fechaSeleccionada = DateTime.Today;

            // Inicializar el temporizador
            _timerActualizacion = new System.Windows.Threading.DispatcherTimer();
            _timerActualizacion.Interval = TimeSpan.FromMinutes(1);
            _timerActualizacion.Tick += async (s, e) => await CargarEventosDeHoy();
            _timerActualizacion.Start();

            // Cargar datos iniciales
            CargarDatosIniciales();
        }
        #endregion

        #region Modo oscuro/claro
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
                this.Tag = true;
                navbar.ActualizarTema(true);
            }
            else
            {
                // Aplicar modo claro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna.png", UriKind.Relative));
                }
                this.Tag = false;
                navbar.ActualizarTema(false);
            }
        }

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
                this.Tag = true;
                navbar.ActualizarTema(true);
            }
            else
            {
                // Cambiar a modo claro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna.png", UriKind.Relative));
                }
                this.Tag = false;
                navbar.ActualizarTema(false);
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

            // Inicializar los brushes para el fondo animado
            mesh1Brush = new RadialGradientBrush
            {
                Center = new Point(0.3, 0.3),
                RadiusX = 0.5,
                RadiusY = 0.5,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(222, 156, 184), 0),
                    new GradientStop(Color.FromRgb(157, 205, 225), 1)
                }
            };

            mesh2Brush = new RadialGradientBrush
            {
                Center = new Point(0.7, 0.7),
                RadiusX = 0.6,
                RadiusY = 0.6,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(220, 142, 184), 0),
                    new GradientStop(Color.FromRgb(152, 211, 236), 1)
                }
            };

            // Crear el fondo animado
            DrawingGroup drawingGroup = new DrawingGroup();
            GeometryDrawing geometryDrawing1 = new GeometryDrawing
            {
                Brush = mesh1Brush,
                Geometry = new RectangleGeometry(new Rect(0, 0, 1, 1))
            };
            GeometryDrawing geometryDrawing2 = new GeometryDrawing
            {
                Brush = mesh2Brush,
                Geometry = new RectangleGeometry(new Rect(0, 0, 1, 1))
            };
            drawingGroup.Children.Add(geometryDrawing1);
            drawingGroup.Children.Add(geometryDrawing2);

            DrawingBrush drawingBrush = new DrawingBrush(drawingGroup)
            {
                Viewport = new Rect(0, 0, 1, 1),
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox,
                TileMode = TileMode.None
            };

            // Asignar el fondo al Grid
            var grid = this.FindName("meshGradientBrush") as DrawingBrush;
            if (grid != null)
            {
                grid.Drawing = drawingGroup;
            }

            // Iniciar la animación del fondo
            StartMeshAnimation();
        }

        private void StartMeshAnimation()
        {
            meshAnimStoryboard = new Storyboard();

            // Animación para mesh1
            PointAnimation mesh1CenterAnimation = new PointAnimation
            {
                From = new Point(0.3, 0.3),
                To = new Point(0.4, 0.4),
                Duration = TimeSpan.FromSeconds(10),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(mesh1CenterAnimation, mesh1Brush);
            Storyboard.SetTargetProperty(mesh1CenterAnimation, new PropertyPath(RadialGradientBrush.CenterProperty));
            meshAnimStoryboard.Children.Add(mesh1CenterAnimation);

            // Animación para mesh2
            PointAnimation mesh2CenterAnimation = new PointAnimation
            {
                From = new Point(0.7, 0.7),
                To = new Point(0.6, 0.6),
                Duration = TimeSpan.FromSeconds(8),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(mesh2CenterAnimation, mesh2Brush);
            Storyboard.SetTargetProperty(mesh2CenterAnimation, new PropertyPath(RadialGradientBrush.CenterProperty));
            meshAnimStoryboard.Children.Add(mesh2CenterAnimation);

            meshAnimStoryboard.Begin();
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

        #region Inicialización de controles
        private void InitializeTimeComboBox()
        {
            // Añadir horas en formato 24h al ComboBox
            for (int hour = 0; hour < 24; hour++)
            {
                for (int minute = 0; minute < 60; minute += 30)
                {
                    string time = $"{hour:D2}:{minute:D2}";
                    EventTimeComboBox.Items.Add(time);
                }
            }
            // Seleccionar la hora actual por defecto
            EventTimeComboBox.SelectedIndex = 0;
        }
        #endregion

        #region Gestión de panel de nuevo evento
        private void MainCalendar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Obtener la fecha seleccionada
            DateTime? selectedDate = MainCalendar.SelectedDate;

            if (selectedDate.HasValue)
            {
                // Establecer la fecha seleccionada en el DatePicker
                EventDatePicker.SelectedDate = selectedDate;

                // Mostrar el panel de nuevo evento
                ShowNewEventPanel();
            }
        }

        // Muestra el panel de nuevo evento con animación
        private void ShowNewEventPanel()
        {
            // Limpiar los campos del formulario
            EventTitleTextBox.Clear();
            EventDescriptionTextBox.Clear();
            EventLocationTextBox.Clear();
            ParticipantsListBox.Items.Clear();

            // Mostrar el panel con una animación suave
            NewEventPanel.Visibility = Visibility.Visible;
            NewEventPanel.Opacity = 0;

            // Crear una animación de fade in
            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            NewEventPanel.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void ClosePanelButton_Click(object sender, RoutedEventArgs e)
        {
            HideNewEventPanel();
        }

        private void CancelEventButton_Click(object sender, RoutedEventArgs e)
        {
            HideNewEventPanel();
        }

        private void HideNewEventPanel()
        {
            // Crear una animación de fade out
            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            animation.Completed += (s, e) =>
            {
                NewEventPanel.Visibility = Visibility.Collapsed;
            };

            NewEventPanel.BeginAnimation(UIElement.OpacityProperty, animation);
        }
        #endregion

        #region Carga de datos y eventos
        private async void CargarDatosIniciales()
        {
            try
            {
                await _eventosCitasService.InicializarAsync();
                await _estadosEventosService.InicializarAsync();
                await _casosService.InicializarAsync();

                // Establecer la fecha seleccionada al día actual
                _fechaSeleccionada = DateTime.Today;
                MainCalendar.SelectedDate = DateTime.Today;

                // Cargar los eventos
                await CargarEventosDelDia();
                await CargarEventosDeHoy();

                // Forzar la actualización de la UI
                EventosDelDiaList.ItemsSource = null;
                EventosDelDiaList.ItemsSource = EventosDelDia;
                EventosDeHoyList.ItemsSource = null;
                EventosDeHoyList.ItemsSource = EventosDeHoy;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos iniciales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarEventosDelDia()
        {
            try
            {
                var eventos = await _eventosCitasService.ObtenerEventosCitas();
                var estados = await _estadosEventosService.ObtenerEstadosEventos();
                var casos = await _casosService.ObtenerTodosCasosManualAsync();

                var eventosDelDia = eventos
                    .Where(e => e.Fecha.Date == _fechaSeleccionada.Date)
                    .OrderBy(e => e.FechaInicio)
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

                Application.Current.Dispatcher.Invoke(() =>
                {
                    EventosDelDia.Clear();
                    foreach (var evento in eventosDelDia)
                    {
                        EventosDelDia.Add(evento);
                    }
                });

                // Actualizar el diccionario de días con evento y color
                DiasConEventoColor.Clear();
                var eventosPorDia = eventos
                    .GroupBy(e => e.Fecha.Date)
                    .ToDictionary(g => g.Key, g => ObtenerColorEstado(estados.FirstOrDefault(s => s.Id == g.First().IdEstado)?.Nombre));
                foreach (var kv in eventosPorDia)
                    DiasConEventoColor[kv.Key] = kv.Value;

                // Forzar refresco visual del calendario
                MainCalendar.InvalidateVisual();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los eventos del día: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarEventosDeHoy()
        {
            try
            {
                var eventos = await _eventosCitasService.ObtenerEventosCitas();
                var estados = await _estadosEventosService.ObtenerEstadosEventos();
                var casos = await _casosService.ObtenerTodosCasosManualAsync();

                var eventosDeHoy = eventos
                    .Where(e => e.Fecha.Date == DateTime.Today)
                    .OrderBy(e => e.FechaInicio)
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

                Application.Current.Dispatcher.Invoke(() =>
                {
                    EventosDeHoy.Clear();
                    foreach (var evento in eventosDeHoy)
                    {
                        EventosDeHoy.Add(evento);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los eventos de hoy: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Utilidades de eventos
        private string ObtenerColorEstado(string estado)
        {
            return estado?.ToLower() switch
            {
                "finalizado" => "#F44336",   // Rojo
                "programado" => "#FFA726",   // Naranja
                "cancelado" => "#757575",    // Gris
                _ => "#BDBDBD"              // Gris claro por defecto
            };
        }
        #endregion

        #region Eventos de controles
        private async void MainCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is Calendar calendar && calendar.SelectedDate.HasValue)
            {
                _fechaSeleccionada = calendar.SelectedDate.Value;
                await CargarEventosDelDia();
                await CargarEventosDeHoy();
            }
        }

        private async void SaveEventButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EventTitleTextBox.Text))
                {
                    MessageBox.Show("Por favor, introduce un título para el evento.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var fechaSeleccionada = EventDatePicker.SelectedDate ?? DateTime.Today;
                var horaSeleccionada = EventTimeComboBox.SelectedItem?.ToString() ?? "00:00";
                var horaMinuto = TimeSpan.Parse(horaSeleccionada);

                var nuevoEvento = new EventoCita
                {
                    Titulo = EventTitleTextBox.Text,
                    Descripcion = EventDescriptionTextBox.Text,
                    Fecha = fechaSeleccionada,
                    FechaInicio = horaMinuto,
                    IdEstado = 1, // Estado por defecto: Programado
                    IdCaso = 0 // Por ahora no asociamos a ningún caso
                };

                await _eventosCitasService.InsertarEventoCita(nuevoEvento);
                await CargarEventosDelDia();
                await CargarEventosDeHoy();

                HideNewEventPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el evento: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NuevoContactoButton_Click(object sender, RoutedEventArgs e)
        {
            var nuevoContactoWindow = new SubVentanas.NuevoContactoWindow();
            nuevoContactoWindow.Owner = this;
            nuevoContactoWindow.ShowDialog();
        }
        #endregion
    }
}