using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TFG_V0._01.Supabase;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfImage = System.Windows.Controls.Image;

namespace TFG_V0._01.Ventanas
{
    public partial class Login : Window
    {
        #region variables
        private readonly SupabaseAutentificacion _authService;
        private Storyboard fadeInStoryboard;
        private Storyboard shakeStoryboard;

        private const string ErrorCamposVacios = "Por favor, complete todos los campos";
        private const string ErrorCredenciales = "Email o contraseña incorrectos";
        #endregion

        #region InitializeComponent
        public Login()
        {
            InitializeComponent();
            _authService = new SupabaseAutentificacion();
            CargarIdioma(MainWindow.idioma);
            InitializeAnimations();
            AplicarTema();
            BeginFadeInAnimation();
        }
        #endregion

        #region Animaciones
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
        #endregion

        #region Tema oscuro/claro
        private void AplicarTema()
        {
            var button = this.FindName("ThemeButton") as Button;
            var icon = button?.Template.FindName("ThemeIcon", button) as WpfImage;

            if (MainWindow.isDarkTheme)
            {
                if (icon != null)
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/sol.png", UriKind.Relative));
                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString(
                    "pack://application:,,,/TFG V0.01;component/Recursos/Background/oscuro/main.png") as ImageSource;
            }
            else
            {
                if (icon != null)
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna2.png", UriKind.Relative));
                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString(
                    "pack://application:,,,/TFG V0.01;component/Recursos/Background/claro/main.png") as ImageSource;
            }
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.isDarkTheme = !MainWindow.isDarkTheme;
            AplicarTema();

            var fadeAnimation = CrearFadeAnimation(0.7, 0.9, 0.3, true);
            backgroundFondo.BeginAnimation(OpacityProperty, fadeAnimation);
        }
        #endregion

        #region Login
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

                var successAnim = CrearFadeAnimation(1, 0, 0.3);
                successAnim.Completed += (s, args) =>
                {
                    var home = new Home();
                    home.Show();
                    this.Close();
                };
                this.BeginAnimation(OpacityProperty, successAnim);
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
        #endregion

        #region Registro
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
        #endregion

        #region saltarse el inicio de sesión con root
        private async void saltarInicio(object sender, RoutedEventArgs e)
        {
            string email = "root@root.com";
            string password = "root";

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var resultado = await _authService.SignInAsync(email, password);
                Mouse.OverrideCursor = null;

                var fadeOut = CrearFadeAnimation(1, 0, 0.3);
                fadeOut.Completed += (s, args) =>
                {
                    var home = new Home();
                    home.Show();
                    this.Close();
                };
                this.BeginAnimation(OpacityProperty, fadeOut);
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show($"Error al iniciar sesión:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Eventos de UI
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

        #region Idiomas
        private void CargarIdioma(int idioma)
        {
            var idiomas = new (string IniciarSesion, string Registrarse, string Titulo, string SubTitulo, string Correo, string Pass, string Error)[]
            {
                ("Iniciar Sesión", "Registrarse", "Bienvenido", "Inicia sesión para continuar", "Email", "Contraseña", "Email o contraseña incorrectos."), // Español
                ("Log In", "Sign Up", "Welcome", "Log in to continue", "Email", "Password", "Incorrect email or password."), // Inglés
                ("Inicia Sessió", "Registra’t", "Benvingut", "Inicia sessió per continuar", "Correu electrònic", "Contrasenya", "Correu o contrasenya incorrectes."), // Catalán
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
            Correo.Text = textos.Correo;
            Pass.Text = textos.Pass;
            errorLogin.Text = textos.Error;
        }
        #endregion
    }
}
