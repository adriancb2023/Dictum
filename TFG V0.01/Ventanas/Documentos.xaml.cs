using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using TFG_V0._01.Controladores;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using TFG_V0._01.Supabase.Models;

namespace TFG_V0._01.Ventanas
{
    public partial class Documentos : Window, INotifyPropertyChanged
    {
        #region 🎬 variables animacion
        private Storyboard? fadeInStoryboard;
        private Storyboard? shakeStoryboard;
        #endregion

        #region Propiedades y Variables
        private bool isFiltrosPanelVisible = false;
        private readonly Duration animationDuration = new Duration(TimeSpan.FromSeconds(0.3));
        private readonly DoubleAnimation fadeInAnimation;
        private readonly DoubleAnimation fadeOutAnimation;
        private readonly DoubleAnimation shakeAnimation;
        private Point lastMousePosition;
        private bool isDragging = false;
        private DrawingBrush? _meshGradientBrush;

        public ObservableCollection<DocumentPanel> DocumentPanelsCollection { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public Documentos()
        {
            InitializeComponent();
            DataContext = this;

            // Inicializar animaciones
            fadeInAnimation = new DoubleAnimation(0, 1, animationDuration);
            fadeOutAnimation = new DoubleAnimation(1, 0, animationDuration);
            shakeAnimation = new DoubleAnimation
            {
                From = 0,
                To = 5,
                Duration = new Duration(TimeSpan.FromMilliseconds(50)),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3)
            };

            InitializeDocumentPanels();
            InitializeMeshGradient();
            UpdateTheme();
        }

        #region 🌓 Aplicar modo oscuro/claro cargado por sistema
        private string GetIconoTema() =>
            MainWindow.isDarkTheme
                ? "/TFG V0.01;component/Recursos/Iconos/sol.png"
                : "/TFG V0.01;component/Recursos/Iconos/luna.png";

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.isDarkTheme = !MainWindow.isDarkTheme;
            UpdateTheme();
        }
        #endregion

        #region 🎬  Animaciones

        private void InitializeAnimations()
        {
            fadeInStoryboard = CrearStoryboard(this, OpacityProperty, CrearFadeAnimation(0, 1, 0.5));
            shakeStoryboard = CrearStoryboard(null, TranslateTransform.XProperty, CrearShakeAnimation());
        }

        private Storyboard CrearStoryboard(DependencyObject target, DependencyProperty property, DoubleAnimation animation)
        {
            var storyboard = new Storyboard();
            if (target != null && property != null)
            {
                Storyboard.SetTarget(animation, target);
                Storyboard.SetTargetProperty(animation, new PropertyPath(property));
            }
            storyboard.Children.Add(animation);
            return storyboard;
        }

        private DoubleAnimation CrearFadeAnimation(double from, double to, double durationSeconds, bool autoReverse = false) =>
            new()
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                AutoReverse = autoReverse
            };

        private DoubleAnimation CrearShakeAnimation() =>
            new()
            {
                From = 0,
                To = 5,
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3),
                Duration = TimeSpan.FromSeconds(0.05)
            };

        private void BeginFadeInAnimation()
        {
            this.Opacity = 0;
            fadeInStoryboard.Begin();
        }

        private void ShakeElement(FrameworkElement element)
        {
            var trans = new TranslateTransform();
            element.RenderTransform = trans;
            trans.BeginAnimation(TranslateTransform.XProperty, CrearShakeAnimation());
        }
        #endregion


        #region Inicialización de Paneles
        private void InitializeDocumentPanels()
        {
            DocumentPanelsCollection = new ObservableCollection<DocumentPanel>();
            var panelTypes = new[] { "PDF", "IMG", "VID", "AUD", "OTR" };
            var panelTitles = new[] { "Documentos PDF", "Imágenes", "Videos", "Audios", "Otros Documentos" };

            for (int i = 0; i < panelTypes.Length; i++)
            {
                DocumentPanelsCollection.Add(new DocumentPanel
                {
                    Title = panelTitles[i],
                    Type = panelTypes[i],
                    IsVisible = true,
                    IsOddPanel = i % 2 == 0
                });
            }

            DocumentPanels.ItemsSource = DocumentPanelsCollection;
        }
        #endregion

        #region Filtros y Manejo de Documentos
        private void ToggleFiltros_Click(object sender, RoutedEventArgs e)
        {
            if (SlidePanelFiltros.Visibility == Visibility.Collapsed)
            {
                ShowSlidePanelFiltros();
            }
            else
            {
                HideSlidePanelFiltros();
            }
        }

        private void ShowSlidePanelFiltros()
        {
            SlidePanelFiltros.Visibility = Visibility.Visible;
            OverlayPanel.Visibility = Visibility.Visible;

            var slideInAnimation = new DoubleAnimation
            {
                From = SlidePanelFiltros.ActualWidth,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            SlidePanelFiltrosTransform.BeginAnimation(TranslateTransform.XProperty, slideInAnimation);
        }

        private void HideSlidePanelFiltros()
        {
            var slideOutAnimation = new DoubleAnimation
            {
                From = 0,
                To = SlidePanelFiltros.ActualWidth,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            slideOutAnimation.Completed += (s, e) =>
            {
                SlidePanelFiltros.Visibility = Visibility.Collapsed;
                OverlayPanel.Visibility = Visibility.Collapsed;
            };

            SlidePanelFiltrosTransform.BeginAnimation(TranslateTransform.XProperty, slideOutAnimation);
        }

        private void OverlayPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SlidePanelFiltros.Visibility == Visibility.Visible)
            {
                HideSlidePanelFiltros();
            }
        }

        private void ComboClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Implement client selection change logic
            // Nota: Este manejador se llama desde el ComboBox en el panel de filtros.
            // Asegúrate de que el x:Name del ComboBox en el panel deslizante sea ComboClientesPanel
            var selectedClient = ComboClientesPanel.SelectedItem;
            if (selectedClient != null)
            {
                // Update cases list based on selected client
                // Update documents list based on selected client
            }
        }

        private void ComboCasos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Implement case selection change logic
            // Nota: Este manejador se llama desde el ComboBox en el panel de filtros.
            // Asegúrate de que el x:Name del ComboBox en el panel deslizante sea ComboCasosPanel
            var selectedCase = ComboCasosPanel.SelectedItem;
            if (selectedCase != null)
            {
                // Update documents list based on selected case
            }
        }

        private void FiltroFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Implement date filter change logic
             // Nota: Este manejador se llama desde el DatePicker en el panel de filtros.
            // Asegúrate de que el x:Name del DatePicker en el panel deslizante sea FiltroFechaPanel
            var selectedDate = FiltroFechaPanel.SelectedDate;
            if (selectedDate.HasValue)
            {
                // Filter documents based on selected date
            }
        }

        private void DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                // TODO: Handle dropped files
                // ProcessFiles(files);
            }
        }

        private void SelectFiles_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                Filter = "All Files|*.*|PDF Files|*.pdf|Image Files|*.jpg;*.jpeg;*.png;*.gif|Video Files|*.mp4;*.avi;*.mov|Audio Files|*.mp3;*.wav"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // TODO: Handle selected files
                // ProcessFiles(openFileDialog.FileNames);
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var file = button?.DataContext as DocumentFile;
            if (file != null)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = file.Path,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al abrir el archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var file = button?.DataContext as DocumentFile;
            if (file != null)
            {
                var result = MessageBox.Show(
                    $"¿Estás seguro de que deseas eliminar el archivo '{file.Name}'?",
                    "Confirmar eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // TODO: Implement file deletion logic
                        // DeleteFile(file);
                        // Update UI
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar el archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void TipoDocumento_Changed(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            if (checkbox != null && checkbox.Tag != null)
            {
                string tipo = checkbox.Tag.ToString()!;
                bool isChecked = checkbox.IsChecked ?? false;

                // Si ningún checkbox está marcado, mostrar todos los paneles
                var anyCheckboxChecked = false;
                var wrapPanel = checkbox.Parent as WrapPanel;
                if (wrapPanel != null)
                {
                    foreach (CheckBox cb in wrapPanel.Children)
                    {
                        if (cb.IsChecked ?? false)
                        {
                            anyCheckboxChecked = true;
                            break;
                        }
                    }

                    if (!anyCheckboxChecked)
                    {
                        foreach (var panel in DocumentPanelsCollection)
                        {
                            panel.IsVisible = true;
                        }
                    }
                    else
                    {
                        // Actualizar la visibilidad del panel correspondiente
                        var panel = DocumentPanelsCollection.FirstOrDefault(p => p.Type == tipo);
                        if (panel != null)
                        {
                            panel.IsVisible = isChecked;
                        }
                    }
                }
            }
        }

        private void ReorderVisiblePanels()
        {
            var visiblePanels = DocumentPanelsCollection.Where(p => p.IsVisible).ToList();
            for (int i = 0; i < visiblePanels.Count; i++)
            {
                visiblePanels[i].IsOddPanel = i % 2 == 0;
            }
        }
        #endregion

        private void InitializeMeshGradient()
        {
            _meshGradientBrush = new DrawingBrush();
            var drawing = new DrawingGroup();
            
            // Crear el primer gradiente
            var gradient1 = new RadialGradientBrush
            {
                Center = new Point(0.3, 0.3),
                RadiusX = 0.5,
                RadiusY = 0.5,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(222, 156, 184), 0),
                    new GradientStop(Color.FromRgb(157, 205, 225), 1)
                }
            };

            // Crear el segundo gradiente
            var gradient2 = new RadialGradientBrush
            {
                Center = new Point(0.7, 0.7),
                RadiusX = 0.6,
                RadiusY = 0.6,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(220, 142, 184), 0),
                    new GradientStop(Color.FromRgb(152, 211, 236), 1)
                }
            };

            // Añadir los gradientes al drawing
            drawing.Children.Add(new GeometryDrawing(gradient1, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
            drawing.Children.Add(new GeometryDrawing(gradient2, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));

            _meshGradientBrush.Drawing = drawing;
            _meshGradientBrush.Viewport = new Rect(0, 0, 1, 1);
            _meshGradientBrush.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            _meshGradientBrush.TileMode = TileMode.None;

            // Aplicar el brush al fondo
            this.Background = _meshGradientBrush;
        }

        private void UpdateTheme()
        {
            // Actualizar el Tag de la ventana para que los estilos del XAML reaccionen
            this.Tag = MainWindow.isDarkTheme;

            // Actualizar los colores del gradiente
            if (_meshGradientBrush != null && _meshGradientBrush.Drawing is DrawingGroup drawingGroup && drawingGroup.Children.Count == 2)
            {
                if (drawingGroup.Children[0] is GeometryDrawing gd1 && gd1.Brush is RadialGradientBrush gradient1 &&
                    drawingGroup.Children[1] is GeometryDrawing gd2 && gd2.Brush is RadialGradientBrush gradient2)
                {
                    if (MainWindow.isDarkTheme)
                    {
                        // Colores para modo oscuro (iguales a Home.xaml.cs)
                        gradient1.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#8C7BFF");
                        gradient1.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#08a693"); // Tono verde azulado oscuro
                        gradient2.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#3a4d5f"); // Tono gris azulado oscuro
                        gradient2.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#272c3f"); // Tono gris oscuro
                    }
                    else
                    {
                        // Colores para modo claro (iguales a Home.xaml.cs)
                        gradient1.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#de9cb8");
                        gradient1.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#9dcde1");
                        gradient2.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#dc8eb8");
                        gradient2.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#98d3ec");
                    }
                }
            }
             // Actualizar el tema de la Navbar (si es necesario)
            navbar.ActualizarTema(MainWindow.isDarkTheme);
             // Actualizar el icono del botón de tema
             var button = FindName("ThemeButton") as Button;
             var icon = button?.Template.FindName("ThemeIcon", button) as Image;

             if (icon != null)
                 icon.Source = new BitmapImage(new Uri(GetIconoTema(), UriKind.Relative));
        }
    }

    public class DocumentPanel : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private string _type = string.Empty;
        private bool _isVisible;
        private bool _isOddPanel;
        private ObservableCollection<DocumentFile> _files = new();

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public bool IsOddPanel
        {
            get => _isOddPanel;
            set
            {
                _isOddPanel = value;
                OnPropertyChanged(nameof(IsOddPanel));
            }
        }

        public ObservableCollection<DocumentFile> Files
        {
            get => _files;
            set
            {
                _files = value;
                OnPropertyChanged(nameof(Files));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DocumentPanel()
        {
            Files = new ObservableCollection<DocumentFile>();
        }
    }

    public class DocumentFile
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
        public long Size { get; set; }
    }
}