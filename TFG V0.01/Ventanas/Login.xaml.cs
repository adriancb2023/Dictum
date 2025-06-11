using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TFG_V0._01.Supabase;
using TFG_V0._01.Ventanas.SubVentanas;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfImage = System.Windows.Controls.Image;
using System.Windows.Documents;

namespace TFG_V0._01.Ventanas
{
    public partial class Login : Window
    {
        #region 🎨 Variables y Recursos
        private readonly SupabaseAutentificacion _authService;
        private Storyboard fadeInStoryboard;
        private Storyboard shakeStoryboard;
        private Storyboard meshAnimStoryboard;

        private const string ErrorCamposVacios = "Por favor, complete todos los campos";
        private const string ErrorCredenciales = "Email o contraseña incorrectos";
        private string _correoPlaceholder = "Email";
        private string _passPlaceholder = "Contraseña";
        // Brushes y fondo animado
        private RadialGradientBrush mesh1Brush;
        private RadialGradientBrush mesh2Brush;
        private DrawingBrush meshGradientBrush;
        #endregion

        #region ⚡ Inicialización
        public Login()
        {
            InitializeComponent();
            this.Tag = MainWindow.isDarkTheme; // Establecer el Tag inicial
            _authService = new SupabaseAutentificacion();
            CargarIdioma(MainWindow.idioma);
            InitializeAnimations();
            CrearFondoAnimado();
            AplicarTema();
            BeginFadeInAnimation();
            // Añadir PlaceholderAdorner a los campos
            Loaded += (s, e) =>
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(UsernameTextBox);
                if (adornerLayer != null)
                {
                    adornerLayer.Add(new PlaceholderAdorner(
                        UsernameTextBox,
                        _correoPlaceholder,
                        MainWindow.isDarkTheme ? Brushes.White : Brushes.Black,
                        MainWindow.isDarkTheme ? Brushes.WhiteSmoke : Brushes.DarkSlateGray,
                        14));
                }
                adornerLayer = AdornerLayer.GetAdornerLayer(PasswordBox);
                if (adornerLayer != null)
                {
                    adornerLayer.Add(new PlaceholderAdorner(
                        PasswordBox,
                        _passPlaceholder,
                        MainWindow.isDarkTheme ? Brushes.White : Brushes.Black,
                        MainWindow.isDarkTheme ? Brushes.WhiteSmoke : Brushes.DarkSlateGray,
                        14));
                }
                IniciarAnimacionMesh();
            };
        }
        #endregion

        #region 🎬 Animaciones y Efectos
        private void InitializeAnimations()
        {
            fadeInStoryboard = new Storyboard();
            var fadeIn = CrearFadeAnimation(0, 1, 0.5);
            Storyboard.SetTarget(fadeIn, this);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(nameof(Opacity)));
            fadeInStoryboard.Children.Add(fadeIn);

            shakeStoryboard = new Storyboard();
            var shakeAnimation = CrearShakeAnimation();
            shakeStoryboard.Children.Add(shakeAnimation);
        }

        private DoubleAnimation CrearFadeAnimation(double from, double to, double durationSeconds, bool autoReverse = false)
        {
            return new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                AutoReverse = autoReverse
            };
        }

        private DoubleAnimation CrearShakeAnimation()
        {
            return new DoubleAnimation
            {
                From = 0,
                To = 5,
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3),
                Duration = TimeSpan.FromSeconds(0.05)
            };
        }

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

        private void IniciarAnimacionMesh()
        {
            // Detener si ya existe
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
        #endregion

        #region 🌓 Gestión de Tema
        private void AplicarTema()
        {
            this.Tag = MainWindow.isDarkTheme; // Actualizar el Tag cuando cambia el tema
            var button = this.FindName("ThemeButton") as Button;
            var icon = button?.Template.FindName("ThemeIcon", button) as WpfImage;
            var closeButton = this.FindName("CloseButton") as Button;

            // Cambiar fondo mesh gradient
            if (MainWindow.isDarkTheme)
            {
                if (icon != null)
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/sol.png", UriKind.Relative));
                // Colores mesh oscuro
                mesh1Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#d2cdc6");
                mesh1Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#08a693");
                mesh2Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#3a4d5f");
                mesh2Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#272c3f");
                OverlayDark.Visibility = Visibility.Visible;
                Titulo.Foreground = Brushes.White;
                subTitulo.Foreground = Brushes.White;
                // Cambiar color de la X
                if (closeButton != null)
                    closeButton.Foreground = (Brush)this.FindResource("CloseButtonForegroundDark");
                // Cambiar recursos de color de campos
                var app = Application.Current;
                app.Resources["TextBoxBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#20FFFFFF"));
                app.Resources["TextBoxForegroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                app.Resources["TextBoxBorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAFFFFFF"));
            }
            else
            {
                if (icon != null)
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna.png", UriKind.Relative));
                // Colores mesh claro
                mesh1Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#de9cb8");
                mesh1Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#9dcde1");
                mesh2Brush.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString("#dc8eb8");
                mesh2Brush.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#98d3ec");
                OverlayDark.Visibility = Visibility.Collapsed;
                Titulo.Foreground = Brushes.Black;
                subTitulo.Foreground = Brushes.Black;
                // Cambiar color de la X
                if (closeButton != null)
                    closeButton.Foreground = (Brush)this.FindResource("CloseButtonForegroundLight");
                // Cambiar recursos de color de campos
                var app = Application.Current;
                app.Resources["TextBoxBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#20FFFFFF"));
                app.Resources["TextBoxForegroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222222"));
                app.Resources["TextBoxBorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAFFFFFF"));
            }

            // Aplicar colores a los campos del formulario
            UsernameTextBox.Foreground = (Brush)Application.Current.Resources["TextBoxForegroundBrush"];
            PasswordBox.Foreground = (Brush)Application.Current.Resources["TextBoxForegroundBrush"];

            // Actualizar los placeholders para todos los campos
            ActualizarPlaceholders(UsernameTextBox, _correoPlaceholder);
            ActualizarPlaceholders(PasswordBox, _passPlaceholder);

            IniciarAnimacionMesh();
        }

        private void ActualizarPlaceholders(Control control, string placeholderText)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(control);
            if (adornerLayer != null)
            {
                var adorners = adornerLayer.GetAdorners(control);
                if (adorners != null)
                {
                    foreach (var adorner in adorners)
                    {
                        if (adorner is PlaceholderAdorner placeholder)
                        {
                            placeholder.UpdateColors(
                                MainWindow.isDarkTheme ? Brushes.White : Brushes.Black,
                                MainWindow.isDarkTheme ? Brushes.WhiteSmoke : Brushes.DarkSlateGray
                            );
                        }
                    }
                }
            }
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.isDarkTheme = !MainWindow.isDarkTheme;
            AplicarTema();

            var fadeAnimation = CrearFadeAnimation(0.7, 0.9, 0.3, true);
        }
        #endregion

        #region 🔐 Autenticación
        private async void Loguearse(object sender, RoutedEventArgs e)
        {
            string email = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MostrarError(ErrorCamposVacios);
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var resultado = await _authService.SignInAsync(email, password);
                Mouse.OverrideCursor = null;

                if (resultado.User != null && resultado.User.Email == "root@root.com")
                {
                    var eleccionBBDD = new EleccionBBDD();
                    eleccionBBDD.Show();
                    MainWindow.isAdmin = true;
                    this.Close();
                }
                else
                {
                    var fadeOut = CrearFadeAnimation(1, 0, 0.3);
                    fadeOut.Completed += (s, args) =>
                    {
                        var home = new Home();
                        home.Show();
                        this.Close();
                    };
                    this.BeginAnimation(OpacityProperty, fadeOut);
                }
            }
            catch
            {
                Mouse.OverrideCursor = null;
                PasswordBox.Clear();
                MostrarError(ErrorCredenciales);
                ShakeElement(UsernameTextBox);
                ShakeElement(PasswordBox);
            }
        }

        private void MostrarError(string mensaje)
        {
            errorLogin.Text = mensaje;
            errorLogin.Visibility = Visibility.Visible;
            errorLogin.BeginAnimation(OpacityProperty, CrearFadeAnimation(0, 1, 0.3));
            UsernameTextBox.BorderBrush = WpfBrushes.Red;
            UsernameTextBox.BorderThickness = new Thickness(1);
            PasswordBox.BorderBrush = WpfBrushes.Red;
            PasswordBox.BorderThickness = new Thickness(1);
        }

        private void irRegistrarse(object sender, RoutedEventArgs e)
        {
            var fadeOut = CrearFadeAnimation(1, 0, 0.3);
            fadeOut.Completed += (s, args) =>
            {
                var registro = new Registro();
                registro.Show();
                this.Close();
            };
            this.BeginAnimation(OpacityProperty, fadeOut);
        }

        private async void saltarInicio(object sender, RoutedEventArgs e)
        {
            string email = "root@root.com";
            string password = "root";

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var resultado = await _authService.SignInAsync(email, password);
                Mouse.OverrideCursor = null;

                if (resultado.User != null && resultado.User.Email == "root@root.com")
                {
                    MainWindow.isAdmin = true;
                    var eleccionBBDD = new EleccionBBDD();
                    eleccionBBDD.Show();
                    this.Close();
                }
                else
                {
                    var fadeOut = CrearFadeAnimation(1, 0, 0.3);
                    fadeOut.Completed += (s, args) =>
                    {
                        var home = new Home();
                        home.Show();
                        this.Close();
                    };
                    this.BeginAnimation(OpacityProperty, fadeOut);
                }
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show($"Error al iniciar sesión:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region 🖱️ Eventos de UI
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            fadeOut.Completed += (s, args) => this.Close();
            this.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                // Restaurar estilo normal al obtener foco
                textBox.BorderBrush = WpfBrushes.Transparent;
                errorLogin.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Puedes agregar validación aquí si es necesario
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                // Restaurar estilo normal al obtener foco
                passwordBox.BorderBrush = WpfBrushes.Transparent;
                errorLogin.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Puedes agregar validación aquí si es necesario
        }
        #endregion

        #region 🌍 Gestión de Idiomas
        private void CargarIdioma(int idioma)
        {
            var idiomas = new (string IniciarSesion, string Registrarse, string Titulo, string SubTitulo, string Correo, string Pass, string Error)[]
            {
                ("Iniciar Sesión", "Registrarse", "Bienvenido", "Inicia sesión para continuar", "Email", "Contraseña", "Email o contraseña incorrectos."), // Español
                ("Log In", "Sign Up", "Welcome", "Log in to continue", "Email", "Password", "Incorrect email or password."), // Inglés
                ("Inicia Sessió", "Registra't", "Benvingut", "Inicia sessió per continuar", "Correu electrònic", "Contrasenya", "Correu o contrasenya incorrectes."), // Catalán
                ("Iniciar sesión", "Rexistrarse", "Benvido", "Inicia sesión para continuar", "Correo electrónico", "Contrasinal", "Correo ou contrasinal incorrectos."), // Gallego
                ("Saioa hasi", "Erregistratu", "Ongi etorri", "Jarraitzeko hasi saioa", "Posta elektronikoa", "Pasahitza", "Posta edo pasahitz okerra.") // Euskera
            };

            if (idioma < 0 || idioma >= idiomas.Length)
                idioma = 0;

            var textos = idiomas[idioma];

            btnIniciarSecion.Content = textos.IniciarSesion;
            btnRegistrarse.Content = textos.Registrarse;
            Titulo.Text = textos.Titulo;
            subTitulo.Text = textos.SubTitulo;
            errorLogin.Text = textos.Error;
            _correoPlaceholder = textos.Correo;
            _passPlaceholder = textos.Pass;
        }
        #endregion

        #region 🎨 Fondo Animado
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
            MainGrid.Background = meshGradientBrush;
        }
        #endregion
    }
}
