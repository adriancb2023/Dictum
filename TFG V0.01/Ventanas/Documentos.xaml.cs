using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace TFG_V0._01.Ventanas
{
    public partial class Documentos : Window, INotifyPropertyChanged
    {
        #region 🎬 variables animacion
        private Storyboard fadeInStoryboard;
        private Storyboard shakeStoryboard;
        #endregion

        #region Propiedades y Variables
        private bool isDarkMode = true;
        private bool isFiltrosPanelVisible = false;
        private readonly Duration animationDuration = new Duration(TimeSpan.FromSeconds(0.3));
        private readonly DoubleAnimation fadeInAnimation;
        private readonly DoubleAnimation fadeOutAnimation;
        private readonly DoubleAnimation shakeAnimation;

        public ObservableCollection<DocumentPanel> DocumentPanelsCollection { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
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
                string tipo = checkbox.Tag.ToString();
                bool isChecked = checkbox.IsChecked ?? false;

                // Si ningún checkbox está marcado, mostrar todos los paneles
                var anyCheckboxChecked = false;
                foreach (CheckBox cb in ((WrapPanel)((StackPanel)checkbox.Parent).Parent).Children)
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
                    // Actualizar visibilidad según los checkboxes marcados
                    foreach (var panel in DocumentPanelsCollection)
                    {
                        if (panel.Type == tipo)
                        {
                            panel.IsVisible = isChecked;
                        }
                        else if (!isChecked)
                        {
                            // Mantener visible si su checkbox correspondiente está marcado
                            var correspondingCheckbox = ((WrapPanel)((StackPanel)checkbox.Parent).Parent).Children
                                .OfType<CheckBox>()
                                .FirstOrDefault(cb => cb.Tag.ToString() == panel.Type);
                            panel.IsVisible = correspondingCheckbox?.IsChecked ?? false;
                        }
                    }
                }

                // Reordenar paneles visibles
                ReorderVisiblePanels();
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

        private void DropZone_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            e.Handled = true;
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                // TODO: Implement file handling logic
            }
        }

        private void SelectFiles_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                Filter = "All Files|*.*|PDF Files|*.pdf|Image Files|*.jpg;*.jpeg;*.png|Audio Files|*.mp3;*.wav|Video Files|*.mp4;*.avi"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // TODO: Implement file selection handling
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is string filePath)
            {
                try
                {
                    System.Diagnostics.Process.Start(filePath);
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
            if (button?.Tag is string filePath)
            {
                var result = MessageBox.Show("¿Está seguro de que desea eliminar este archivo?", 
                    "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                        // TODO: Update UI to reflect deletion
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar el archivo: {ex.Message}", 
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ComboClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Implement client selection change logic
        }

        private void ComboCasos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Implement case selection change logic
        }

        private void FiltroFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Implement date filter change logic
        }
    }

    public class DocumentPanel : INotifyPropertyChanged
    {
        private string _title;
        private string _type;
        private bool _isVisible;
        private bool _isOddPanel;
        private ObservableCollection<DocumentFile> _files;

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DocumentPanel()
        {
            Files = new ObservableCollection<DocumentFile>();
        }

        #region Eventos de UI
        private void DropZone_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            e.Handled = true;
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                // TODO: Implement file handling logic
            }
        }

        private void SelectFiles_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                Filter = "All Files|*.*|PDF Files|*.pdf|Image Files|*.jpg;*.jpeg;*.png|Audio Files|*.mp3;*.wav|Video Files|*.mp4;*.avi"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // TODO: Implement file selection handling
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is string filePath)
            {
                try
                {
                    System.Diagnostics.Process.Start(filePath);
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
            if (button?.Tag is string filePath)
            {
                var result = MessageBox.Show("¿Está seguro de que desea eliminar este archivo?", 
                    "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                        // TODO: Update UI to reflect deletion
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar el archivo: {ex.Message}", 
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ComboClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Implement client selection change logic
        }

        private void ComboCasos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Implement case selection change logic
        }

        private void FiltroFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Implement date filter change logic
        }
        #endregion
    }
    public class DocumentFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
    }
}
