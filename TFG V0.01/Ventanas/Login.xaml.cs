using System;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TFG_V0._01.Supabase;
using DrawingBrushes = System.Drawing.Brushes;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfImage = System.Windows.Controls.Image;

namespace TFG_V0._01.Ventanas
{
    public partial class Login : Window
    {
        #region variables
        private SupabaseAutentificacion _authService;
        private Storyboard fadeInStoryboard;
        private Storyboard shakeStoryboard;
        #endregion

        #region InitializeComponent
        public Login()
        {
            InitializeComponent();
            _authService = new SupabaseAutentificacion();
            // Inicializar animaciones
            InitializeAnimations();

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
            var icon = button.Template.FindName("ThemeIcon", button) as WpfImage;

            if (MainWindow.isDarkTheme)
            {
                icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/sol.png", UriKind.Relative));
                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/TFG V0.01;component/Recursos/Background/oscuro/main.png") as ImageSource;
            }
            else
            {
                icon.Source = new BitmapImage(new Uri("/TFG V0.01;component/Recursos/Iconos/luna2.png", UriKind.Relative)); 
                backgroundFondo.ImageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/TFG V0.01;component/Recursos/Background/claro/main.png") as ImageSource;
            }

            // Animación de transición
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                From = 0.7,
                To = 0.9,
                AutoReverse = true,
                Duration = TimeSpan.FromSeconds(0.3)
            };
            backgroundFondo.BeginAnimation(OpacityProperty, fadeAnimation);
        }
        #endregion

        #region Login
        private async void Loguearse(object sender, RoutedEventArgs e)
        {
            string email = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            // Validación básica
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MostrarError("Por favor, complete todos los campos");
                return;
            }

            try
            {
                // Animación de carga (cambiar el cursor)
                Mouse.OverrideCursor = Cursors.Wait;

                var resultado = await _authService.SignInAsync(email, password);

                // Restaurar cursor
                Mouse.OverrideCursor = null;

                // Animación de éxito
                DoubleAnimation successAnim = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                successAnim.Completed += (s, args) =>
                {
                    var home = new Home();
                    home.Show();
                    this.Close();
                };

                this.BeginAnimation(OpacityProperty, successAnim);
            }
            catch (Exception ex)
            {
                // Restaurar cursor
                Mouse.OverrideCursor = null;

                // Limpiar campos y mostrar error
                PasswordBox.Clear();
                MostrarError("Email o contraseña incorrectos");

                // Animar elementos con error
                ShakeElement(UsernameTextBox);
                ShakeElement(PasswordBox);
            }
        }

        private void MostrarError(string mensaje)
        {
            errorLogin.Text = mensaje;
            errorLogin.Visibility = Visibility.Visible;

            // Animación de aparición del error
            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3)
            };
            errorLogin.BeginAnimation(OpacityProperty, fadeIn);

            // Cambiar bordes a rojo
            UsernameTextBox.BorderBrush = WpfBrushes.Red;
            UsernameTextBox.BorderThickness = new Thickness(1);
            PasswordBox.BorderBrush = WpfBrushes.Red;
            PasswordBox.BorderThickness = new Thickness(1);
        }
        #endregion

        #region Registro
        private void irRegistrarse(object sender, RoutedEventArgs e)
        {
            // Animación de salida
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            fadeOut.Completed += (s, args) =>
            {
                Registro registro = new Registro();
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
                // Animación de carga
                Mouse.OverrideCursor = Cursors.Wait;

                var resultado = await _authService.SignInAsync(email, password);

                // Restaurar cursor
                Mouse.OverrideCursor = null;

                // Animación de salida
                DoubleAnimation fadeOut = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

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
                // Restaurar cursor
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
    }
}