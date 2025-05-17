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
    public partial class Registro : Window
    {
        #region variables
        private readonly SupabaseAutentificacion _supabaseAutentificacion;
        private Storyboard fadeInStoryboard;
        private Storyboard shakeStoryboard;
        #endregion

        #region InitializeComponent
        public Registro()
        {
            InitializeComponent();
            _supabaseAutentificacion = new SupabaseAutentificacion();
            InitializeAnimations();
            CargarIdioma(MainWindow.idioma);
            AplicarModoSistema();
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

        #region Aplicar modo oscuro/claro cargado por sistema
        private void AplicarModoSistema()
        {
            var button = this.FindName("ThemeButton") as Button;
            var icon = button?.Template.FindName("ThemeIcon", button) as WpfImage;

            if (MainWindow.isDarkTheme)
            {
                if (icon != null)
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/sol.png", UriKind.Relative));
                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString(
                    "pack://application:,,,/TFG V0.01;component/Recursos/Background/oscuro/main.png") as ImageSource;
                Titulo.Foreground = WpfBrushes.White;
                Subtitulo.Foreground = WpfBrushes.White;
                correo.Foreground = WpfBrushes.White;
                Pass1.Foreground = WpfBrushes.White;
                Pass2.Foreground = WpfBrushes.White;
            }
            else
            {
                if (icon != null)
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna2.png", UriKind.Relative));
                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString(
                    "pack://application:,,,/TFG V0.01;component/Recursos/Background/claro/main.png") as ImageSource;
                Titulo.Foreground = WpfBrushes.Black;
                Subtitulo.Foreground = WpfBrushes.Black;
                correo.Foreground = WpfBrushes.Black;
                Pass1.Foreground = WpfBrushes.Black;
                Pass2.Foreground = WpfBrushes.Black;
            }
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.isDarkTheme = !MainWindow.isDarkTheme;
            AplicarModoSistema();
            var fadeAnimation = CrearFadeAnimation(0.7, 0.9, 0.3, true);
            backgroundFondo.BeginAnimation(OpacityProperty, fadeAnimation);
        }
        #endregion

        #region volver al login
        private void VolverLogin(object sender, RoutedEventArgs e)
        {
            var login = new Login();
            login.Show();
            this.Close();
        }
        #endregion

        #region Registro usuario
        private async void RegistrarUser(object sender, RoutedEventArgs e)
        {
            try
            {
                var email = UsernameTextBox.Text;
                var password = PasswordBox.Password;
                var confirmPassword = PasswordBox2.Password;

                if (password != confirmPassword)
                {
                    MessageBox.Show("Las contraseñas no coinciden");
                    ShakeElement(PasswordBox);
                    ShakeElement(PasswordBox2);
                    return;
                }

                var result = await _supabaseAutentificacion.SignUpAsync(email, password);
                MessageBox.Show("Registro exitoso. Confirme su correo antes de acceder.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en el registro: {ex.Message}");
            }
        }
        #endregion

        #region  metodos flotantes
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null && passwordBox.Password == "Contraseña")
            {
                passwordBox.Password = string.Empty;
                passwordBox.Foreground = WpfBrushes.Black;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null && string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                passwordBox.Password = "Contraseña";
                passwordBox.Foreground = WpfBrushes.Gray;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == "Usuario")
            {
                textBox.Text = string.Empty;
                textBox.Foreground = WpfBrushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Usuario";
                textBox.Foreground = WpfBrushes.Gray;
            }
        }
        #endregion

        #region Idiomas
        private void CargarIdioma(int idioma)
        {
            var idiomas = new (string Titulo, string Subtitulo, string Correo, string Pass1, string Pass2, string BtnRegistrarse, string BtnVolver)[]
            {
                ("Registro", "Crea una cuenta nueva", "Email", "Contraseña", "Repita la contraseña", "Registrarse", "Volver al login"), // Español
                ("Sign Up", "Create a new account", "Email", "Password", "Repeat password", "Sign Up", "Back to login"), // Inglés
                ("Registre", "Crea un compte nou", "Correu electrònic", "Contrasenya", "Repeteix la contrasenya", "Registra’t", "Torna a l'inici de sessió"), // Catalán
                ("Rexistro", "Crea unha conta nova", "Correo electrónico", "Contrasinal", "Repita o contrasinal", "Rexistrarse", "Volver ao login"), // Gallego
                ("Erregistroa", "Kontu berri bat sortu", "Posta elektronikoa", "Pasahitza", "Pasahitza berriro idatzi", "Erregistratu", "Itzuli saio-hasierara") // Euskera
            };

            if (idioma < 0 || idioma >= idiomas.Length)
                idioma = 0;

            var textos = idiomas[idioma];

            Titulo.Text = textos.Titulo;
            Subtitulo.Text = textos.Subtitulo;
            correo.Text = textos.Correo;
            Pass1.Text = textos.Pass1;
            Pass2.Text = textos.Pass2;
            btnRegistrarse.Content = textos.BtnRegistrarse;
            btnVolver.Content = textos.BtnVolver;
        }
        #endregion
    }
}
