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
            var idiomas = new (string Inicio, string Buscar, string Agenda, string Casos, string Clientes, string Documentos, string Ajustes)[]
            {
                ("Inicio", "Buscar", "Agenda", "Casos", "Clientes", "Documentos", "Ajustes"),
                ("Home", "Search", "Calendar", "Cases", "Clients", "Documents", "Settings"),
                ("Inici", "Cercar", "Agenda", "Casos", "Clients", "Documents", "Ajustos"),
                ("Inicio", "Buscar", "Axenda", "Casos", "Clientes", "Documentos", "Axustes"),
                ("Hasiera", "Bilatu", "Agenda", "Kasuak", "Bezeroak", "Dokumentuak", "Ezarpenak")
            };

            if (idioma < 0 || idioma >= idiomas.Length)
                idioma = 0;

            var t = idiomas[idioma];

            inicio.Text = t.Inicio;
            buscar.Text = t.Buscar;
            agenda.Text = t.Agenda;
            casos.Text = t.Casos;
            clientes.Text = t.Clientes;
            documentos.Text = t.Documentos;
            ajustes.Text = t.Ajustes;
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

        public void CambiarIconosAOscuros()
        {
            imagenHome2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/home.png", UriKind.Relative));
            imagenBuscar2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/buscar.png", UriKind.Relative));
            imagenAgenda2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/agenda.png", UriKind.Relative));
            imagenCasos2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/casos.png", UriKind.Relative));
            imagenClientes2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/clientes.png", UriKind.Relative));
            imagenDocumentos2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/documentos.png", UriKind.Relative));
            imagenAjustes2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/ajustes.png", UriKind.Relative));
            imagenAyuda2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/ayuda.png", UriKind.Relative));
        }

        public void CambiarIconosAClaros()
        {
            imagenHome2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/home2.png", UriKind.Relative));
            imagenBuscar2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/buscar2.png", UriKind.Relative));
            imagenAgenda2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/agenda2.png", UriKind.Relative));
            imagenCasos2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/casos2.png", UriKind.Relative));
            imagenClientes2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/clientes2.png", UriKind.Relative));
            imagenDocumentos2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/documentos2.png", UriKind.Relative));
            imagenAjustes2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/ajustes2.png", UriKind.Relative));
            imagenAyuda2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/ayuda2.png", UriKind.Relative));
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