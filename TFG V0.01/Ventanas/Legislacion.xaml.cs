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
using TFG_V0._01.Supabase;

namespace TFG_V0._01.Ventanas
{
    public partial class Legislacion : Window
    {
        #region Variables
        private RadialGradientBrush mesh1Brush;
        private RadialGradientBrush mesh2Brush;
        #endregion

        #region Initialization
        public Legislacion()
        {
            InitializeComponent();
            CrearFondoAnimado();
            AplicarModoSistema();
        }
        #endregion

        #region Fondo
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
        }
        #endregion

        #region Aplicar MODO CLARO/OSCURO
        private void AplicarModoSistema()
        {
            this.Tag = MainWindow.isDarkTheme;

            // Cambiar fondo mesh gradient
            if (MainWindow.isDarkTheme)
            {
                // Colores mesh oscuro
                mesh1Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#8C7BFF");
                mesh1Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#08a693");
                mesh2Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#3a4d5f");
                mesh2Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#272c3f");
            }
            else
            {
                // Colores mesh claro
                mesh1Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#de9cb8");
                mesh1Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#9dcde1");
                mesh2Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#dc8eb8");
                mesh2Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#98d3ec");
            }

            // Crear nuevos estilos dinámicamente
            var primaryTextStyle = new Style(typeof(TextBlock));
            var secondaryTextStyle = new Style(typeof(TextBlock));

            if (MainWindow.isDarkTheme)
            {
                primaryTextStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"))));
                secondaryTextStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B0B0B0"))));
            }
            else
            {
                primaryTextStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"))));
                secondaryTextStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#606060"))));
            }

            // Reemplazar los recursos existentes
            this.Resources["PrimaryTextStyle"] = primaryTextStyle;
            this.Resources["SecondaryTextStyle"] = secondaryTextStyle;
        }
        #endregion

        #region Cambio modo Claro/oscuro
        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.isDarkTheme = !MainWindow.isDarkTheme;
            AplicarModoSistema();
        }
        #endregion
    }
}
