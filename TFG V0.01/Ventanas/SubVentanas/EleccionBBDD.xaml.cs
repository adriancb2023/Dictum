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

namespace TFG_V0._01.Ventanas.SubVentanas
{
    /// <summary>
    /// Lógica de interacción para EleccionBBDD.xaml
    /// </summary>
    public partial class EleccionBBDD : Window
    {
        public EleccionBBDD()
        {
            InitializeComponent();
            ReadConfiguration();
            CargarIdioma(MainWindow.idioma);
            AplicarModoSistema();
        }

        #region ⌛ Leer Configuración
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
                        // Ruta relativa al recurso local
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
                        // Ruta relativa al recurso de nube
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

        #region 🔄 Eventos
        private void iniciarModoAmind(object sender, RoutedEventArgs e)
        {
            MainWindow.tipoBBDD = BBDD.IsChecked.HasValue && BBDD.IsChecked.Value;

            var home = new Home();
            home.Show();
            this.Close();


        }
        #endregion

        #region 🈳 Idioma
        private void CargarIdioma(int idioma)
        {
            var idiomas = new (string SeleccionBBDD, string InfoSeleccion, string Local, string Nube, string BtnAceptar)[] {
        ("Selecciona el tipo de Base de Datos", "La base de datos seleccionada anteriormente está marcada con un icono ", "Local", "Nube", "Aceptar"),
        ("Select the type of Database", "The previously selected database is marked with an icon ", "Local", "Cloud", "Accept"),
        ("Selecciona el tipus de Base de Dades", "La base de dades seleccionada anteriorment està marcada amb una icona ", "Local", "Núvol", "Acceptar"),
        ("Selecciona o tipo de Base de Datos", "A base de datos seleccionada anteriormente está marcada cunha icona ", "Local", "Nube", "Aceptar"),
        ("Aukeratu Datu Base mota", "Aurrekoan hautatutako datu-basea ikono batekin markatuta dago ", "Lokala", "Hodeia", "Onartu")
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

        #region 🌓 Aplicar modo oscuro/claro cargado por sistema
        private void AplicarModoSistema()
        {
            // Buscar el botón de tema y su icono
            if (FindName("ThemeButton") is Button button)
            {
                if (button.Template?.FindName("ThemeIcon", button) is Image icon)
                {
                    icon.Source = new BitmapImage(new Uri(GetIconoTema(), UriKind.Relative));
                }
            }

            // Cambiar el fondo principal
            if (backgroundFondo != null)
            {
                var imageSource = new ImageSourceConverter().ConvertFromString(GetBackgroundPath()) as ImageSource;
                if (imageSource != null)
                    backgroundFondo.ImageSource = imageSource;
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

        private DoubleAnimation CrearFadeAnimation(double from, double to, double durationSeconds, bool autoReverse = false) =>
            new()
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                AutoReverse = autoReverse
            };
        #endregion
    }
}
