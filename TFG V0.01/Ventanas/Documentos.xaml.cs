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
using System.Threading.Tasks;

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
        public ObservableCollection<Cliente> ListaClientes { get; set; } = new();
        public ObservableCollection<Caso> ListaCasosFiltrados { get; set; } = new();
        public ObservableCollection<Caso> ListaCasos { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public Documentos()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("es-ES");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es-ES");
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
            DocumentPanels.ItemsSource = DocumentPanelsCollection;
            InitializeMeshGradient();
            UpdateTheme();
            _ = CargarClientesAsync();
            ComboClientesPanel.SelectedItem = null;
            ComboClientes_SelectionChanged(null, null);
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
            DocumentPanelsCollection.Clear();
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

        private async void AplicarFiltros_Click(object sender, RoutedEventArgs e)
        {
            await FiltrarYMostrarDocumentosAsync();
            HideSlidePanelFiltros();
        }

        private void RestablecerFiltros_Click(object sender, RoutedEventArgs e)
        {
            // Desactivar temporalmente los eventos de los ComboBox
            ComboClientesPanel.SelectionChanged -= ComboClientes_SelectionChanged;
            ComboCasosPanel.SelectionChanged -= ComboCasosPanel_SelectionChanged;

            // Limpiar los filtros
            ComboClientesPanel.SelectedItem = null;
            ComboCasosPanel.SelectedItem = null;
            FiltroFechaPanel.SelectedDate = null;

            // Limpiar los checkboxes de tipo de documento
            foreach (CheckBox checkBox in FindVisualChildren<CheckBox>(SlidePanelFiltros))
            {
                checkBox.IsChecked = true;
            }

            // Limpiar las tarjetas de documentos
            if (DocumentPanels.ItemsSource != null)
            {
                var documentos = DocumentPanels.ItemsSource as ObservableCollection<DocumentPanel>;
                if (documentos != null)
                {
                    foreach (var panel in documentos)
                    {
                        panel.Files.Clear();
                    }
                }
            }

            // Actualizar la lista de casos
            ListaCasosFiltrados = new ObservableCollection<Caso>(ListaCasos);

            // Reactivar los eventos de los ComboBox
            ComboClientesPanel.SelectionChanged += ComboClientes_SelectionChanged;
            ComboCasosPanel.SelectionChanged += ComboCasosPanel_SelectionChanged;
        }

        private async void ComboClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cliente = ComboClientesPanel.SelectedItem as TFG_V0._01.Supabase.Models.Cliente;
            ListaCasosFiltrados.Clear();
            ComboCasosPanel.SelectedItem = null;

            var casosService = new TFG_V0._01.Supabase.SupabaseCasos();
            var casos = await casosService.ObtenerTodosAsync();

            // Actualizar ListaCasos con todos los casos
            ListaCasos.Clear();
            foreach (var caso in casos)
                ListaCasos.Add(caso);

            if (cliente != null)
            {
                // Solo los casos del cliente seleccionado
                foreach (var caso in casos.Where(c => c.id_cliente == cliente.id))
                    ListaCasosFiltrados.Add(caso);
            }
            else
            {
                // TODOS los casos si no hay cliente seleccionado
                foreach (var caso in casos)
                    ListaCasosFiltrados.Add(caso);
            }
            await FiltrarYMostrarDocumentosAsync();
        }

        private async void ComboCasosPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await FiltrarYMostrarDocumentosAsync();
        }

        private async Task FiltrarYMostrarDocumentosAsync()
        {
            var clienteSeleccionado = ComboClientesPanel.SelectedItem as TFG_V0._01.Supabase.Models.Cliente;
            var casoSeleccionado = ComboCasosPanel.SelectedItem as TFG_V0._01.Supabase.Models.Caso;
            var fechaSeleccionada = FiltroFechaPanel.SelectedDate;

            // Buscar el WrapPanel de tipos de documento de forma robusta
            var tipoDocWrapPanel = FindVisualChildren<WrapPanel>(SlidePanelFiltros)
                .FirstOrDefault(wp => wp.Children.OfType<CheckBox>().Any(cb => cb.Tag != null));
            var extensionesSeleccionadas = new List<string>();
            var incluirOtros = false;
            if (tipoDocWrapPanel != null)
            {
                foreach (var child in tipoDocWrapPanel.Children)
                {
                    if (child is CheckBox cb && cb.IsChecked == true && cb.Tag is string tipo)
                    {
                        switch (tipo)
                        {
                            case "PDF":
                                extensionesSeleccionadas.Add(".pdf");
                                break;
                            case "AUD":
                                extensionesSeleccionadas.AddRange(new[] { ".mp3", ".wav", ".ogg", ".aac", ".flac" });
                                break;
                            case "IMG":
                                extensionesSeleccionadas.AddRange(new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" });
                                break;
                            case "VID":
                                extensionesSeleccionadas.AddRange(new[] { ".mp4", ".avi", ".mov", ".wmv", ".mkv", ".webm" });
                                break;
                            case "OTR":
                                incluirOtros = true;
                                break;
                        }
                    }
                }
            }

            // Si no hay ningún filtro seleccionado, vacía todos los paneles y sal del método
            bool sinFiltros = clienteSeleccionado == null
                && casoSeleccionado == null
                && fechaSeleccionada == null
                && !(extensionesSeleccionadas.Any() || incluirOtros);

            if (sinFiltros)
            {
                foreach (var panel in DocumentPanelsCollection)
                {
                    panel.Files.Clear();
                    panel.IsVisible = true;
                }
                return;
            }

            var documentosService = new TFG_V0._01.Supabase.SupabaseDocumentos();
            var documentos = await documentosService.ObtenerTodosAsync();

            if (clienteSeleccionado != null)
            {
                documentos = documentos.Where(d => d.Caso != null && d.Caso.id_cliente == clienteSeleccionado.id).ToList();
                
                // Si se seleccionó un caso específico (y no es la opción "Todos los casos")
                if (casoSeleccionado != null && casoSeleccionado.id != -1)
                {
                    documentos = documentos.Where(d => d.id_caso == casoSeleccionado.id).ToList();
                }
            }
            else if (casoSeleccionado != null && casoSeleccionado.id != -1)
            {
                documentos = documentos.Where(d => d.id_caso == casoSeleccionado.id).ToList();
            }

            if (fechaSeleccionada != null)
                documentos = documentos.Where(d => d.fecha_subid.Date == fechaSeleccionada.Value.Date).ToList();

            if (extensionesSeleccionadas.Any() || incluirOtros)
            {
                documentos = documentos.Where(d =>
                    extensionesSeleccionadas.Contains(NormalizarExtension(d.extension_archivo))
                    || (incluirOtros && !EsExtensionClasica(d.extension_archivo))
                ).ToList();
            }

            // Si no hay tipos seleccionados y no está marcado 'Otros', muestra todos los paneles vacíos
            bool mostrarTodos = !extensionesSeleccionadas.Any() && !incluirOtros;

            foreach (var panel in DocumentPanelsCollection)
            {
                if (mostrarTodos ||
                    extensionesSeleccionadas.Any(ext => GetTipoPanelPorExtension(ext) == panel.Type) ||
                    (incluirOtros && panel.Type == "OTR"))
                {
                    panel.IsVisible = true;
                    panel.Files.Clear();
                }
                else
                {
                    panel.IsVisible = false;
                    panel.Files.Clear();
                }
            }

            // Añade archivos a los paneles visibles
            foreach (var doc in documentos)
            {
                string tipoPanel = GetTipoPanelPorExtension(doc.extension_archivo);
                var panel = DocumentPanelsCollection.FirstOrDefault(p => p.Type.Equals(tipoPanel, StringComparison.OrdinalIgnoreCase) && p.IsVisible);
                if (panel != null)
                {
                    var file = new DocumentFile
                    {
                        Id = doc.id?.ToString() ?? string.Empty,
                        Name = doc.nombre,
                        Path = doc.ruta,
                        Type = tipoPanel,
                        UploadDate = doc.fecha_subid,
                        Size = 0
                    };
                    panel.Files.Add(file);
                }
            }
        }

        // Normaliza la extensión a ".ext" en minúsculas
        private string NormalizarExtension(string ext)
        {
            if (string.IsNullOrWhiteSpace(ext)) return "";
            ext = ext.Trim().ToLower();
            if (!ext.StartsWith(".")) ext = "." + ext;
            return ext;
        }

        // Función auxiliar para saber si una extensión es clásica (PDF, audio, imagen, video)
        private bool EsExtensionClasica(string ext)
        {
            var e = NormalizarExtension(ext);
            return e == ".pdf"
                || new[] { ".mp3", ".wav", ".ogg", ".aac", ".flac" }.Contains(e)
                || new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" }.Contains(e)
                || new[] { ".mp4", ".avi", ".mov", ".wmv", ".mkv", ".webm" }.Contains(e);
        }

        // Función auxiliar para mapear extensión a tipo de panel
        private string GetTipoPanelPorExtension(string ext)
        {
            var e = NormalizarExtension(ext);
            if (e == ".pdf") return "PDF";
            if (new[] { ".mp3", ".wav", ".ogg", ".aac", ".flac" }.Contains(e)) return "AUD";
            if (new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" }.Contains(e)) return "IMG";
            if (new[] { ".mp4", ".avi", ".mov", ".wmv", ".mkv", ".webm" }.Contains(e)) return "VID";
            return "OTR";
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

        private bool PuedeGuardarDocumento()
        {
            var clienteSeleccionado = ComboClientesPanel.SelectedItem as TFG_V0._01.Supabase.Models.Cliente;
            var casoSeleccionado = ComboCasosPanel.SelectedItem as TFG_V0._01.Supabase.Models.Caso;

            if (clienteSeleccionado == null || casoSeleccionado == null)
            {
                MessageBox.Show("Debes seleccionar un cliente y un caso antes de guardar el documento.", "Selección requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private async Task GuardarDocumentoAsync(string filePath)
        {
            if (!PuedeGuardarDocumento())
                return;

            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
            {
                MessageBox.Show("Por favor, seleccione un archivo válido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var clienteSeleccionado = ComboClientesPanel.SelectedItem as TFG_V0._01.Supabase.Models.Cliente;
                var casoSeleccionado = ComboCasosPanel.SelectedItem as TFG_V0._01.Supabase.Models.Caso;

                // Subir archivo a Supabase Storage
                var storage = new TFG_V0._01.Supabase.SupaBaseStorage();
                await storage.InicializarAsync();
                string extension = System.IO.Path.GetExtension(filePath);
                string uniqueName = $"{System.IO.Path.GetFileNameWithoutExtension(filePath)}_{Guid.NewGuid()}{extension}";
                string storagePath = await storage.SubirArchivoAsync("documentos", filePath, uniqueName);

                // Guardar registro en la base de datos
                var documento = new TFG_V0._01.Supabase.Models.Documento.DocumentoInsertDto
                {
                    nombre = System.IO.Path.GetFileNameWithoutExtension(filePath),
                    ruta = storagePath,
                    fecha_subid = DateTime.Now,
                    id_caso = casoSeleccionado.id,
                    tipo_documento = 1, // Ajusta según tu lógica de tipos
                    extension_archivo = extension
                };

                var documentosService = new TFG_V0._01.Supabase.SupabaseDocumentos();
                await documentosService.InicializarAsync();
                await documentosService.InsertarAsync(documento);

                MessageBox.Show("Documento guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                // Aquí puedes refrescar la lista de documentos si lo necesitas
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el documento: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            if (!PuedeGuardarDocumento())
                return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    _ = GuardarDocumentoAsync(files[0]);
                }
            }
        }

        private void SelectFiles_Click(object sender, RoutedEventArgs e)
        {
            if (!PuedeGuardarDocumento())
                return;

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                Filter = "All Files|*.*|PDF Files|*.pdf|Image Files|*.jpg;*.jpeg;*.png;*.gif|Video Files|*.mp4;*.avi;*.mov|Audio Files|*.mp3;*.wav"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    _ = GuardarDocumentoAsync(file);
                }
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

        private async Task CargarClientesAsync()
        {
            var clientesService = new TFG_V0._01.Supabase.SupabaseClientes();
            var clientes = await clientesService.ObtenerClientesAsync();
            ListaClientes.Clear();
            foreach (var c in clientes)
                ListaClientes.Add(c);
        }

        // Helper para buscar visualmente hijos de un tipo
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T t)
                    {
                        yield return t;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void ComboBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo != null && !combo.IsDropDownOpen)
            {
                combo.IsDropDownOpen = true;
                e.Handled = true;
            }
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
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }
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