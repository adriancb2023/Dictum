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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using TFG_V0._01.Helpers;
using System.Collections.ObjectModel;
using Mscc.GenerativeAI;

namespace TFG_V0._01.Ventanas
{
    /// <summary>
    /// Lógica de interacción para GPTChat.xaml
    /// </summary>
    public class ChatMessage
    {
        public required string Message { get; set; }
        public bool IsUser { get; set; }
    }

    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isUser = (bool)value;
            return isUser ? new SolidColorBrush(Color.FromRgb(0, 120, 215)) : new SolidColorBrush(Color.FromRgb(45, 45, 45));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isUser = (bool)value;
            return isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class GPTChat : Window
    {
        private static Storyboard meshAnimStoryboard;
        private static RadialGradientBrush mesh1Brush;
        private static RadialGradientBrush mesh2Brush;
        private DrawingBrush? _meshGradientBrush;
        private readonly string API_KEY = "AIzaSyCrnIeToA1TuahYRsr2jgP0HIvoeeCymUc";
        private readonly ObservableCollection<ChatMessage> messages;
        private GenerativeModel? generativeModel;

        static GPTChat()
        {
            meshAnimStoryboard = new Storyboard();
            mesh1Brush = new RadialGradientBrush();
            mesh2Brush = new RadialGradientBrush();
        }

        public GPTChat()
        {
            InitializeComponent();
            this.DataContext = this;

            // Inicializar colección de mensajes
            messages = new ObservableCollection<ChatMessage>();
            ChatMessages.ItemsSource = messages;

            // Configurar cliente de Google AI
            try
            {
                var googleAI = new GoogleAI(apiKey: API_KEY);
                generativeModel = googleAI.GenerativeModel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar Google AI: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                generativeModel = null;
            }

            InitializeAnimations();
            CrearFondoAnimado();
            AplicarModoSistema();
            BeginFadeInAnimation();

            navbar.ActualizarIdioma(MainWindow.idioma);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Aquí irá la lógica de carga inicial
        }

        private void InitializeAnimations()
        {
            // Inicializar animaciones si es necesario
        }

        private void CrearFondoAnimado()
        {
            // Crear los brushes
            mesh1Brush = new RadialGradientBrush();
            mesh1Brush.Center = new Point(0.3, 0.3);
            mesh1Brush.RadiusX = 0.5;
            mesh1Brush.RadiusY = 0.5;
            mesh1Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#de9cb8"), 0));
            mesh1Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#9dcde1"), 1));
            mesh1Brush.Freeze();
            mesh1Brush = mesh1Brush.Clone();

            mesh2Brush = new RadialGradientBrush();
            mesh2Brush.Center = new Point(0.7, 0.7);
            mesh2Brush.RadiusX = 0.6;
            mesh2Brush.RadiusY = 0.6;
            mesh2Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#dc8eb8"), 0));
            mesh2Brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#98d3ec"), 1));
            mesh2Brush.Freeze();
            mesh2Brush = mesh2Brush.Clone();

            // Crear el DrawingBrush
            var drawingGroup = new DrawingGroup();
            drawingGroup.Children.Add(new GeometryDrawing(mesh1Brush, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
            drawingGroup.Children.Add(new GeometryDrawing(mesh2Brush, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
            _meshGradientBrush = new DrawingBrush(drawingGroup) { Stretch = Stretch.Fill };
            ((Grid)this.Content).Background = _meshGradientBrush;
        }

        private void AplicarModoSistema()
        {
            this.Tag = MainWindow.isDarkTheme;
            var button = FindName("ThemeButton") as Button;
            var icon = button?.Template.FindName("ThemeIcon", button) as System.Windows.Controls.Image;

            // Cambiar fondo mesh gradient
            if (MainWindow.isDarkTheme)
            {
                if (icon != null)
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/sol.png", UriKind.Relative));
                // Colores mesh oscuro
                mesh1Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#8C7BFF");
                mesh1Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#08a693");
                mesh2Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#3a4d5f");
                mesh2Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#272c3f");
            }
            else
            {
                if (icon != null)
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna.png", UriKind.Relative));
                // Colores mesh claro
                mesh1Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#de9cb8");
                mesh1Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#9dcde1");
                mesh2Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#dc8eb8");
                mesh2Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#98d3ec");
            }

            navbar.ActualizarTema(MainWindow.isDarkTheme);
            IniciarAnimacionMesh();
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.isDarkTheme = !MainWindow.isDarkTheme;
            AplicarModoSistema();
            var fadeAnimation = CrearFadeAnimation(0.7, 0.9, 0.3, true);
            this.BeginAnimation(OpacityProperty, fadeAnimation);
        }

        private void BeginFadeInAnimation()
        {
            this.Opacity = 0;
            var fadeInAnimation = CrearFadeAnimation(0, 1, 0.5);
            this.BeginAnimation(OpacityProperty, fadeInAnimation);
        }

        private DoubleAnimation CrearFadeAnimation(double from, double to, double durationSeconds, bool autoReverse = false) =>
            new()
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                AutoReverse = autoReverse
            };

        private void IniciarAnimacionMesh()
        {
            // Detener si ya existe
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

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                await SendMessage();
                e.Handled = true;
            }
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageInput.Text) || generativeModel == null)
                return;

            var userMessage = MessageInput.Text;
            MessageInput.Clear();

            // Agregar mensaje del usuario
            messages.Add(new ChatMessage { Message = userMessage, IsUser = true });

            try
            {
                // Llamada directa con string, sin Prompt
                // Si la librería solo tiene método síncrono, usa Task.Run
                string aiResponse;
                if (generativeModel.GetType().GetMethod("GenerateContentAsync") != null)
                {
                    // Si existe GenerateContentAsync(string)
                    dynamic response = await ((dynamic)generativeModel).GenerateContentAsync(userMessage);
                    aiResponse = response.Text;
                }
                else if (generativeModel.GetType().GetMethod("GenerateContent") != null)
                {
                    // Si solo existe GenerateContent(string)
                    dynamic response = await Task.Run(() => ((dynamic)generativeModel).GenerateContent(userMessage));
                    aiResponse = response.Text;
                }
                else
                {
                    aiResponse = "No se encontró un método válido en GenerativeModel.";
                }

                // Agregar respuesta de la IA
                messages.Add(new ChatMessage { Message = aiResponse, IsUser = false });
            }
            catch (Exception ex)
            {
                messages.Add(new ChatMessage { Message = $"Error: {ex.Message}", IsUser = false });
            }

            // Scroll al último mensaje
            ChatScrollViewer.ScrollToBottom();
        }
    }
}