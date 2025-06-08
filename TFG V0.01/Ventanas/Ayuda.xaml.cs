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
using System.Diagnostics;
using System.Net.Mail;
using System.Windows.Forms;

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
            CargarIdioma(MainWindow.idioma); // Cargar el idioma
        }
        #endregion

        #region Aplicar modo oscuro/claro cargado por sistema
        private void AplicarModoSistema()
        {
            this.Tag = MainWindow.isDarkTheme;
            AplicarTemaMesh(); // Aplicar tema al mesh gradient
            navbar.ActualizarTema(MainWindow.isDarkTheme);
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
            MainGrid.Background = meshGradientBrush;
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
            if (string.IsNullOrWhiteSpace(NombreTextBox?.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox?.Text) ||
                string.IsNullOrWhiteSpace(MensajeTextBox?.Text))
            {
                MessageBox.Show("Por favor, complete todos los campos del formulario.", 
                              "Campos incompletos", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Configuración de contactos
                string numeroWhatsApp = "34612345678"; // Número de ejemplo de WhatsApp Business
                string emailDestino = "soporte@tfgapp.com";

                // Preparar mensaje para WhatsApp
                string mensajeWhatsApp = $"Nombre: {NombreTextBox.Text}\n\nMensaje:\n{MensajeTextBox.Text}";
                string mensajeCodificado = Uri.EscapeDataString(mensajeWhatsApp);
                string urlWhatsApp = $"https://wa.me/{numeroWhatsApp}?text={mensajeCodificado}";

                // Preparar mensaje para email
                string asunto = Uri.EscapeDataString($"Mensaje de contacto de {NombreTextBox.Text}");
                string cuerpo = Uri.EscapeDataString($"Nombre: {NombreTextBox.Text}\nEmail: {EmailTextBox.Text}\n\nMensaje:\n{MensajeTextBox.Text}");
                string mailtoUrl = $"mailto:{emailDestino}?subject={asunto}&body={cuerpo}";

                // Abrir WhatsApp
                Process.Start(new ProcessStartInfo
                {
                    FileName = urlWhatsApp,
                    UseShellExecute = true
                });

                // Esperar un momento antes de abrir el correo
                System.Threading.Thread.Sleep(1000);

                // Abrir el cliente de correo
                Process.Start(new ProcessStartInfo
                {
                    FileName = mailtoUrl,
                    UseShellExecute = true
                });

                // Limpiar campos después de enviar
                NombreTextBox.Text = string.Empty;
                EmailTextBox.Text = string.Empty;
                MensajeTextBox.Text = string.Empty;

                MessageBox.Show("Se han abierto WhatsApp y el cliente de correo para enviar su mensaje.", 
                              "Mensaje preparado", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar el mensaje: {ex.Message}", 
                              "Error", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }

        #region 🌍 Gestión de Idiomas
        private void CargarIdioma(int idioma)
        {
            var idiomas = new (string Titulo, string PreguntasFrecuentes, string GuiasRapidasTexto, string ConsejosPracticas,
                string OrganizacionEficiente, string OptimizacionUso, string NecesitasAyuda, string Contacto,
                string EnvianosMensaje, string TuNombre, string TuEmail, string TuMensaje, string EnviarMensaje,
                string ComoAnadirCaso, string ComoGestionarDocumentos, string ComoGestionarClientes,
                string ComoFuncionaCalendario, string ComoGestionarTareas, string ComoCambiarTema,
                string RespuestaAnadirCaso, string RespuestaGestionarDocumentos, string RespuestaGestionarClientes,
                string RespuestaCalendario, string RespuestaTareas, string RespuestaCambiarTema,
                string ConsejoNombresCasos, string ConsejoEtiquetas, string ConsejoActualizarEstado,
                string ConsejoOrganizarDocumentos, string ConsejoRecordatorios, string ConsejoFiltros,
                string ConsejoInfoClientes, string ConsejoDashboard, string ConsejoCalendario,
                string ConsejoCopiasSeguridad, string EmailContacto, string TelefonoContacto,
                string HorarioContacto, string Copyright, string Version,
                string GuiaCasosTitulo, string GuiaCasosDescripcion,
                string GuiaDocumentosTitulo, string GuiaDocumentosDescripcion,
                string GuiaCalendarioTitulo, string GuiaCalendarioDescripcion,
                string GuiaClientesTitulo, string GuiaClientesDescripcion,
                string GuiaTareasTitulo, string GuiaTareasDescripcion,
                string GuiaDashboardTitulo, string GuiaDashboardDescripcion)[] 
            {
                // Español
                ("Ayuda", "Preguntas Frecuentes", "Guías Rápidas", "Consejos y Mejores Prácticas",
                "Organización Eficiente", "Optimización del Uso", "¿Necesitas más ayuda?", "Contacto",
                "Envíanos un mensaje", "Tu nombre", "Tu email", "Tu mensaje", "Enviar mensaje",
                "¿Cómo puedo añadir un nuevo caso?", "¿Cómo gestiono los documentos?", "¿Cómo gestiono los clientes?",
                "¿Cómo funciona el calendario y las citas?", "¿Cómo gestiono las tareas pendientes?", "¿Cómo cambio entre modo claro y oscuro?",
                "Para añadir un nuevo caso, haz clic en el botón '+' en la barra superior de la ventana principal. Se abrirá un panel donde podrás introducir los detalles del caso, incluyendo título, descripción, cliente asociado y tipo de caso. También podrás establecer el estado inicial del caso y añadir documentos relacionados.",
                "En la sección de Documentos puedes arrastrar y soltar archivos para subirlos, o usar el botón de subida. Los documentos se organizarán automáticamente por caso y tipo (PDF, imágenes, videos, audios y otros). Puedes filtrar los documentos por cliente, caso, fecha y tipo de archivo. También puedes previsualizar, descargar o eliminar documentos según tus necesidades.",
                "En la sección de Clientes puedes ver un listado de todos tus clientes. Puedes añadir nuevos clientes con sus datos de contacto, ver su historial de casos, documentos asociados y casos activos. También puedes editar la información de los clientes y gestionar su documentación personal.",
                "El calendario te permite gestionar todas tus citas y eventos. Puedes añadir nuevos eventos asociados a casos específicos, establecer recordatorios y ver un resumen de las próximas actividades. Los eventos se pueden filtrar por caso y estado, y puedes recibir notificaciones de próximas citas.",
                "En el panel de inicio puedes ver tus tareas pendientes. Cada tarea puede estar asociada a un caso específico y tiene un estado (Pendiente, En progreso, Finalizado). Puedes crear nuevas tareas, actualizar su estado y establecer fechas límite. Las tareas pendientes se muestran en el dashboard para un seguimiento rápido.",
                "Puedes cambiar entre los modos claro y oscuro usando el botón de tema ubicado en la esquina inferior derecha de la aplicación. El modo se guardará automáticamente y se aplicará en todas las ventanas de la aplicación.",
                "• Mantén los nombres de los casos claros y descriptivos", "• Utiliza etiquetas y estados para categorizar casos",
                "• Actualiza regularmente el estado de tus casos", "• Organiza los documentos por tipo y fecha",
                "• Establece recordatorios para fechas importantes", "• Utiliza los filtros para encontrar información rápidamente",
                "• Mantén actualizada la información de los clientes", "• Revisa regularmente el dashboard para tareas pendientes",
                "• Utiliza el calendario para planificar citas", "• Realiza copias de seguridad de documentos importantes",
                "Email: soporte@tfgapp.com", "Teléfono: +34 900 123 456", "Horario: L-V 11:00 - 14:00",
                "© 2025 TFG", "Versión: ",
                // Guías Rápidas Español
                "Gestión de Casos", "Aprende a crear, editar y gestionar casos eficientemente. Organiza tus casos por estado, tipo y cliente para un mejor seguimiento.",
                "Gestión de Documentos", "Descubre cómo organizar y gestionar tus documentos legales. Sube, categoriza y comparte documentos de forma segura.",
                "Calendario y Citas", "Gestiona tu agenda de citas y eventos. Establece recordatorios y mantén un control de todas tus actividades.",
                "Gestión de Clientes", "Administra la información de tus clientes, su historial de casos y documentación personal de forma centralizada.",
                "Tareas y Seguimiento", "Organiza tus tareas pendientes, establece prioridades y realiza un seguimiento efectivo de tus actividades diarias.",
                "Dashboard y Estadísticas", "Utiliza el panel de control para tener una visión general de tus casos activos, tareas pendientes y próximas citas."),

                // Inglés
                ("Help", "Frequently Asked Questions", "Quick Guides", "Tips and Best Practices",
                "Efficient Organization", "Usage Optimization", "Need more help?", "Contact",
                "Send us a message", "Your name", "Your email", "Your message", "Send message",
                "How can I add a new case?", "How do I manage documents?", "How do I manage clients?",
                "How does the calendar and appointments work?", "How do I manage pending tasks?", "How do I switch between light and dark mode?",
                "To add a new case, click the '+' button in the main window's top bar. A panel will open where you can enter case details, including title, description, associated client, and case type. You can also set the initial case status and add related documents.",
                "In the Documents section, you can drag and drop files to upload them or use the upload button. Documents will be automatically organized by case and type (PDF, images, videos, audio, and others). You can filter documents by client, case, date, and file type. You can also preview, download, or delete documents as needed.",
                "In the Clients section, you can see a list of all your clients. You can add new clients with their contact information, view their case history, associated documents, and active cases. You can also edit client information and manage their personal documentation.",
                "The calendar allows you to manage all your appointments and events. You can add new events associated with specific cases, set reminders, and view a summary of upcoming activities. Events can be filtered by case and status, and you can receive notifications for upcoming appointments.",
                "In the home panel, you can see your pending tasks. Each task can be associated with a specific case and has a status (Pending, In Progress, Completed). You can create new tasks, update their status, and set deadlines. Pending tasks are displayed on the dashboard for quick tracking.",
                "You can switch between light and dark modes using the theme button located in the bottom right corner of the application. The mode will be automatically saved and applied to all application windows.",
                "• Keep case names clear and descriptive", "• Use tags and statuses to categorize cases",
                "• Regularly update case status", "• Organize documents by type and date",
                "• Set reminders for important dates", "• Use filters to quickly find information",
                "• Keep client information up to date", "• Regularly check the dashboard for pending tasks",
                "• Use the calendar to plan appointments", "• Back up important documents",
                "Email: support@tfgapp.com", "Phone: +34 900 123 456", "Hours: M-F 11:00 - 14:00",
                "© 2025 TFG", "Version: ",
                 // Quick Guides English
                "Case Management", "Learn to create, edit, and manage cases efficiently. Organize your cases by status, type, and client for better tracking.",
                "Document Management", "Discover how to organize and manage your legal documents. Upload, categorize, and securely share documents.",
                "Calendar and Appointments", "Manage your appointment and event schedule. Set reminders and keep track of all your activities.",
                "Client Management", "Manage your clients' information, case history, and personal documentation from a centralized location.",
                "Tasks and Tracking", "Organize your pending tasks, set priorities, and effectively track your daily activities.",
                "Dashboard and Statistics", "Use the control panel to get an overview of your active cases, pending tasks, and upcoming appointments."),

                // Catalán
                ("Ajuda", "Preguntes Freqüents", "Guies Ràpides", "Consells i Millors Pràctiques",
                "Organització Eficient", "Optimització de l'Ús", "Necessites més ajuda?", "Contacte",
                "Envia'ns un missatge", "El teu nom", "El teu correu", "El teu missatge", "Enviar missatge",
                "Com puc afegir un nou cas?", "Com gestiono els documents?", "Com gestiono els clients?",
                "Com funciona el calendari i les cites?", "Com gestiono les tasques pendents?", "Com canvio entre mode clar i fosc?",
                "Per afegir un nou cas, fes clic al botó '+' a la barra superior de la finestra principal. S'obrirà un panell on podràs introduir els detalls del cas, incloent títol, descripció, client associat i tipus de cas. També podràs establir l'estat inicial del cas i afegir documents relacionats.",
                "A la secció de Documents pots arrossegar i deixar anar arxius per pujar-los, o utilitzar el botó de pujada. Els documents s'organitzaran automàticament per cas i tipus (PDF, imatges, vídeos, àudios i altres). Pots filtrar els documents per client, cas, data i tipus d'arxiu. També pots previsualitzar, descarregar o eliminar documents segons les teves necessitats.",
                "A la secció de Clients pots veure un llistat de tots els teus clients. Pots afegir nous clients amb les seves dades de contacte, veure el seu historial de casos, documents associats i casos actius. També pots editar la informació dels clients i gestionar la seva documentació personal.",
                "El calendari et permet gestionar totes les teves cites i esdeveniments. Pots afegir nous esdeveniments associats a casos específics, establir recordatoris i veure un resum de les properes activitats. Els esdeveniments es poden filtrar per cas i estat, i pots rebre notificacions de properes cites.",
                "Al panell d'inici pots veure les teves tasques pendents. Cada tasca pot estar associada a un cas específic i té un estat (Pendent, En progrés, Finalitzat). Pots crear noves tasques, actualitzar el seu estat i establir dates límit. Les tasques pendents es mostren al dashboard per a un seguiment ràpid.",
                "Pots canviar entre els modes clar i fosc utilizando el botó de tema ubicat a la cantonada inferior dreta de l'aplicació. El mode es guardarà automàticament i s'aplicarà a totes les finestres de l'aplicació.",
                "• Mantén els noms dels casos clars i descriptius", "• Utilitza etiquetes i estats per categoritzar casos",
                "• Actualitza regularmente l'estat dels teus casos", "• Organitza els documents per tipus i data",
                "• Estableix recordatoris per a dates importants", "• Utilitza els filtres per trobar informació ràpidament",
                "• Mantén actualitzada la informació dels clients", "• Revisa regularment el dashboard para tarefas pendentes",
                "• Utilitza el calendari per planificar cites", "• Realitza còpies de seguretat de documents importants",
                "Correu: suport@tfgapp.com", "Telèfon: +34 900 123 456", "Horari: D-V 11:00 - 14:00",
                "© 2025 TFG", "Versió: ",
                 // Quick Guides Catalán
                "Gestió de Casos", "Aprèn a crear, editar i gestionar casos eficientment. Organitza els teus casos per estat, tipus i client per a un millor seguiment.",
                "Gestió de Documents", "Descobreix com organizer i gestionar els teus documents legals. Puja, categoritza i comparteix documents de forma segura.",
                "Calendari i Cites", "Gestiona la teva agenda de cites i esdeveniments. Estableix recordatoris i mantén un control de totes les teves activitats.",
                "Gestió de Clients", "Administra la informació dels teus clients, el seu historial de casos i documentació personal de forma centralitzada.",
                "Tasques i Seguiment", "Organitza les teves tasques pendents, estableix prioritats i realitza un seguiment efectiu de les teves activitats diàries.",
                "Dashboard i Estadístiques", "Utilitza el panell de control per tenir una visió general dels teus casos actius, tasques pendents i properes cites."),

                // Gallego
                ("Axuda", "Preguntas Frecuentes", "Guías Rápidas", "Consellos e Melloras Prácticas",
                "Organización Eficiente", "Optimización do Uso", "Necesitas máis axuda?", "Contacto",
                "Envíanos unha mensaxe", "O teu nome", "O teu correo", "A túa mensaxe", "Enviar mensaxe",
                "Como podo engadir un novo caso?", "Como xestiono os documentos?", "Como xestiono os clientes?",
                "Como funciona o calendario e as citas?", "Como xestiono as tarefas pendentes?", "Como cambio entre modo claro e escuro?",
                "Para engadir un novo caso, fai clic no botón '+' na barra superior da ventá principal. Abrirase un panel onde poderás introducir os detalles do caso, incluíndo título, descrición, cliente asociado e tipo de caso. Tamén poderás establecer o estado inicial do caso e engadir documentos relacionados.",
                "Na sección de Documentos podes arrastrar e soltar arquivos para subilos, ou usar o botón de subida. Os documentos organizaranse automaticamente por caso e tipo (PDF, imaxes, videos, audios e outros). Podes filtrar os documentos por cliente, caso, data e tipo de arquivo. Tamén podes previsualizar, descarregar ou eliminar documentos segundo as túas necesidades.",
                "Na sección de Clientes podes ver un listado de todos os teus clientes. Podes engadir novos clientes cos seus datos de contacto, ver o seu historial de casos, documentos asociados e casos activos. Tamén podes editar a información dos clientes e xestionar a súa documentación persoal.",
                "O calendario permítete xestionar todas as túas citas e eventos. Podes engadir novos eventos asociados a casos específicos, establecer recordatorios e ver un resumo das próximas actividades. Os eventos pódense filtrar por caso e estado, e podes recibir notificacións de próximas citas.",
                "No panel de inicio podes ver as túas tarefas pendentes. Cada tarefa pode estar asociada a un caso específico e ten un estado (Pendente, En progreso, Finalizado). Podes crear novas tarefas, actualizar o seu estado e establecer datas límite. As tarefas pendentes móstranse no dashboard para un seguimento rápido.",
                "Podes cambiar entre os modos claro e escuro usando o botón de tema situado na esquina inferior dereita da aplicación. O modo gardarase automaticamente e aplicarase en todas as ventás da aplicación.",
                "• Mantén os nomes dos casos claros e descriptivos", "• Usa etiquetas e estados para categorizar casos",
                "• Actualiza regularmente o estado dos teus casos", "• Organiza os documentos por tipo e data",
                "• Establece recordatorios para fechas importantes", "• Usa os filtros para atopar información rapidamente",
                "• Mantén actualizada a información dos clientes", "• Revisa regularmente el dashboard para tareas pendentes",
                "• Usa o calendario para planificar citas", "• Fai copias de seguridade de documentos importantes",
                "Correo: soporte@tfgapp.com", "Teléfono: +34 900 123 456", "Horario: L-V 11:00 - 14:00",
                "© 2025 TFG", "Versión: ",
                 // Quick Guides Gallego
                "Xestión de Casos", "Aprende a crear, editar e xestionar casos eficientemente. Organiza os teus casos por estado, tipo e cliente para un mellor seguimento.",
                "Xestión de Documentos", "Descubre como organizar e xestionar os teus documentos legais. Sube, categoriza e comparte documentos de forma segura.",
                "Calendario e Citas", "Xestiona a túa axenda de citas e eventos. Establece recordatorios e mantén un control de todas as túas actividades.",
                "Xestión de Clientes", "Administra a información dos teus clientes, o seu historial de casos e documentación persoal de forma centralizada.",
                "Tarefas e Seguimento", "Organiza as túas tarefas pendentes, establece prioridades e realiza un seguimento efectivo das túas actividades diarias.",
                "Dashboard e Estatísticas", "Utiliza o panel de control para ter unha visión xeral dos teus casos activos, tarefas pendentes e próximas citas."),

                // Euskera
                ("Laguntza", "Ohiko Galderak", "Gida Azkarrak", "Aholkuak eta Praktika Onenak",
                "Eraginkor Antolaketa", "Erabilera Optimizazioa", "Laguntza gehiago behar duzu?", "Kontaktua",
                "Bidali mezu bat", "Zure izena", "Zure posta", "Zure mezua", "Mezua bidali",
                "Nola gehitu dezaket kasu berri bat?", "Nola kudeatu dokumentuak?", "Nola kudeatu bezeroak?",
                "Nola funtzionatzen du egutegia eta hitzorduak?", "Nola kudeatu zeregin pendenteak?", "Nola aldatu modu argi eta ilun artean?",
                "Kasu berri bat gehitzeko, egin klik '+' botoian leiho nagusiaren goiko barran. Panela irekiko da, non kasuaren xehetasunak sartu ahal izango dituzun, izenburua, deskribapena, lotutako bezeroa eta kasu mota barne. Kasuaren hasierako egoera ezarri eta dokumentu erlazionatuak gehitu ditzakezu ere.",
                "Dokumentuen atalean, fitxategiak arrastatu eta jaregin ditzakezu kargatzeko, edo kargatzeko botoia erabili. Dokumentuak automatikoki antolatu egingo dira kasuaren eta motaren arabera (PDF, irudiak, bideoak, audioak eta besteak). Dokumentuak iragazi ditzakezu bezeroaren, kasuaren, dataren eta fitxategi motaren arabera. ere previsualizatu, deskargatu edo ezabatu ditzakezu zure beharretara egokituz.",
                "Bezeroen atalean, zure bezero guztien zerrenda ikus dezakezu. Bezero berriak gehitu ditzakezu beren kontaktuko informazioarekin, kasuen historiala ikusi, lotutako dokumentuak eta kasu aktiboak. Bezeroen informazioa editatu eta beren dokumentazio pertsonala kudeatu dezakezu ere.",
                "Egutegiak zure hitzordu eta gertaera guztiak kudeatzea ahalbidetzen dizu. Gertaera berriak gehitu ditzakezu kasu zehatzekin lotuta, oroigarriak ezarri eta hurrengo jardueren laburpena ikusi. Gertaerak iragazi daitezke kasuaren eta egoeraren arabera, eta hurrengo hitzorduen jakinarazpenak jaso ditzakezu.",
                "Hasierako panelean, zure zeregin pendenteak ikus ditzakezu. Zeregin bakoitza kasu zehatz batekin lotuta egon daiteke eta egoera bat du (Pendente, Martxan, Bukatua). Zeregin berriak sortu, egoera eguneratu eta epeak ezarri ditzakezu. Zeregin pendenteak dashboardean erakusten dira jarraipen azkarra egiteko.",
                "Aplikazioaren beheko eskuineko izkinean dagoen gai botoia erabiliz modu argi eta ilun artean aldatu dezakezu. Modua automatikoki gordeko da eta aplikazioaren leiho guztietan aplikatuko da.",
                "• Mantendu kasuen izenak argi eta deskriptiboak", "• Erabili etiketak eta egoerak kasuak kategorizatzeko",
                "• Eguneratu regularmente kasuen egoera", "• Antolatu dokumentuak motaren eta dataren arabera",
                "• Ezarri oroigarriak data garrantzitsuetarako", "• Erabili iragazkiak informazioa azkar aurkitzeko",
                "• Mantendu eguneratuta bezeroen informazioa", "• Egiaztatu regularmente dashboarda zeregin pendenteetarako",
                "• Erabili egutegia hitzorduak planifikatzeko", "• Egin dokumentu garrantzitsuen segurtasun kopia",
                "Posta: laguntza@tfgapp.com", "Telefonoa: +34 900 123 456", "Ordutegia: A-O 11:00 - 14:00",
                "© 2025 TFG", "Bertsioa: ",
                 // Quick Guides Euskera
                "Kasuen kudeaketa", "Ikasi kasuak eraginkortasunez sortzen, editatzen eta kudeatzen. Antolatu zure kasuak egoeraren, motaren eta bezeroaren arabera jarraipen hobea izateko.",
                "Dokumentuen kudeaketa", "Ezagutu zure dokumentu legalak nola antolatu eta kudeatu. Kargatu, kategorizatu eta modu seguruan partekatu dokumentuak.",
                "Egutegia eta Hitzorduak", "Kudeatu zure hitzordu eta gertaeren egutegia. Ezarri oroigarriak eta mantendu zure jarduera guztien kontrola.",
                "Bezeroen kudeaketa", "Kudeatu zure bezeroen informazioa, kasuen historiala eta dokumentazio pertsonala zentralizatutako leku batetik.",
                "Zereginak eta Jarraipena", "Antolatu zure zeregin pendenteak, ezarri lehentasunak eta egin zure eguneroko jardueren jarraipen eraginkorra.",
                "Dashboard eta Estatistikak", "Erabili kontrol panela zure kasu aktiboen, zeregin pendenteen eta hurrengo hitzorduen ikuspegi orokorra izateko.")
            };

            if (idioma < 0 || idioma >= idiomas.Length)
                idioma = 0;

            var t = idiomas[idioma];

            // Actualizar textos principales
            txtTituloPrincipal.Text = t.Titulo;
            txtPreguntasFrecuentes.Text = t.PreguntasFrecuentes;
            txtGuiasRapidas.Text = t.GuiasRapidasTexto;
            txtConsejosPracticas.Text = t.ConsejosPracticas;
            txtOrganizacionEficiente.Text = t.OrganizacionEficiente;
            txtOptimizacionUso.Text = t.OptimizacionUso;
            txtNecesitasAyuda.Text = t.NecesitasAyuda;
            txtContacto.Text = t.Contacto;
            txtEnvianosMensaje.Text = t.EnvianosMensaje;

            // Actualizar placeholders (usando Tag en TextBox)
            NombreTextBox.Tag = t.TuNombre;
            EmailTextBox.Tag = t.TuEmail;
            MensajeTextBox.Tag = t.TuMensaje;

            // Actualizar botón de envío
            // Asumiendo que el Content del botón es un StackPanel con un TextBlock como segundo elemento
            if (btnEnviarMensaje.Content is StackPanel buttonStackPanel && buttonStackPanel.Children.Count > 1 && buttonStackPanel.Children[1] is TextBlock buttonTextBlock)
            {
                buttonTextBlock.Text = t.EnviarMensaje;
            }

            // Actualizar preguntas frecuentes (Expanders)
            expanderAnadirCaso.Header = t.ComoAnadirCaso;
            if (expanderAnadirCaso.Content is TextBlock textBlockAnadirCaso)
            {
                textBlockAnadirCaso.Text = t.RespuestaAnadirCaso;
            }

            expanderGestionarDocumentos.Header = t.ComoGestionarDocumentos;
            if (expanderGestionarDocumentos.Content is TextBlock textBlockGestionarDocumentos)
            {
                textBlockGestionarDocumentos.Text = t.RespuestaGestionarDocumentos;
            }

            expanderGestionarClientes.Header = t.ComoGestionarClientes;
            if (expanderGestionarClientes.Content is TextBlock textBlockGestionarClientes)
            {
                textBlockGestionarClientes.Text = t.RespuestaGestionarClientes;
            }

            expanderFuncionaCalendario.Header = t.ComoFuncionaCalendario;
            if (expanderFuncionaCalendario.Content is TextBlock textBlockFuncionaCalendario)
            {
                textBlockFuncionaCalendario.Text = t.RespuestaCalendario;
            }

            expanderGestionarTareas.Header = t.ComoGestionarTareas;
            if (expanderGestionarTareas.Content is TextBlock textBlockGestionarTareas)
            {
                textBlockGestionarTareas.Text = t.RespuestaTareas;
            }

            expanderCambiarTema.Header = t.ComoCambiarTema;
            if (expanderCambiarTema.Content is TextBlock textBlockCambiarTema)
            {
                textBlockCambiarTema.Text = t.RespuestaCambiarTema;
            }

            // Actualizar Guías Rápidas (ObservableCollection)
            if (GuiasRapidas != null && GuiasRapidas.Count >= 6)
            {
                GuiasRapidas[0].Title = t.GuiaCasosTitulo;
                GuiasRapidas[0].Description = t.GuiaCasosDescripcion;

                GuiasRapidas[1].Title = t.GuiaDocumentosTitulo;
                GuiasRapidas[1].Description = t.GuiaDocumentosDescripcion;

                GuiasRapidas[2].Title = t.GuiaCalendarioTitulo;
                GuiasRapidas[2].Description = t.GuiaCalendarioDescripcion;

                GuiasRapidas[3].Title = t.GuiaClientesTitulo;
                GuiasRapidas[3].Description = t.GuiaClientesDescripcion;

                GuiasRapidas[4].Title = t.GuiaTareasTitulo;
                GuiasRapidas[4].Description = t.GuiaTareasDescripcion;

                GuiasRapidas[5].Title = t.GuiaDashboardTitulo;
                GuiasRapidas[5].Description = t.GuiaDashboardDescripcion;
            }

            // Actualizar consejos
            var consejosOrganizacion = this.FindName("ConsejosOrganizacion") as StackPanel;
            if (consejosOrganizacion != null && consejosOrganizacion.Children.Count >= 5)
            {
                // Assuming the first child is the title and the rest are TextBlocks for list items
                if (consejosOrganizacion.Children[0] is TextBlock titleOrganizacion)
                {
                     titleOrganizacion.Text = t.OrganizacionEficiente;
                }
                if (consejosOrganizacion.Children[1] is TextBlock item1) item1.Text = t.ConsejoNombresCasos;
                if (consejosOrganizacion.Children[2] is TextBlock item2) item2.Text = t.ConsejoEtiquetas;
                if (consejosOrganizacion.Children[3] is TextBlock item3) item3.Text = t.ConsejoActualizarEstado;
                if (consejosOrganizacion.Children[4] is TextBlock item4) item4.Text = t.ConsejoOrganizarDocumentos;
                if (consejosOrganizacion.Children.Count > 5 && consejosOrganizacion.Children[5] is TextBlock item5) item5.Text = t.ConsejoRecordatorios;
            }

            var consejosUso = this.FindName("ConsejosUso") as StackPanel;
            if (consejosUso != null && consejosUso.Children.Count >= 5)
            {
                // Assuming the first child is the title and the rest are TextBlocks for list items
                 if (consejosUso.Children[0] is TextBlock titleUso)
                {
                     titleUso.Text = t.OptimizacionUso;
                }
                if (consejosUso.Children[1] is TextBlock item1) item1.Text = t.ConsejoFiltros;
                if (consejosUso.Children[2] is TextBlock item2) item2.Text = t.ConsejoInfoClientes;
                if (consejosUso.Children[3] is TextBlock item3) item3.Text = t.ConsejoDashboard;
                if (consejosUso.Children[4] is TextBlock item4) item4.Text = t.ConsejoCalendario;
                 if (consejosUso.Children.Count > 5 && consejosUso.Children[5] is TextBlock item5) item5.Text = t.ConsejoCopiasSeguridad;
            }

            // Actualizar información de contacto
            var infoContacto = this.FindName("InfoContacto") as StackPanel;
            if (infoContacto != null && infoContacto.Children.Count >= 3)
            {
                // Assuming the first child is the title and the rest are TextBlocks for contact info
                 if (infoContacto.Children[0] is TextBlock titleContacto)
                {
                     titleContacto.Text = t.Contacto;
                }
                if (infoContacto.Children[1] is TextBlock item1) item1.Text = t.EmailContacto;
                if (infoContacto.Children[2] is TextBlock item2) item2.Text = t.TelefonoContacto;
                if (infoContacto.Children.Count > 3 && infoContacto.Children[3] is TextBlock item3) item3.Text = t.HorarioContacto;
            }

            // Actualizar footer
            var txtCopyright = this.FindName("txtCopyright") as TextBlock;
            if (txtCopyright != null)
                txtCopyright.Text = t.Copyright;

            var txtVersion = this.FindName("txtVersion") as TextBlock;
            if (txtVersion != null)
                txtVersion.Text = t.Version + "0.1";
        }
        #endregion
    }

    public class GuiaRapida
    {
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
