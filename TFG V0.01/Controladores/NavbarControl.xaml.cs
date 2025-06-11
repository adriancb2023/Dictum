using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TFG_V0._01.Ventanas;

namespace TFG_V0._01.Controladores
{
    public partial class NavbarControl : UserControl
    {
        private bool _isExpanded = false;

        public NavbarControl()
        {
            InitializeComponent();
            CargarIdioma(MainWindow.idioma);
        }

        private void CargarIdioma(int idioma)
        {
            var idiomas = new (string Inicio, string Buscar, string Agenda, string Casos, string Clientes, string Documentos, string Ajustes, string Gpt)[]
            {
                ("Inicio", "Buscar", "Agenda", "Casos", "Clientes", "Documentos", "Ajustes", "GPT"),
                ("Home", "Search", "Calendar", "Cases", "Clients", "Documents", "Settings", "GPT"),
                ("Inici", "Cercar", "Agenda", "Casos", "Clients", "Documents", "Ajustos", "GPT"),
                ("Inicio", "Buscar", "Axenda", "Casos", "Clientes", "Documentos", "Axustes", "GPT"),
                ("Hasiera", "Bilatu", "Agenda", "Kasuak", "Bezeroak", "Dokumentuak", "Ezarpenak", "GPT"),
                ("GPT", "GPT", "GPT", "GPT", "GPT", "GPT", "GPT", "GPT")
            };

            if (idioma >= 0 && idioma < idiomas.Length)
            {
                inicio.Text = idiomas[idioma].Inicio;
                buscar.Text = idiomas[idioma].Buscar;
                agenda.Text = idiomas[idioma].Agenda;
                casos.Text = idiomas[idioma].Casos;
                clientes.Text = idiomas[idioma].Clientes;
                documentos.Text = idiomas[idioma].Documentos;
                ajustes.Text = idiomas[idioma].Ajustes;
                GPT.Text = idiomas[idioma].Gpt;
            }
        }

        public void CambiarVisibilidadNavbar(bool expandir)
        {
            if (expandir)
            {
                inicio.Visibility = Visibility.Visible;
                buscar.Visibility = Visibility.Visible;
                agenda.Visibility = Visibility.Visible;
                casos.Visibility = Visibility.Visible;
                clientes.Visibility = Visibility.Visible;
                documentos.Visibility = Visibility.Visible;
                ajustes.Visibility = Visibility.Visible;
                GPT.Visibility = Visibility.Visible;
                _isExpanded = true;
            }
            else
            {
                inicio.Visibility = Visibility.Collapsed;
                buscar.Visibility = Visibility.Collapsed;
                agenda.Visibility = Visibility.Collapsed;
                casos.Visibility = Visibility.Collapsed;
                clientes.Visibility = Visibility.Collapsed;
                documentos.Visibility = Visibility.Collapsed;
                ajustes.Visibility = Visibility.Collapsed;
                GPT.Visibility = Visibility.Collapsed;
                _isExpanded = false;
            }
        }

        private void Menu_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CambiarVisibilidadNavbar(true);
        }

        private void Menu_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CambiarVisibilidadNavbar(false);
        }

        private void CambiarIcono(Image imagen, string rutaIcono)
        {
            try
            {
                imagen.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(rutaIcono, UriKind.Relative));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el icono: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CambiarIconosAOscuros()
        {
            CambiarIcono(imagenHome2, "/TFG V0.01;component/Recursos/Iconos/home.png");
            CambiarIcono(imagenBuscar2, "/TFG V0.01;component/Recursos/Iconos/buscar.png");
            CambiarIcono(imagenAgenda2, "/TFG V0.01;component/Recursos/Iconos/agenda.png");
            CambiarIcono(imagenCasos2, "/TFG V0.01;component/Recursos/Iconos/casos.png");
            CambiarIcono(imagenClientes2, "/TFG V0.01;component/Recursos/Iconos/clientes.png");
            CambiarIcono(imagenDocumentos2, "/TFG V0.01;component/Recursos/Iconos/documentos.png");
            CambiarIcono(imagenAjustes2, "/TFG V0.01;component/Recursos/Iconos/ajustes.png");
            CambiarIcono(imagenAyuda2, "/TFG V0.01;component/Recursos/Iconos/ayuda.png");
            CambiarIcono(imagenGpt, "/TFG V0.01;component/Recursos/Iconos/ia2.png");
        }

        public void CambiarIconosAClaros()
        {
            CambiarIcono(imagenHome2, "/TFG V0.01;component/Recursos/Iconos/home2.png");
            CambiarIcono(imagenBuscar2, "/TFG V0.01;component/Recursos/Iconos/buscar2.png");
            CambiarIcono(imagenAgenda2, "/TFG V0.01;component/Recursos/Iconos/agenda2.png");
            CambiarIcono(imagenCasos2, "/TFG V0.01;component/Recursos/Iconos/casos2.png");
            CambiarIcono(imagenClientes2, "/TFG V0.01;component/Recursos/Iconos/clientes2.png");
            CambiarIcono(imagenDocumentos2, "/TFG V0.01;component/Recursos/Iconos/documentos2.png");
            CambiarIcono(imagenAjustes2, "/TFG V0.01;component/Recursos/Iconos/ajustes2.png");
            CambiarIcono(imagenAyuda2, "/TFG V0.01;component/Recursos/Iconos/ayuda2.png");
            CambiarIcono(imagenGpt, "/TFG V0.01;component/Recursos/Iconos/ia.png");
        }

        private void irHome(object sender, RoutedEventArgs e)
        {
            AbrirVentana(new Home());
        }

        private void irJurisprudencia(object sender, RoutedEventArgs e)
        {
            AbrirVentana(new BusquedaJurisprudencia());
        }

        private void irAgenda(object sender, RoutedEventArgs e)
        {
            AbrirVentana(new Agenda());
        }

        private void irCasos(object sender, RoutedEventArgs e)
        {
            AbrirVentana(new Casos());
        }

        private void irClientes(object sender, RoutedEventArgs e)
        {
            AbrirVentana(new Clientes());
        }

        private void irDocumentos(object sender, RoutedEventArgs e)
        {
            AbrirVentana(new Documentos());
        }

        private void irAjustes(object sender, RoutedEventArgs e)
        {
            AbrirVentana(new Ajustes());
        }

        private void irAyuda(object sender, RoutedEventArgs e)
        {
            AbrirVentana(new Ayuda());
        }

        private void irGPT(object sender, RoutedEventArgs e)
        {
            AbrirVentana(new GPTChat());
        }

        private void AbrirVentana(Window ventana)
        {
            Window currentWindow = Window.GetWindow(this);
            ventana.Show();
            if (currentWindow != null)
            {
                currentWindow.Close();
            }
        }

        public void CambiarTextosNegro()
        {
            inicio.Foreground = new SolidColorBrush(Colors.Black);
            buscar.Foreground = new SolidColorBrush(Colors.Black);
            agenda.Foreground = new SolidColorBrush(Colors.Black);
            casos.Foreground = new SolidColorBrush(Colors.Black);
            clientes.Foreground = new SolidColorBrush(Colors.Black);
            documentos.Foreground = new SolidColorBrush(Colors.Black);
            ajustes.Foreground = new SolidColorBrush(Colors.Black);
            GPT.Foreground = new SolidColorBrush(Colors.Black);
        }

        public void CambiarTextosBlanco()
        {
            inicio.Foreground = new SolidColorBrush(Colors.White);
            buscar.Foreground = new SolidColorBrush(Colors.White);
            agenda.Foreground = new SolidColorBrush(Colors.White);
            casos.Foreground = new SolidColorBrush(Colors.White);
            clientes.Foreground = new SolidColorBrush(Colors.White);
            documentos.Foreground = new SolidColorBrush(Colors.White);
            ajustes.Foreground = new SolidColorBrush(Colors.White);
            GPT.Foreground = new SolidColorBrush(Colors.White);
        }

        public void ActualizarTema(bool isDarkTheme)
        {
            if (isDarkTheme)
            {
                CambiarIconosAClaros();
                CambiarTextosBlanco();
            }
            else
            {
                CambiarIconosAOscuros();
                CambiarTextosNegro();
            }
        }

        public void ActualizarIdioma(int idioma)
        {
            CargarIdioma(idioma);
        }
    }
}