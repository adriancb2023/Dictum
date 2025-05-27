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
using System.Collections.ObjectModel;

namespace TFG_V0._01.Ventanas
{
    public partial class Ayuda : Window
    {
        #region variables animacion
        private Storyboard fadeInStoryboard;
        private Storyboard shakeStoryboard;
        #endregion

        #region variables
        public ObservableCollection<GuiaRapida> GuiasRapidas { get; set; }

        // Variables para el fondo mesh gradient animado
        private Storyboard meshAnimStoryboard;
        private RadialGradientBrush mesh1Brush;
        private RadialGradientBrush mesh2Brush;
        private DrawingBrush meshGradientBrush;
        private bool fondoAnimadoInicializado = false;
        #endregion

        #region Inicializacion
        public Ayuda()
        {
            InitializeComponent();
            InitializeGuiasRapidas();
            InitializeAnimations();
            CrearFondoAnimado(); // Crear el fondo animado
            AplicarModoSistema();
            IniciarAnimacionMesh(); // Iniciar la animacion del fondo
        }
        #endregion

        #region Aplicar modo oscuro/claro cargado por sistema
        private void AplicarModoSistema()
        {
            this.Tag = MainWindow.isDarkTheme;
            AplicarTemaMesh(); // Aplicar tema al mesh gradient
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
            // Animación de fade in
            var fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5)
            };

            this.Resources.Add("FadeInAnimation", fadeInAnimation);
        }

        private void BeginFadeInAnimation()
        {
            var fadeInAnimation = this.Resources["FadeInAnimation"] as DoubleAnimation;
            if (fadeInAnimation != null)
            {
                this.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
            }
        }

        private void ShakeElement(UIElement element)
        {   
            // No es necesario en esta ventana, pero se mantiene por si acaso
        }
        #endregion

        #region Fondo mesh gradient animado igual que Home
        private void CrearFondoAnimado()
        {
             if (fondoAnimadoInicializado) return;

            // Obtener los brushes definidos en XAML
            mesh1Brush = this.FindResource("Mesh1") as RadialGradientBrush;
            mesh2Brush = this.FindResource("Mesh2") as RadialGradientBrush;

            if (mesh1Brush == null || mesh2Brush == null)
            {
                // Manejar el caso si los recursos no se encuentran
                return;
            }

            // Clonar los brushes para poder animarlos independientemente
            mesh1Brush = mesh1Brush.Clone();
            mesh2Brush = mesh2Brush.Clone();

            // Crear el DrawingBrush
            var drawingGroup = new DrawingGroup();
            drawingGroup.Children.Add(new GeometryDrawing(mesh1Brush, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
            drawingGroup.Children.Add(new GeometryDrawing(mesh2Brush, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
            meshGradientBrush = new DrawingBrush(drawingGroup) { Stretch = Stretch.Fill };

            // Asignar al fondo del Grid principal
            fondoAnimadoInicializado = true;
        }

        private void IniciarAnimacionMesh()
        {
            if (!fondoAnimadoInicializado) CrearFondoAnimado();
            meshAnimStoryboard?.Stop();
            meshAnimStoryboard = new Storyboard();

            // Animar Center de mesh1
            var anim1 = new PointAnimationUsingKeyFrames();
            anim1.KeyFrames.Add(new EasingPointKeyFrame(new Point(0.3, 0.3), KeyTime.FromTimeSpan(TimeSpan.Zero)));
            anim1.KeyFrames.Add(new EasingPointKeyFrame(new Point(0.7, 0.5), KeyTime.FromTimeSpan(TimeSpan.FromSeconds(4))) { EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } });
            anim1.KeyFrames.Add(new EasingPointKeyFrame(new Point(0.3, 0.3), KeyTime.FromTimeSpan(TimeSpan.FromSeconds(8))) { EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } });
            anim1.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTarget(anim1, mesh1Brush);
            Storyboard.SetTargetProperty(anim1, new PropertyPath(RadialGradientBrush.CenterProperty));
            meshAnimStoryboard.Children.Add(anim1);

            // Animar Center de mesh2
            var anim2 = new PointAnimationUsingKeyFrames();
            anim2.KeyFrames.Add(new EasingPointKeyFrame(new Point(0.7, 0.7), KeyTime.FromTimeSpan(TimeSpan.Zero)));
            anim2.KeyFrames.Add(new EasingPointKeyFrame(new Point(0.4, 0.4), KeyTime.FromTimeSpan(TimeSpan.FromSeconds(4))) { EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } });
            anim2.KeyFrames.Add(new EasingPointKeyFrame(new Point(0.7, 0.7), KeyTime.FromTimeSpan(TimeSpan.FromSeconds(8))) { EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } });
            anim2.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTarget(anim2, mesh2Brush);
            Storyboard.SetTargetProperty(anim2, new PropertyPath(RadialGradientBrush.CenterProperty));
            meshAnimStoryboard.Children.Add(anim2);

            meshAnimStoryboard.Begin();
        }

        private void AplicarTemaMesh()
        {
             if (!fondoAnimadoInicializado) CrearFondoAnimado();

            // Cambiar colores según el modo igual que en Home
            if (MainWindow.isDarkTheme)
            {
                mesh1Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#8C7BFF");
                mesh1Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#08a693");
                mesh2Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#3a4d5f");
                mesh2Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#272c3f");
            }
            else
            {
                mesh1Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#de9cb8");
                mesh1Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#9dcde1");
                mesh2Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#dc8eb8");
                mesh2Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#98d3ec");
            }
        }
        #endregion

        private void InitializeGuiasRapidas()
        {
            GuiasRapidas = new ObservableCollection<GuiaRapida>
            {
                new GuiaRapida
                {
                    Icon = "/TFG V0.01;component/Recursos/Icons/case.png",
                    Title = "Gestión de Casos",
                    Description = "Aprende a crear, editar y gestionar casos eficientemente. Organiza tus casos por estado, tipo y cliente para un mejor seguimiento."
                },
                new GuiaRapida
                {
                    Icon = "/TFG V0.01;component/Recursos/Icons/document.png",
                    Title = "Gestión de Documentos",
                    Description = "Descubre cómo organizar y gestionar tus documentos legales. Sube, categoriza y comparte documentos de forma segura."
                },
                new GuiaRapida
                {
                    Icon = "/TFG V0.01;component/Recursos/Icons/calendar.png",
                    Title = "Calendario y Citas",
                    Description = "Gestiona tu agenda de citas y eventos. Establece recordatorios y mantén un control de todas tus actividades."
                },
                new GuiaRapida
                {
                    Icon = "/TFG V0.01;component/Recursos/Icons/client.png",
                    Title = "Gestión de Clientes",
                    Description = "Administra la información de tus clientes, su historial de casos y documentación personal de forma centralizada."
                },
                new GuiaRapida
                {
                    Icon = "/TFG V0.01;component/Recursos/Icons/task.png",
                    Title = "Tareas y Seguimiento",
                    Description = "Organiza tus tareas pendientes, establece prioridades y realiza un seguimiento efectivo de tus actividades diarias."
                },
                new GuiaRapida
                {
                    Icon = "/TFG V0.01;component/Recursos/Icons/dashboard.png",
                    Title = "Dashboard y Estadísticas",
                    Description = "Utiliza el panel de control para tener una visión general de tus casos activos, tareas pendientes y próximas citas."
                }
            };

            var itemsControl = this.FindName("GuiasRapidasItemsControl") as ItemsControl;
            if (itemsControl != null)
            {
                itemsControl.ItemsSource = GuiasRapidas;
            }
        }

        private void EnviarMensaje_Click(object sender, RoutedEventArgs e)
        {
            var nombreTextBox = this.FindName("NombreTextBox") as TextBox;
            var emailTextBox = this.FindName("EmailTextBox") as TextBox;
            var mensajeTextBox = this.FindName("MensajeTextBox") as TextBox;

            if (string.IsNullOrWhiteSpace(nombreTextBox?.Text) ||
                string.IsNullOrWhiteSpace(emailTextBox?.Text) ||
                string.IsNullOrWhiteSpace(mensajeTextBox?.Text))
            {
                MessageBox.Show("Por favor, complete todos los campos del formulario.", 
                              "Campos incompletos", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                return;
            }

            // Aquí iría la lógica para enviar el mensaje
            MessageBox.Show("Gracias por su mensaje. Nos pondremos en contacto con usted pronto.", 
                          "Mensaje enviado", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Information);

            // Limpiar campos
            nombreTextBox.Text = string.Empty;
            emailTextBox.Text = string.Empty;
            mensajeTextBox.Text = string.Empty;
        }
    }

    public class GuiaRapida
    {
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
