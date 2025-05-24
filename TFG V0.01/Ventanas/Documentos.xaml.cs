using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        private bool isDarkMode = true; // CS0414: El campo está asignado pero nunca se usa. Si no lo necesitas, elimínalo.
        private bool isFiltrosPanelVisible = false;
        private readonly Duration animationDuration = new Duration(TimeSpan.FromSeconds(0.3));
        private readonly DoubleAnimation fadeInAnimation;
        private readonly DoubleAnimation fadeOutAnimation;
        private readonly DoubleAnimation shakeAnimation;

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
            AplicarModoSistema();
        }

        #region 🌓 Aplicar modo oscuro/claro cargado por sistema
        private void AplicarModoSistema()
        {
            var button = FindName("ThemeButton") as Button;
            var icon = button?.Template.FindName("ThemeIcon", button) as Image;

            if (icon != null)
                icon.Source = new BitmapImage(new Uri(GetIconoTema(), UriKind.Relative));

            backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString(GetBackgroundPath()) as ImageSource;

            navbar.ActualizarTema(MainWindow.isDarkTheme);
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
            isFiltrosPanelVisible = !isFiltrosPanelVisible;
            PanelFiltros.Visibility = isFiltrosPanelVisible ? Visibility.Visible : Visibility.Collapsed;
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