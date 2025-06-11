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

namespace TFG_V0._01.Ventanas
{
    public partial class Ajustes : Window
    {
        #region Variables animación y fondo
        private Storyboard fadeInStoryboard;
        private Storyboard shakeStoryboard;
        private Storyboard meshAnimStoryboard;
        private RadialGradientBrush mesh1Brush;
        private RadialGradientBrush mesh2Brush;
        private DrawingBrush meshGradientBrush;
        private bool fondoAnimadoInicializado = false;
        #endregion

        public bool IsAdmin { get; set; }

        #region variables

        #endregion

        #region Inicialización
        public Ajustes()
        {
            InitializeComponent();
            IsAdmin = MainWindow.isAdmin;
            this.DataContext = this;
            InitializeAnimations();
            CrearFondoAnimado();
            fondoAnimadoInicializado = true;
            AplicarTemaMesh();
            IniciarAnimacionMesh();
            AplicarModoSistema();
            ReadConfiguration();
            CargarIdioma(MainWindow.idioma);
        }
        #endregion

        #region Fondo mesh gradient animado igual que Home
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
            meshGradientBrush = new DrawingBrush(drawingGroup) { Stretch = Stretch.Fill };
            ((Grid)this.Content).Background = meshGradientBrush;
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

        #region Aplicar modo oscuro/claro cargado por sistema
        private void AplicarModoSistema()
        {
            if (!fondoAnimadoInicializado) CrearFondoAnimado();
            this.Tag = MainWindow.isDarkTheme;
            AplicarTemaMesh();
            navbar.ActualizarTema(MainWindow.isDarkTheme);
        }
        #endregion

        #region Cambiar tema
        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!fondoAnimadoInicializado) CrearFondoAnimado();
            MainWindow.isDarkTheme = !MainWindow.isDarkTheme;
            this.Tag = MainWindow.isDarkTheme;
            AplicarTemaMesh();
            navbar.ActualizarTema(MainWindow.isDarkTheme);
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

        #region Switch
        private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (!fondoAnimadoInicializado) CrearFondoAnimado();
            MainWindow.isDarkTheme = true;
            this.Tag = true;
            AplicarTemaMesh();
            navbar.ActualizarTema(true);
        }

        private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!fondoAnimadoInicializado) CrearFondoAnimado();
            MainWindow.isDarkTheme = false;
            this.Tag = false;
            AplicarTemaMesh();
            navbar.ActualizarTema(false);
        }
        #endregion

        #region Creacion JSON Config
        private void GuardarConfig(object sender, RoutedEventArgs e)
        {
            bool isDarkMode = modoClaro.IsChecked.HasValue && modoClaro.IsChecked.Value;
            int idioma = LanguageComboBox.SelectedIndex;
            bool bbdd = BBDD.IsChecked.HasValue && BBDD.IsChecked.Value;

            // Crear el objeto de configuración
            Configuracion config = new Configuracion
            {
                ModoOscuro = isDarkMode,
                Idioma = idioma,
                TipoBBDD = bbdd
            };

            // Serializar a JSON
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);

            // Guardar en el escritorio
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = System.IO.Path.Combine(desktopPath, "config.json");
            System.IO.File.WriteAllText(filePath, json);

            MessageBox.Show("Configuración guardada correctamente en el escritorio.");
            MainWindow.isDarkTheme = isDarkMode;
            MainWindow.idioma = idioma;
            MainWindow.tipoBBDD = bbdd;

            AplicarTemaMesh();
            navbar.ActualizarTema(MainWindow.isDarkTheme);
            CargarIdioma(MainWindow.idioma);


        }
        #endregion

        #region Archivo Config Leido
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
                    if (config.ModoOscuro != null)
                    {
                        modoClaro.IsChecked = config.ModoOscuro.Value;
                        MainWindow.isDarkTheme = config.ModoOscuro.Value;
                        this.Tag = config.ModoOscuro.Value;
                        AplicarTemaMesh();
                    }

                    if (config.Idioma.HasValue && config.Idioma.Value >= 0 && config.Idioma.Value < LanguageComboBox.Items.Count)
                        LanguageComboBox.SelectedIndex = config.Idioma.Value;

                    if (config.TipoBBDD != null)
                        BBDD.IsChecked = config.TipoBBDD.Value;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al leer la configuración: " + ex.Message);
            }
        }
        #endregion

        #region 🌍 Gestión de Idiomas
        private void CargarIdioma(int idioma)
        {
            var idiomas = new (string Titulo, string Apariencia, string TemaOscuro, string CambiarTema, string Idioma, string BaseDatos, string TipoBBDD, string EligeBBDD, string Guardar, string Cancelar)[] {
                ("Ajustes", "Apariencia", "Tema Oscuro", "Cambiar entre tema claro y oscuro", "Idioma", "Base de Datos", "Tipo de Base de Datos", "Elige dónde almacenar tus datos", "Guardar", "Cancelar"),
                ("Settings", "Appearance", "Dark Theme", "Switch between light and dark theme", "Language", "Database", "Database Type", "Choose where to store your data", "Save", "Cancel"),
                ("Ajustos", "Aparença", "Tema Fosc", "Canvia entre tema clar i fosc", "Idioma", "Base de Dades", "Tipus de Base de Dades", "Tria on desar les teves dades", "Desar", "Cancel·lar"),
                ("Axustes", "Apariencia", "Tema escuro", "Cambiar entre tema claro e escuro", "Idioma", "Base de Datos", "Tipo de Base de Datos", "Elixe onde gardar os teus datos", "Gardar", "Cancelar"),
                ("Ezarpenak", "Itxura", "Gaueko gaia", "Aldatu argi eta ilun moduen artean", "Hizkuntza", "Datu-basea", "Datu-base mota", "Aukeratu datuak non gorde", "Gorde", "Utzi")
            };

            if (idioma < 0 || idioma >= idiomas.Length)
                idioma = 0;

            var t = idiomas[idioma];

            // Asigna los textos a los controles
            TituloAjustes.Text = t.Titulo;
            txtApariencia.Text = t.Apariencia;
            txtTemaOscuro.Text = t.TemaOscuro;
            txtCambiarTema.Text = t.CambiarTema;
            txtIdioma.Text = t.Idioma;
            txtBaseDatos.Text = t.BaseDatos;
            txtTipoBBDD.Text = t.TipoBBDD;
            txtEligeBBDD.Text = t.EligeBBDD;
            btnGuardar.Content = t.Guardar;
            btnCancelar.Content = t.Cancelar;
        }
        #endregion

    }
}
