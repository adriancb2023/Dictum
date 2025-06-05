using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TFG_V0._01.Supabase;
using TFG_V0._01.Supabase.Models;
using System.Linq;

namespace TFG_V0._01.Ventanas.SubVentanas
{
    public partial class AñadirClienteWindow : UserControl
    {
        private readonly SupabaseClientes _supabaseClientes;

        public event EventHandler ClienteGuardado;
        public event EventHandler ClienteCancelado;

        public AñadirClienteWindow()
        {
            InitializeComponent();
            _supabaseClientes = new SupabaseClientes();
            CargarIdioma(MainWindow.idioma);
            Loaded += AñadirClienteWindow_Loaded;
        }

        private void CargarIdioma(int idioma)
        {
            var idiomas = new (string Titulo, string Nombre, string Apellido1, string Apellido2, string DNI, 
                string Telefono, string Telefono2, string Email, string Email2, string Direccion, 
                string FechaContrato, string Cancelar, string Guardar)[]
            {
                ("Añadir Nuevo Cliente", "Nombre:", "Primer Apellido:", "Segundo Apellido:", "DNI:", 
                "Teléfono:", "Segundo Teléfono (Opcional):", "Email:", "Segundo Email (Opcional):", "Dirección:", 
                "Fecha de Contrato:", "Cancelar", "Guardar"),
                ("Add New Client", "Name:", "First Surname:", "Second Surname:", "ID Number:", 
                "Phone:", "Second Phone (Optional):", "Email:", "Second Email (Optional):", "Address:", 
                "Contract Date:", "Cancel", "Save"),
                ("Afegir Nou Client", "Nom:", "Primer Cognom:", "Segon Cognom:", "DNI:", 
                "Telèfon:", "Segon Telèfon (Opcional):", "Correu:", "Segon Correu (Opcional):", "Adreça:", 
                "Data de Contracte:", "Cancel·lar", "Guardar"),
                ("Engadir Novo Cliente", "Nome:", "Primeiro Apelido:", "Segundo Apelido:", "DNI:", 
                "Teléfono:", "Segundo Teléfono (Opcional):", "Correo:", "Segundo Correo (Opcional):", "Dirección:", 
                "Data de Contrato:", "Cancelar", "Gardar"),
                ("Bezero Berria Gehitu", "Izena:", "Lehen Abizena:", "Bigarren Abizena:", "NAN:", 
                "Telefonoa:", "Bigarren Telefonoa (Aukerakoa):", "Posta:", "Bigarren Posta (Aukerakoa):", "Helbidea:", 
                "Kontratu Data:", "Utzi", "Gorde")
            };

            if (idioma < 0 || idioma >= idiomas.Length)
                idioma = 0;

            var t = idiomas[idioma];

            txtTitulo.Text = t.Titulo;
            txtLabelNombre.Text = t.Nombre;
            txtLabelApellido1.Text = t.Apellido1;
            txtLabelApellido2.Text = t.Apellido2;
            txtLabelDNI.Text = t.DNI;
            txtLabelTelefono.Text = t.Telefono;
            txtLabelTelefono2.Text = t.Telefono2;
            txtLabelEmail.Text = t.Email;
            txtLabelEmail2.Text = t.Email2;
            txtLabelDireccion.Text = t.Direccion;
            txtLabelFechaContrato.Text = t.FechaContrato;

            // Actualizar textos de los botones directamente
            btnCancelar.Content = t.Cancelar;
            btnGuardar.Content = t.Guardar;
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

        private async void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidarCampos())
                    return;

                var nuevoCliente = new ClienteInsertDto
                {
                    nombre = txtNombre.Text.Trim(),
                    apellido1 = txtApellido1.Text.Trim(),
                    apellido2 = string.IsNullOrWhiteSpace(txtApellido2.Text) ? null : txtApellido2.Text.Trim(),
                    dni = txtDNI.Text.Trim(),
                    email1 = txtEmail.Text.Trim(),
                    email2 = string.IsNullOrWhiteSpace(txtEmail2.Text) ? null : txtEmail2.Text.Trim(),
                    telf1 = txtTelefono.Text.Trim(),
                    telf2 = string.IsNullOrWhiteSpace(txtTelefono2.Text) ? null : txtTelefono2.Text.Trim(),
                    direccion = txtDireccion.Text.Trim(),
                    fecha_contrato = dpFechaContrato.SelectedDate ?? DateTime.Today
                };

                // Comprobar si el DNI ya existe
                var clientesExistentes = await _supabaseClientes.ObtenerClientesAsync();
                if (clientesExistentes.Any(c => c.dni != null && c.dni.Equals(nuevoCliente.dni, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"El DNI '{nuevoCliente.dni}' ya existe en la base de datos. Por favor, introduce un DNI único.", "DNI duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validar formato y letra del NIF (DNI o NIE)
                if (!IsValidNif(txtDNI.Text.Trim()))
                {
                    MessageBox.Show("El DNI/NIE introducido no es válido. Debe tener el formato y la letra correctos según el algoritmo oficial.", "Identificador no válido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _supabaseClientes.InsertarClienteAsync(nuevoCliente);
                MessageBox.Show("Cliente añadido correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                ClienteGuardado?.Invoke(this, EventArgs.Empty);
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

            if (string.IsNullOrWhiteSpace(txtDireccion.Text))
            {
                MessageBox.Show("La dirección es obligatoria", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDireccion.Focus();
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

        private bool IsValidDni(string dni)
        {
            return IsValidDniFormat(dni) && IsValidDniLetter(dni);
        }

        private bool IsValidDniFormat(string dni)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(dni, @"^\d{8}[A-HJ-NP-TV-Z]$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private bool IsValidDniLetter(string dni)
        {
            if (dni == null || dni.Length != 9) return false;
            string letras = "TRWAGMYFPDXBNJZSQVHLCKE";
            if (!int.TryParse(dni.Substring(0, 8), out int numero)) return false;
            char letraEsperada = letras[numero % 23];
            return char.ToUpper(dni[8]) == letraEsperada;
        }

        private bool IsValidNie(string nie)
        {
            if (nie == null || nie.Length != 9) return false;
            string letras = "TRWAGMYFPDXBNJZSQVHLCKE";
            char first = char.ToUpper(nie[0]);
            if (first != 'X' && first != 'Y' && first != 'Z') return false;
            string nieNum = nie;
            switch (first)
            {
                case 'X': nieNum = "0" + nie.Substring(1); break;
                case 'Y': nieNum = "1" + nie.Substring(1); break;
                case 'Z': nieNum = "2" + nie.Substring(1); break;
            }
            if (!int.TryParse(nieNum.Substring(0, 8), out int numero)) return false;
            char letraEsperada = letras[numero % 23];
            return char.ToUpper(nie[8]) == letraEsperada;
        }

        private bool IsValidNif(string nif)
        {
            return IsValidDni(nif) || IsValidNie(nif);
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            ClienteCancelado?.Invoke(this, EventArgs.Empty);
        }

        private void dpFechaContrato_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpFechaContrato.SelectedDate.HasValue)
            {
                // Aquí puedes manejar la fecha seleccionada si lo necesitas
                DateTime fechaSeleccionada = dpFechaContrato.SelectedDate.Value;
            }
        }
    }
} 