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
        private readonly SupabaseContactos _contactosService;
        private ObservableCollection<EventoViewModel> _eventosDelDia;
        private ObservableCollection<EventoViewModel> _eventosDeHoy;
        private DateTime _fechaSeleccionada;
        private Dictionary<DateTime, string> _diasConEventoColor;
        private System.Windows.Threading.DispatcherTimer _timerActualizacion;

        // Colecciones para los ComboBox de Contacto
        private ObservableCollection<TFG_V0._01.Supabase.Models.Caso> _casosParaContacto;
        private ObservableCollection<string> _rolesParaContacto;

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

        // Propiedades públicas para las colecciones de Contacto
        public ObservableCollection<TFG_V0._01.Supabase.Models.Caso> CasosParaContacto
        {
            get => _casosParaContacto;
            set { _casosParaContacto = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> RolesParaContacto
        {
            get => _rolesParaContacto;
            set { _rolesParaContacto = value; OnPropertyChanged(); }
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
            DataContext = this;
            InitializeAnimations();
            CrearFondoAnimado();
            AplicarModoSistema();
            InitializeTimeComboBox();

            _eventosCitasService = new SupabaseEventosCitas();
            _estadosEventosService = new SupabaseEstadosEventos();
            _casosService = new SupabaseCasos();
            _contactosService = new SupabaseContactos();

            EventosDelDia = new ObservableCollection<EventoViewModel>();
            EventosDeHoy = new ObservableCollection<EventoViewModel>();
            DiasConEventoColor = new Dictionary<DateTime, string>();

            // Inicializar colecciones para Contacto
            CasosParaContacto = new ObservableCollection<TFG_V0._01.Supabase.Models.Caso>();
            RolesParaContacto = new ObservableCollection<string>();

            _fechaSeleccionada = DateTime.Today;

            // Inicializar el temporizador
            _timerActualizacion = new System.Windows.Threading.DispatcherTimer();
            _timerActualizacion.Interval = TimeSpan.FromMinutes(1);
            _timerActualizacion.Tick += async (s, e) => await CargarEventosDeHoy();
            _timerActualizacion.Start();

            // Cargar datos iniciales (incluyendo datos para Contacto)
            CargarDatosIniciales();
            CargarDatosContactoAsync();
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
            ActualizarColoresFondoYAnimacion(MainWindow.isDarkTheme);
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            // Alternar el estado del tema
            MainWindow.isDarkTheme = !MainWindow.isDarkTheme;
            AplicarModoSistema();
        }
        #endregion

        #region Fondo Animado
        private void CrearFondoAnimado()
        {
            // Crear los brushes una sola vez
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

            // Crear el DrawingGroup con los brushes
            var drawingGroup = new DrawingGroup();
            drawingGroup.Children.Add(new GeometryDrawing(mesh1Brush, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
            drawingGroup.Children.Add(new GeometryDrawing(mesh2Brush, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));

            // Obtener el DrawingBrush del XAML y asignar el DrawingGroup
            var drawingBrush = this.FindName("meshGradientBrush") as DrawingBrush;
             if (drawingBrush != null)
             {
                 drawingBrush.Drawing = drawingGroup;
             }
             // No asignamos el DrawingBrush al Background aquí, ya está asignado en XAML
        }

        private void ActualizarColoresFondoYAnimacion(bool esModoOscuro)
        {
            if (mesh1Brush == null || mesh2Brush == null) return; // Asegurarse de que los brushes existan

            // Limpiar los GradientStops existentes y añadir los nuevos
            mesh1Brush.GradientStops.Clear();
            mesh2Brush.GradientStops.Clear();

            if (esModoOscuro)
            {
                // Colores para modo oscuro (los mismos que en Home.xaml.cs para consistencia)
                mesh1Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#8C7BFF"), 0));
                mesh1Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#08a693"), 1));

                mesh2Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3a4d5f"), 0));
                mesh2Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#272c3f"), 1));
            }
            else
            {
                // Colores para modo claro (los mismos que en Home.xaml.cs para consistencia)
                mesh1Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#de9cb8"), 0));
                mesh1Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#9dcde1"), 1));

                mesh2Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#dc8eb8"), 0));
                mesh2Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#98d3ec"), 1));
            }

            // Reiniciar la animación para que apunte a los brushes con los colores actualizados
            StartMeshAnimation();
        }

         private void StartMeshAnimation()
         {
             if (mesh1Brush == null || mesh2Brush == null) return; // Asegurarse de que los brushes existan

             // Detener la animación actual si está corriendo
             if (meshAnimStoryboard != null)
             {
                 meshAnimStoryboard.Stop();
             }

             // Crear un nuevo Storyboard cada vez que se inicia la animación
             meshAnimStoryboard = new Storyboard();

             // Animación para mesh1Brush
             PointAnimation mesh1CenterAnimation = new PointAnimation
             {
                 From = new Point(0.3, 0.3),
                 To = new Point(0.7, 0.5), // Usar mismos puntos de animación que Home
                 Duration = TimeSpan.FromSeconds(8), // Duración total de la secuencia
                 AutoReverse = true,
                 RepeatBehavior = RepeatBehavior.Forever,
                 EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } // Usar el mismo easing que Home
             };
             Storyboard.SetTarget(mesh1CenterAnimation, mesh1Brush);
             Storyboard.SetTargetProperty(mesh1CenterAnimation, new PropertyPath(RadialGradientBrush.CenterProperty));
             meshAnimStoryboard.Children.Add(mesh1CenterAnimation);

             // Animación para mesh2Brush
             PointAnimation mesh2CenterAnimation = new PointAnimation
             {
                 From = new Point(0.7, 0.7),
                 To = new Point(0.4, 0.4), // Usar mismos puntos de animación que Home
                 Duration = TimeSpan.FromSeconds(8), // Duración total de la secuencia
                 AutoReverse = true,
                 RepeatBehavior = RepeatBehavior.Forever,
                 EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } // Usar el mismo easing que Home
             };
             Storyboard.SetTarget(mesh2CenterAnimation, mesh2Brush);
             Storyboard.SetTargetProperty(mesh2CenterAnimation, new PropertyPath(RadialGradientBrush.CenterProperty));
             meshAnimStoryboard.Children.Add(mesh2CenterAnimation);

             meshAnimStoryboard.Begin();
         }
        #endregion

        #region Animaciones (Básicas)
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
            // Asegurarse de que el otro panel esté oculto
            if (SlidePanel.Visibility == Visibility.Visible)
            {
                HideNewContactPanel();
            }

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

            // Mostrar el overlay con animación
            OverlayPanel.Visibility = Visibility.Visible;
            OverlayPanel.Opacity = 0;
            var fadeOverlayAnimation = new DoubleAnimation
            {
                From = 0,
                To = 0.8, // Opacidad del overlay
                Duration = TimeSpan.FromMilliseconds(300)
            };
            OverlayPanel.BeginAnimation(UIElement.OpacityProperty, fadeOverlayAnimation);
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

            // Ocultar el overlay con animación
            var fadeOverlayAnimation = new DoubleAnimation
            {
                From = 0.8, // Opacidad actual del overlay
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            fadeOverlayAnimation.Completed += (s, e) => OverlayPanel.Visibility = Visibility.Collapsed; // Ocultar completamente al finalizar
            OverlayPanel.BeginAnimation(UIElement.OpacityProperty, fadeOverlayAnimation);
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
        #endregion

        #region Gestión de panel de nuevo contacto
        private void NuevoContactoButton_Click(object sender, RoutedEventArgs e)
        {
            ShowNewContactPanel();
        }

        private void ShowNewContactPanel()
        {
            // Asegurarse de que el otro panel esté oculto
            if (NewEventPanel.Visibility == Visibility.Visible)
            {
                HideNewEventPanel();
            }

            // Limpiar los campos del formulario
            ContactCasoComboBox.SelectedItem = null;
            ContactNombreTextBox.Clear();
            ContactRolComboBox.SelectedItem = null;
            ContactTelefonoTextBox.Clear();
            ContactEmailTextBox.Clear();

            // Mostrar el panel con una animación suave
            SlidePanel.Visibility = Visibility.Visible;

            // Crear y comenzar una animación de fade in para el overlay
            OverlayPanel.Visibility = Visibility.Visible;
            OverlayPanel.Opacity = 0;
            var fadeOverlayAnimation = new DoubleAnimation
            {
                From = 0,
                To = 0.8, // Opacidad del overlay
                Duration = TimeSpan.FromMilliseconds(300)
            };
            OverlayPanel.BeginAnimation(UIElement.OpacityProperty, fadeOverlayAnimation);

            // Animación de deslizamiento para el panel
            var slideInAnimation = new DoubleAnimation
            {
                From = SlidePanel.ActualWidth,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            SlidePanelTransform.BeginAnimation(TranslateTransform.XProperty, slideInAnimation);
        }

        private void HideNewContactPanel()
        {
             // Ocultar el overlay con animación
            var fadeOverlayAnimation = new DoubleAnimation
            {
                From = 0.8, // Opacidad actual del overlay
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            fadeOverlayAnimation.Completed += (s, e) => OverlayPanel.Visibility = Visibility.Collapsed; // Ocultar completamente al finalizar
            OverlayPanel.BeginAnimation(UIElement.OpacityProperty, fadeOverlayAnimation);

            // Crear y comenzar una animación de deslizamiento hacia afuera para el SlidePanel
            var slideOutAnimation = new DoubleAnimation
            {
                From = 0,
                To = SlidePanel.ActualWidth,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            slideOutAnimation.Completed += (s, e) => SlidePanel.Visibility = Visibility.Collapsed;
            SlidePanelTransform.BeginAnimation(TranslateTransform.XProperty, slideOutAnimation);
        }

        private void CancelContactButton_Click(object sender, RoutedEventArgs e)
        {
            HideNewContactPanel();
        }

        private async void SaveContactButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ContactCasoComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Por favor, seleccione un caso.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(ContactNombreTextBox.Text))
                {
                    MessageBox.Show("Por favor, ingrese el nombre del contacto.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ContactRolComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Por favor, seleccione un rol.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var casoSeleccionado = (TFG_V0._01.Supabase.Models.Caso)ContactCasoComboBox.SelectedItem;
                var nuevoContacto = new TFG_V0._01.Supabase.Models.Contacto
                {
                    id_caso = casoSeleccionado.id,
                    nombre = ContactNombreTextBox.Text.Trim(),
                    tipo = ContactRolComboBox.SelectedItem.ToString(),
                    telefono = ContactTelefonoTextBox.Text.Trim(),
                    email = ContactEmailTextBox.Text.Trim(),
                    id = null // Asegúrate de que no se envía el id
                };

                await _contactosService.InsertarAsync(nuevoContacto);

                MessageBox.Show("Contacto guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                HideNewContactPanel(); // Ocultar el panel después de guardar
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el contacto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

         private void OverlayPanel_MouseDown(object sender, MouseButtonEventArgs e)
         {
             // Ocultar el panel de nuevo contacto si está visible
             if (SlidePanel.Visibility == Visibility.Visible)
             {
                 HideNewContactPanel();
             }
             // Si el panel de nuevo evento está visible, ocultarlo también
             else if (NewEventPanel.Visibility == Visibility.Visible)
             {
                 HideNewEventPanel();
             }
             // Si también tienes el panel de nuevo evento, asegúrate de ocultarlo aquí si el clic no fue en él.
         }
        #endregion

        #region Carga de datos de contacto
        private async Task CargarDatosContactoAsync()
        {
            try
            {
                await _casosService.InicializarAsync();
                await _contactosService.InicializarAsync(); // Inicializar servicio de contactos aquí también

                var casos = await _casosService.ObtenerTodosCasosManualAsync();
                 // Definir los roles localmente como se hacía en NuevoContactoWindow.xaml.cs
                 var roles = new List<string>
                {
                    "Abogado",
                    "Cliente",
                    "Testigo",
                    "Perito",
                    "Juez",
                    "Secretario Judicial",
                    "Otro"
                };

                // Actualizar las colecciones en el hilo de UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CasosParaContacto.Clear();
                    foreach (var caso in casos)
                    {
                        CasosParaContacto.Add(caso);
                    }

                    RolesParaContacto.Clear();
                    foreach (var rol in roles)
                    {
                        RolesParaContacto.Add(rol);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos de contacto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        #endregion
    }
}