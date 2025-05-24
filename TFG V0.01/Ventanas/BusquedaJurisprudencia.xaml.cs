using JurisprudenciaApi.Controllers;
using JurisprudenciaApi.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // Para ObservableCollection
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

namespace TFG_V0._01.Ventanas
{
    /// <summary>
    /// Lógica de interacción para BusquedaJurisprudencia.xaml
    /// </summary>
    public partial class BusquedaJurisprudencia : Window
    {
        #region variables animacion
        private Storyboard fadeInStoryboard;
        private Storyboard shakeStoryboard;
        #endregion

        #region Variables API
        private static readonly HttpClient client = new HttpClient();
        private const string ApiBaseUrl = "http://localhost:5146";
        private static readonly Dictionary<string, string> TipoOrganoMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Tribunal Supremo", "11|12|13|14|15|16" },
            { "Tribunal Supremo. Sala de lo Civil", "11" },
            { "Tribunal Supremo. Sala de lo Penal", "12" },
            { "Tribunal Supremo. Sala de lo Contencioso", "13" },
            { "Tribunal Supremo. Sala de lo Social", "14" },
            { "Tribunal Supremo. Sala de lo Militar", "15" },
            { "Tribunal Supremo. Sala de lo Especial", "16" },
            { "Audiencia Nacional", "22|2264|23|24|25|26|27|28|29" },
            { "Audiencia Nacional. Sala de lo Penal", "22" },
            { "Sala de Apelación de la Audiencia Nacional", "2264" },
            { "Audiencia Nacional. Sala de lo Contencioso", "23" },
            { "Audiencia Nacional. Sala de lo Social", "24" },
            { "Audiencia Nacional. Juzgados Centrales de Instrucción", "27" },
            { "Audiencia Nacional. Juzgado Central de Menores", "26" },
            { "Audiencia Nacional. Juzgado Central de Vigilancia Penitenciaria", "25" },
            { "Audiencia Nacional. Juzgados Centrales de lo Contencioso", "29" },
            { "Audiencia Nacional. Juzgados Centrales de lo Penal", "28" },
            { "Tribunal Superior de Justicia", "31|31201202|33|34" },
            { "Tribunal Superior de Justicia. Sala de lo Civil y Penal", "31" },
            { "Sección de Apelación Penal. TSJ Sala de lo Civil y Penal", "31201202" },
            { "Tribunal Superior de Justicia. Sala de lo Contencioso", "33" },
            { "Tribunal Superior de Justicia. Sala de lo Social", "34" },
            { "Audiencia Provincial", "37" },
            { "Audiencia Provincial. Tribunal Jurado", "38" },
            { "Tribunal de Marca de la UE", "1001" },
            { "Juzgado de Primera Instancia", "42" },
            { "Juzgado de Instrucción", "43" },
            { "Juzgado de lo Contencioso Administrativo", "45" },
            { "Juzgado de Menores", "53" },
            { "Juzgado de Primera Instancia e Instrucción", "41" },
            { "Juzgado de lo Mercantil", "47" },
            { "Juzgados de Marca de la UE", "1002" },
            { "Juzgado de lo Penal", "51" },
            { "Juzgado de lo Social", "44" },
            { "Juzgado de Vigilancia Penitenciaria", "52" },
            { "Juzgado de Violencia sobre la Mujer", "48" },
            { "Tribunal Militar Territorial", "83" },
            { "Tribunal Militar Central", "85" },
            { "Consejo Supremo de Justicia Militar", "75" },
            { "Audiencia Territorial", "36" }
        };
        public ObservableCollection<JurisprudenciaResult> ResultadosBusqueda { get; set; }
        #endregion

        #region Nuevas variables para paginación
        private int _paginaActual = 1;
        private const int RegistrosPorPaginaConst = 10; // Cuántos cargar por página
        private bool _isLoading = false; // Para evitar múltiples llamadas simultáneas
        private JurisprudenciaSearchParameters _lastSearchParameters; // Para recordar los filtros al cargar más
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
                navbar.ActualizarTema(true);
            }
            else
            {
                // Aplicar modo claro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna.png", UriKind.Relative));
                }
                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/TFG V0.01;component/Recursos/Background/claro/main.png") as ImageSource;
                navbar.ActualizarTema(false);
            }
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
                    "pack://application:,,,/TFG V0.01;component/Recursos/Background/oscuro/main.png") as ImageSource;

                navbar.ActualizarTema(true);
            }
            else
            {
                // Cambiar a modo claro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna.png", UriKind.Relative));
                }

                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString(
                    "pack://application:,,,/TFG V0.01;component/Recursos/Background/claro/main.png") as ImageSource;

                navbar.ActualizarTema(false);
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

        private void LegislacionTextBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var legislacionWindow = new Legislacion();
            legislacionWindow.ShowDialog();
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

        #region API Interaction
        private async void BuscarButton_Click(object sender, RoutedEventArgs e)
        {
            _paginaActual = 1; // Resetear a la primera página para una nueva búsqueda
            ResultadosBusqueda.Clear(); // Limpiar resultados anteriores
            CargarMasButton.Visibility = Visibility.Collapsed; // Ocultar botón al iniciar nueva búsqueda

            _lastSearchParameters = GetSearchParametersFromUI(); // Guardar los parámetros actuales
            _lastSearchParameters.PaginaActual = _paginaActual;
            _lastSearchParameters.RegistrosPorPagina = RegistrosPorPaginaConst;

            await RealizarBusquedaAsync(_lastSearchParameters);
        }

        private async void CargarMasButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoading || _lastSearchParameters == null) return; // No cargar si ya está cargando o no hay búsqueda previa

            _paginaActual++;
            _lastSearchParameters.PaginaActual = _paginaActual;
            // _lastSearchParameters.RegistrosPorPagina ya está establecido

            await RealizarBusquedaAsync(_lastSearchParameters, esCargaAdicional: true);
        }

        private async Task RealizarBusquedaAsync(JurisprudenciaSearchParameters parameters, bool esCargaAdicional = false)
        {
            if (_isLoading) return;

            _isLoading = true;
            // Opcional: Deshabilitar botones para evitar clics múltiples
            BuscarButton.IsEnabled = false;
            CargarMasButton.IsEnabled = false;
            // Opcional: Mostrar un indicador de carga
            // ResultadosTextBlock.Text = "Cargando..."; 
            // ResultadosTextBlock.Visibility = Visibility.Visible;


            try
            {
                string jsonParameters = JsonSerializer.Serialize(parameters, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                var content = new StringContent(jsonParameters, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/api/Jurisprudencia/search", content);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    List<JurisprudenciaResult>? nuevosResultados = JsonSerializer.Deserialize<List<JurisprudenciaResult>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (nuevosResultados != null && nuevosResultados.Any())
                    {
                        foreach (var result in nuevosResultados)
                        {
                            ResultadosBusqueda.Add(result);
                        }
                        // Mostrar el botón "Cargar Más" si la API devolvió el número completo de registros esperados
                        CargarMasButton.Visibility = nuevosResultados.Count == RegistrosPorPaginaConst ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else
                    {
                        CargarMasButton.Visibility = Visibility.Collapsed; // No hay más resultados
                        if (esCargaAdicional)
                        {
                            MessageBox.Show("No hay más resultados para cargar.", "Fin de los resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        // Si no es carga adicional y no hay resultados, el DataTrigger del ResultadosTextBlock se encargará.
                    }
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    string errorMsg = $"Error al buscar: {response.StatusCode}\n{errorContent}";
                    MessageBox.Show(errorMsg, "Error API", MessageBoxButton.OK, MessageBoxImage.Error);
                    CargarMasButton.Visibility = Visibility.Collapsed;
                }
            }
            catch (HttpRequestException httpEx)
            {
                string errorMsg = $"Error de conexión: Verifique que la API ({ApiBaseUrl}) esté ejecutándose.\n{httpEx.Message}";
                MessageBox.Show(errorMsg, "Error Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                CargarMasButton.Visibility = Visibility.Collapsed;
            }
            catch (JsonException jsonEx)
            {
                string errorMsg = "Error al procesar la respuesta de la API:\n" + jsonEx.Message;
                MessageBox.Show(errorMsg, "Error Deserialización", MessageBoxButton.OK, MessageBoxImage.Error);
                CargarMasButton.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                string errorMsg = "Ocurrió un error inesperado:\n" + ex.Message;
                MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CargarMasButton.Visibility = Visibility.Collapsed;
            }
            finally
            {
                _isLoading = false;
                BuscarButton.IsEnabled = true;
                CargarMasButton.IsEnabled = true;
                // Opcional: Ocultar indicador de carga
                // if (ResultadosBusqueda.Any()) ResultadosTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private JurisprudenciaSearchParameters GetSearchParametersFromUI()
        {
            var parameters = new JurisprudenciaSearchParameters
            {
                // NO PONGAS PaginaActual NI RegistrosPorPagina AQUÍ
                // Se asignarán en RealizarBusquedaAsync o BuscarButton_Click
            };

            parameters.Jurisdiccion = (JurisdiccionComboBox.SelectedItem as string == "Todos" || JurisdiccionComboBox.SelectedItem == null) ? null : JurisdiccionComboBox.SelectedItem as string;
            if (TipoResolucionComboBox.SelectedItem is string tipoResValue && !string.IsNullOrEmpty(tipoResValue) && tipoResValue != "Todos")
            {
                parameters.TiposResolucion = new List<string> { tipoResValue };
            }
            if (OrganoJudicialComboBox.SelectedItem is string orgValue && !string.IsNullOrEmpty(orgValue) && orgValue != "Todos")
            {
                parameters.OrganosJudiciales = new List<string> { orgValue }; // Enviar el nombre, la API lo mapea
            }
            if (LocalizacionComboBox.SelectedItem is string locValue && !string.IsNullOrEmpty(locValue) && locValue != "Todos")
            {
                parameters.Localizaciones = new List<string> { locValue };
            }
            if (IdiomaComboBox.SelectedItem is ComboBoxItem iItem && iItem.Content != null)
            {
                string? idiomaValue = iItem.Content.ToString();
                parameters.Idioma = (idiomaValue == "Todos" || string.IsNullOrEmpty(idiomaValue)) ? null : idiomaValue;
            }

            parameters.Roj = string.IsNullOrWhiteSpace(NumeroRojTextBox.Text) ? null : NumeroRojTextBox.Text;
            parameters.Ecli = string.IsNullOrWhiteSpace(EcliTextBox.Text) ? null : EcliTextBox.Text;
            parameters.NumeroResolucion = string.IsNullOrWhiteSpace(NumeroResolucionTextBox.Text) ? null : NumeroResolucionTextBox.Text;
            parameters.NumeroRecurso = string.IsNullOrWhiteSpace(NumeroRecursoTextBox.Text) ? null : NumeroRecursoTextBox.Text;
            parameters.Ponente = string.IsNullOrWhiteSpace(PonenteTextBox.Text) ? null : PonenteTextBox.Text;
            parameters.Seccion = string.IsNullOrWhiteSpace(SeccionTextBox.Text) ? null : SeccionTextBox.Text;
            parameters.Legislacion = string.IsNullOrWhiteSpace(LegislacionTextBox.Text) ? null : LegislacionTextBox.Text; // Asumiendo que es un TextBox
            parameters.FechaDesde = FechaDesdeDatePicker.SelectedDate;
            parameters.FechaHasta = FechaHastaDatePicker.SelectedDate;


            return parameters;
        }

        private async Task CargarDatosInicialesAsync()
        {
            try
            {
                // Llamada a la API para obtener los datos iniciales
                HttpResponseMessage response = await client.GetAsync($"{ApiBaseUrl}/api/Jurisprudencia/initialData");
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var initialData = JsonSerializer.Deserialize<InitialDataResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Añadir "Todos" si no viene de la API y es necesario
                initialData.Jurisdicciones.Insert(0, "Todos");
                initialData.TiposResolucion.Insert(0, "Todos");
                initialData.OrganosJudiciales.Insert(0, "Todos");
                initialData.Localizaciones.Insert(0, "Todos");

                // Asignar los datos a los ComboBox
                JurisdiccionComboBox.ItemsSource = initialData.Jurisdicciones;
                TipoResolucionComboBox.ItemsSource = initialData.TiposResolucion;
                OrganoJudicialComboBox.ItemsSource = initialData.OrganosJudiciales;
                LocalizacionComboBox.ItemsSource = initialData.Localizaciones;

                // Seleccionar "Todos" por defecto
                JurisdiccionComboBox.SelectedItem = "Todos";
                TipoResolucionComboBox.SelectedItem = "Todos";
                OrganoJudicialComboBox.SelectedItem = "Todos";
                LocalizacionComboBox.SelectedItem = "Todos";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos iniciales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public BusquedaJurisprudencia()
        {
            InitializeComponent();
            ResultadosBusqueda = new ObservableCollection<JurisprudenciaResult>(); // Inicializa la colección
            this.DataContext = this; // Establece el DataContext para que los bindings del XAML funcionen

            InitializeAnimations();
            AplicarModoSistema();

            if (client.BaseAddress == null)
            {
                client.BaseAddress = new Uri(ApiBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            // Cargar datos iniciales
            _ = CargarDatosInicialesAsync();
        }
        private void VerDocumentoButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string url)
            {
                if (!string.IsNullOrWhiteSpace(url))
                {
                    try
                    {
                        // Abrir la URL en el navegador por defecto
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"No se pudo abrir el documento: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("La URL del documento no está disponible.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        #endregion

    }
}
