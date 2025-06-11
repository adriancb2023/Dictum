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

namespace TFG_V0._01.Ventanas.SubVentanas
{
    public partial class EleccionBBDD : Window
    {
        #region 🎨 Variables y Recursos
        private readonly SupabaseAutentificacion _authService;
        private Storyboard fadeInStoryboard;
        private Storyboard shakeStoryboard;
        private Storyboard meshAnimStoryboard;

        private RadialGradientBrush mesh1Brush;
        private RadialGradientBrush mesh2Brush;
        private DrawingBrush meshGradientBrush;
        #endregion

        #region ⚡ Inicialización
        public EleccionBBDD()
        {
            InitializeComponent();
            this.Tag = MainWindow.isDarkTheme;
            ReadConfiguration();
            CargarIdioma(MainWindow.idioma);
            InitializeAnimations();
            CrearFondoAnimado();
            AplicarTema();
            BeginFadeInAnimation();
        }
        #endregion

        #region 📂 Configuración
        private void ReadConfiguration()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePathDesktop = System.IO.Path.Combine(desktopPath, "config.json");

                string filePath = filePathDesktop;

                if (!System.IO.File.Exists(filePath))
                    return;

                string json = System.IO.File.ReadAllText(filePath);
                var config = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuracion>(json);

                if (config != null)
                {
                    if (config.TipoBBDD != null)
                        BBDD.IsChecked = config.TipoBBDD.Value;
                    if (BBDD.IsChecked != true)
                    {
                        var localImgPath = "pack://application:,,,/TFG V0.01;component/Recursos/Iconos/localselect.png";
                        try
                        {
                            local.Source = new BitmapImage(new Uri(localImgPath, UriKind.Absolute));
                        }
                        catch
                        {
                            local.Source = null;
                        }
                    }
                    else
                    {
                        var supaImgPath = "pack://application:,,,/TFG V0.01;component/Recursos/Iconos/cloudselect.png";
                        try
                        {
                            supa.Source = new BitmapImage(new Uri(supaImgPath, UriKind.Absolute));
                        }
                        catch
                        {
                            supa.Source = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al leer la configuración: " + ex.Message);
            }
        }
        #endregion

        #region 🔄 Navegación
        private void iniciarModoAmind(object sender, RoutedEventArgs e)
        {
            MainWindow.tipoBBDD = BBDD.IsChecked.HasValue && BBDD.IsChecked.Value;

            var home = new Home();
            home.Show();
            this.Close();
        }
        #endregion

        #region 🌍 Gestión de Idiomas
        private void CargarIdioma(int idioma)
        {
            var idiomas = new (string SeleccionBBDD, string InfoSeleccion, string Local, string Nube, string BtnAceptar)[] {
        ("Selecciona el tipo de Base de Datos", "La base de datos seleccionada anteriormente está marcada con un icono ", "Test", "Nube", "Aceptar"),
        ("Select the type of Database", "The previously selected database is marked with an icon ", "Test", "Cloud", "Accept"),
        ("Selecciona el tipus de Base de Dades", "La base de dades seleccionada anteriorment està marcada amb una icona ", "Test", "Núvol", "Acceptar"),
        ("Selecciona o tipo de Base de Datos", "A base de datos seleccionada anteriormente está marcada cunha icona ", "Test", "Nube", "Aceptar"),
        ("Aukeratu Datu Base mota", "Aurrekoan hautatutako datu-basea ikono batekin markatuta dago ", "Test", "Hodeia", "Onartu")
    };

            if (idioma < 0 || idioma >= idiomas.Length)
                idioma = 0;

            var t = idiomas[idioma];

            txtSeleccionBBDD.Text = t.SeleccionBBDD;
            txtInfoSeleccion.Text = t.InfoSeleccion;
            txtLocal.Text = t.Local;
            txtNube.Text = t.Nube;
            btnAceptar.Content = t.BtnAceptar;
        }
        #endregion

        #region 🌓 Gestión de Tema
        private void AplicarTema()
        {
            this.Tag = MainWindow.isDarkTheme;
            var button = this.FindName("ThemeButton") as Button;
            var icon = button?.Template.FindName("ThemeIcon", button) as Image;
            var closeButton = this.FindName("CloseButton") as Button;

            if (MainWindow.isDarkTheme)
            {
                if (icon != null)
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/sol.png", UriKind.Relative));
         
                mesh1Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#d2cdc6");
                mesh1Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#08a693");
                mesh2Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#3a4d5f");
                mesh2Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#272c3f");
                OverlayDark.Visibility = Visibility.Visible;
                if (closeButton != null)
                    closeButton.Foreground = (Brush)this.FindResource("CloseButtonForegroundDark");
            }
            else
            {
                if (icon != null)
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna.png", UriKind.Relative));
         
                mesh1Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#de9cb8");
                mesh1Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#9dcde1");
                mesh2Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#dc8eb8");
                mesh2Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#98d3ec");
                OverlayDark.Visibility = Visibility.Collapsed;
                if (closeButton != null)
                    closeButton.Foreground = (Brush)this.FindResource("CloseButtonForegroundLight");
            }
            IniciarAnimacionMesh();
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.isDarkTheme = !MainWindow.isDarkTheme;
            AplicarTema();
            var fadeAnimation = CrearFadeAnimation(0.7, 0.9, 0.3, true);
            MainGrid.BeginAnimation(OpacityProperty, fadeAnimation);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            fadeOut.Completed += (s, args) => this.Close();
            this.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        #endregion

        #region 🎬 Animaciones y Efectos
        private DoubleAnimation CrearFadeAnimation(double from, double to, double durationSeconds, bool autoReverse = false) =>
            new()
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                AutoReverse = autoReverse
            };

        private void InitializeAnimations()
        {
            fadeInStoryboard = new Storyboard();
            var fadeIn = CrearFadeAnimation(0, 1, 0.5);
            Storyboard.SetTarget(fadeIn, this);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(nameof(Opacity)));
            fadeInStoryboard.Children.Add(fadeIn);
        }

        private void BeginFadeInAnimation()
        {
            this.Opacity = 0;
            fadeInStoryboard.Begin();
        }

        private void CrearFondoAnimado()
        {
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

            var drawingGroup = new DrawingGroup();
            drawingGroup.Children.Add(new GeometryDrawing(mesh1Brush, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
            drawingGroup.Children.Add(new GeometryDrawing(mesh2Brush, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
            meshGradientBrush = new DrawingBrush(drawingGroup) { Stretch = Stretch.Fill };
            MainGrid.Background = meshGradientBrush;
        }

        private void IniciarAnimacionMesh()
        {
            meshAnimStoryboard?.Stop();
            meshAnimStoryboard = new Storyboard();
            
            var anim1 = new PointAnimationUsingKeyFrames();
            anim1.KeyFrames.Add(new EasingPointKeyFrame(new Point(0.3, 0.3), KeyTime.FromTimeSpan(TimeSpan.Zero)));
            anim1.KeyFrames.Add(new EasingPointKeyFrame(new Point(0.7, 0.5), KeyTime.FromTimeSpan(TimeSpan.FromSeconds(4))) { EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } });
            anim1.KeyFrames.Add(new EasingPointKeyFrame(new Point(0.3, 0.3), KeyTime.FromTimeSpan(TimeSpan.FromSeconds(8))) { EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } });
            anim1.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTarget(anim1, mesh1Brush);
            Storyboard.SetTargetProperty(anim1, new PropertyPath(RadialGradientBrush.CenterProperty));
            meshAnimStoryboard.Children.Add(anim1);
       
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
        #endregion
    }
}
