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
    /// <summary>
    /// Lógica de interacción para Ajustes.xaml
    /// </summary>
    public partial class Ajustes : Window
    {
        #region variables animacion
        private Storyboard fadeInStoryboard;
        private Storyboard shakeStoryboard;
        #endregion

        #region variables

        #endregion

        #region Inicializacion
        public Ajustes()
        {
            InitializeComponent();
            InitializeAnimations();
            AplicarModoSistema();
            ReadConfiguration();
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

        #region boton cambiar tema
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

                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString(
                    @"C:\Users\Harvie\Documents\TFG\V 0.1\TFG\TFG V0.01\Recursos\Background\oscuro\main.png") as ImageSource;

                CambiarIconosAClaros();
                CambiarTextosBlanco();
                backgroun_menu.Background = new SolidColorBrush(Color.FromArgb(48, 255, 255, 255)); // Fondo semitransparente
            }
            else
            {
                // Cambiar a modo claro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna.png", UriKind.Relative));
                }

                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString(
                    @"C:\Users\Harvie\Documents\TFG\V 0.1\TFG\TFG V0.01\Recursos\Background\claro\main.png") as ImageSource;

                CambiarIconosAOscuros();
                CambiarTextosNegro();
                backgroun_menu.Background = new SolidColorBrush(Color.FromArgb(48, 128, 128, 128)); // Gris semitransparente
            }
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
            // Cambia el fondo a tema oscuro
            backgroundFondo.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                new System.Uri("pack://application:,,,/TFG V0.01;component/Recursos/Background/oscuro/main.png"));
        }

        private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            // Cambia el fondo a tema claro
            backgroundFondo.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                new System.Uri("pack://application:,,,/TFG V0.01;component/Recursos/Background/claro/main.png"));
        }
        #endregion

        #region Creacion JSON Config
        private void GuardarConfig(object sender, RoutedEventArgs e)
        {
            bool isDarkMode = modoClaro.IsChecked.HasValue && modoClaro.IsChecked.Value;
            int idioma = LanguageComboBox.SelectedIndex; // Mejor usar SelectedIndex para el idioma
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
            //guardar en la carpeta de datos de la aplicacion
            //string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "config.json");
            string filePath = System.IO.Path.Combine(desktopPath, "config.json");
            System.IO.File.WriteAllText(filePath, json);

            MessageBox.Show("Configuración guardada correctamente en el escritorio.");
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
                        modoClaro.IsChecked = config.ModoOscuro.Value;

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

        //asadasda
    }
}
