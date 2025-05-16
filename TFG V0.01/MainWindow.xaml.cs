using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TFG_V0._01.Ventanas;

namespace TFG_V0._01
{
    public partial class MainWindow : Window
    {
        #region Campos
        private DispatcherTimer progressTimer = null!;
        private DateTime startTime;
        private readonly TimeSpan duration = TimeSpan.FromSeconds(2.0);
        public static bool isDarkTheme;
        public static bool tipoBBDD;
        public static int idioma;
        #endregion

        #region Constructor
        public MainWindow()
        {
            DetectarModo();
            ReadConfiguration();
            InitializeComponent();
            cargarIdioma(idioma);
            StartLoadingAnimation();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // No necesitamos aplicar tema ya que el fondo es translúcido
            // y queremos que se vea el fondo del escritorio
        }
        #endregion

        #region Animacion de carga

        #region Animación de Carga
        private void StartLoadingAnimation()
        {
            startTime = DateTime.Now;
            progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60fps para animación suave
            };
            progressTimer.Tick += UpdateProgressArc!;
            progressTimer.Start();
        }
        #endregion

        #region Actualización del Progreso
        private void UpdateProgressArc(object? sender, EventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - startTime;
            double progress = Math.Min(elapsed.TotalMilliseconds / duration.TotalMilliseconds, 1.0);
            double angle = progress * 360;

            UpdateArc(angle);

            // Actualizar texto de porcentaje
            int percentage = (int)(progress * 100);
            PercentageText.Text = $"{percentage}%";

            if (progress >= 1.0)
            {
                progressTimer.Stop();

                // Animación de finalización
                DoubleAnimation fadeOut = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = TimeSpan.FromSeconds(0.5)
                };

                fadeOut.Completed += (s, args) => OpenNewWindow();
                this.BeginAnimation(OpacityProperty, fadeOut);
            }
        }
        #endregion

        #region Actualización del Arco
        private void UpdateArc(double angle)
        {
            double radius = 90;
            double centerX = 100;
            double centerY = 100;

            double startAngle = -90; // Comenzar desde arriba
            double endAngle = startAngle + angle;

            double startRadians = (Math.PI / 180) * startAngle;
            double endRadians = (Math.PI / 180) * endAngle;

            double startX = centerX + radius * Math.Cos(startRadians);
            double startY = centerY + radius * Math.Sin(startRadians);

            double endX = centerX + radius * Math.Cos(endRadians);
            double endY = centerY + radius * Math.Sin(endRadians);

            // Actualizar la figura del arco
            ProgressFigure.StartPoint = new Point(startX, startY);
            ProgressArc.Point = new Point(endX, endY);
            ProgressArc.IsLargeArc = angle > 180;
        }
        #endregion

        #endregion

        #region Abrir Nueva Ventana
        private void OpenNewWindow()
        {
            Login login = new Login
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            login.Show();
            this.Close();
        }
        #endregion

        #region detectar modo claro/oscuro del sistema
        private void DetectarModo()
        {
            var theme = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1);
            if (theme is int themeValue && themeValue == 0)
            {
                isDarkTheme = true;
            }
            else
            {
                isDarkTheme = false;
            }
        }
        #endregion

        #region Eventos de UI
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Lectura de Configuración
        private void ReadConfiguration()
        {
            // Valores por defecto
            isDarkTheme = true;    // Modo oscuro
            idioma = 0;            // Español 
            tipoBBDD = true;       // Nube

            try
            {
                // Ruta en el escritorio
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePathDesktop = System.IO.Path.Combine(desktopPath, "config.json");

                // Usa solo una de las rutas para leer el archivo de configuración:
                string filePath = filePathDesktop;

                if (!System.IO.File.Exists(filePath))
                    return; // No hay configuración guardada, se mantienen los valores por defecto

                string json = System.IO.File.ReadAllText(filePath);
                var config = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuracion>(json);

                if (config != null)
                {
                    if (config.ModoOscuro != null)
                        isDarkTheme = config.ModoOscuro.Value;

                    if (config.Idioma != null)
                        idioma = config.Idioma.Value;

                    if (config.TipoBBDD != null)
                        tipoBBDD = config.TipoBBDD.Value;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al leer la configuración: " + ex.Message);
            }
        }
        #endregion

        #region Idioma
        private void cargarIdioma(int idioma)
        {
            switch (idioma)
            {
                case 0:
                    cargando.Text = "Cargando";
                    break;
                case 1:
                    cargando.Text = "Loading";
                    break;
                case 2:
                    cargando.Text = "Carregant";
                    break;
                case 3:
                    cargando.Text = "Cargando";
                    break;
                case 4:
                    cargando.Text = "Kargatzen";
                    break;
                default:
                    cargando.Text = "Cargando";
                    break;
            }
        }
        #endregion
    }
}