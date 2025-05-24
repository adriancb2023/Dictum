using System;
using System.Threading.Tasks;
using System.Windows;

namespace TFG_V0._01.Ventanas.SubVentanas
{
    public partial class AñadirClienteWindow : Window
    {
        public AñadirClienteWindow()
        {
            InitializeComponent();
            Loaded += AñadirClienteWindow_Loaded;
        }

        private void AñadirClienteWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set default date to today
                dpFechaContrato.SelectedDate = DateTime.Today;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la ventana: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidarCampos())
                    return;

                // TODO: Implementar la lógica de guardado cuando tengamos el servicio de Supabase
                MessageBox.Show("Cliente añadido correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el cliente: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtApellido1.Text))
            {
                MessageBox.Show("El primer apellido es obligatorio", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtApellido1.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDNI.Text))
            {
                MessageBox.Show("El DNI es obligatorio", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDNI.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtTelefono.Text))
            {
                MessageBox.Show("El teléfono es obligatorio", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTelefono.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("El email es obligatorio", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            if (!IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("El formato del email no es válido", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            if (dpFechaContrato.SelectedDate == null)
            {
                MessageBox.Show("La fecha de contrato es obligatoria", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpFechaContrato.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 