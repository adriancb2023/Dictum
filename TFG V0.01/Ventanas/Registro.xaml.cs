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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;
using System;
using System.ComponentModel.DataAnnotations;
using TFG_V0._01.Supabase;
using DrawingBrushes = System.Drawing.Brushes;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfImage = System.Windows.Controls.Image;

namespace TFG_V0._01.Ventanas
{
    /// <summary>
    /// Lógica de interacción para Registro.xaml
    /// </summary>
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
            // Inicializar animaciones
            InitializeAnimations();
            cargarIdioma(MainWindow.idioma);

            // Aplicar tema
            AplicarModoSistema();

            // Animar entrada
            BeginFadeInAnimation();
        }
        #endregion

        #region Animaciones
        private void InitializeAnimations()
        {
            // Animación de entrada con fade
            fadeInStoryboard = new Storyboard();
            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            Storyboard.SetTarget(fadeIn, this);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity"));
            fadeInStoryboard.Children.Add(fadeIn);

            // Animación de shake para error
            shakeStoryboard = new Storyboard();
            DoubleAnimation shakeAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3),
                Duration = TimeSpan.FromSeconds(0.05)
            };

            shakeStoryboard.Children.Add(shakeAnimation);
        }

        private void BeginFadeInAnimation()
        {
            this.Opacity = 0;
            fadeInStoryboard.Begin();
        }

        private void ShakeElement(FrameworkElement element)
        {
            TranslateTransform trans = new TranslateTransform();
            element.RenderTransform = trans;

            DoubleAnimation anim = new DoubleAnimation
            {
                From = 0,
                To = 5,
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3),
                Duration = TimeSpan.FromSeconds(0.05)
            };

            trans.BeginAnimation(TranslateTransform.XProperty, anim);
        }
        #endregion

        #region Aplicar modo oscuro/claro cargado por sistema
        private void AplicarModoSistema()
        {
            var button = this.FindName("ThemeButton") as Button;
            var icon = button?.Template.FindName("ThemeIcon", button) as WpfImage;

            if (MainWindow.isDarkTheme)
            {
                // Aplicar modo oscuro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/sol.png", UriKind.Relative));
                }
                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/TFG V0.01;component/Recursos/Background/oscuro/main.png") as ImageSource;
            }
            else
            {
                // Aplicar modo claro
                if (icon != null)
                {
                    icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna2.png", UriKind.Relative));
                }
                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/TFG V0.01;component/Recursos/Background/claro/main.png") as ImageSource;
            }
        }
        #endregion

        #region modo oscuro/claro
        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.isDarkTheme = !MainWindow.isDarkTheme;
            var button = sender as Button;
            var icon = button.Template.FindName("ThemeIcon", button) as Image;
            if (MainWindow.isDarkTheme)
            {
                icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/sol.png", UriKind.Relative));
                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/TFG V0.01;component/Recursos/Background/oscuro/main.png") as ImageSource;
                Titulo.Foreground = Brushes.White;
                Subtitulo.Foreground = Brushes.White;
                correo.Foreground = Brushes.White;
                Pass1.Foreground = Brushes.White;
                Pass2.Foreground = Brushes.White;
            }
            else
            {
                icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna.png", UriKind.Relative));
                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/TFG V0.01;component/Recursos/Background/claro/main.png") as ImageSource;
            }
        }
        #endregion

        #region volver al login
        private void VolverLogin(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
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
            {
                this.DragMove(); // Permite mover la ventana al arrastrar el borde
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Cierra la ventana
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null && passwordBox.Password == "Contraseña")
            {
                passwordBox.Password = string.Empty; // Limpia el texto predeterminado
                passwordBox.Foreground = Brushes.Black;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null && string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                passwordBox.Password = "Contraseña"; // Restaura el texto predeterminado
                passwordBox.Foreground = Brushes.Gray;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == "Usuario")
            {
                textBox.Text = string.Empty; // Limpia el texto predeterminado
                textBox.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Usuario"; // Restaura el texto predeterminado
                textBox.Foreground = Brushes.Gray;
            }
        }
        #endregion

        #region Idiomas
        private void cargarIdioma(int idioma)
        {
            switch (idioma)
            {
                case 0: // Español
                    Titulo.Text = "Registro";
                    Subtitulo.Text = "Crea una cuenta nueva";
                    correo.Text = "Email";
                    Pass1.Text = "Contraseña";
                    Pass2.Text = "Repita la contraseña";
                    btnRegistrarse.Content = "Registrarse";
                    btnVolver.Content = "Volver al login";
                    break;

                case 1: // Inglés
                    Titulo.Text = "Sign Up";
                    Subtitulo.Text = "Create a new account";
                    correo.Text = "Email";
                    Pass1.Text = "Password";
                    Pass2.Text = "Repeat password";
                    btnRegistrarse.Content = "Sign Up";
                    btnVolver.Content = "Back to login";
                    break;

                case 2: // Catalán
                    Titulo.Text = "Registre";
                    Subtitulo.Text = "Crea un compte nou";
                    correo.Text = "Correu electrònic";
                    Pass1.Text = "Contrasenya";
                    Pass2.Text = "Repeteix la contrasenya";
                    btnRegistrarse.Content = "Registra’t";
                    btnVolver.Content = "Torna a l'inici de sessió";
                    break;

                case 3: // Gallego
                    Titulo.Text = "Rexistro";
                    Subtitulo.Text = "Crea unha conta nova";
                    correo.Text = "Correo electrónico";
                    Pass1.Text = "Contrasinal";
                    Pass2.Text = "Repita o contrasinal";
                    btnRegistrarse.Content = "Rexistrarse";
                    btnVolver.Content = "Volver ao login";
                    break;

                case 4: // Euskera
                    Titulo.Text = "Erregistroa";
                    Subtitulo.Text = "Kontu berri bat sortu";
                    correo.Text = "Posta elektronikoa";
                    Pass1.Text = "Pasahitza";
                    Pass2.Text = "Pasahitza berriro idatzi";
                    btnRegistrarse.Content = "Erregistratu";
                    btnVolver.Content = "Itzuli saio-hasierara";
                    break;
                default:
                    Titulo.Text = "Registro";
                    Subtitulo.Text = "Crea una cuenta nueva";
                    correo.Text = "Email";
                    Pass1.Text = "Contraseña";
                    Pass2.Text = "Repita la contraseña";
                    btnRegistrarse.Content = "Registrarse";
                    btnVolver.Content = "Volver al login";
                    break;
            }
        }
        #endregion
    } 
}
