using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TFG_V0._01.Supabase;
using TFG_V0._01.Supabase.Models;

namespace TFG_V0._01.Ventanas.SubVentanas
{
    public partial class NuevoContactoWindow : Window
    {
        private readonly SupabaseCasos _casosService;
        private readonly SupabaseContactos _contactosService;
        private List<Caso> _casos;

        public NuevoContactoWindow()
        {
            InitializeComponent();
            _casosService = new SupabaseCasos();
            _contactosService = new SupabaseContactos();
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                await _casosService.InicializarAsync();
                _casos = await _casosService.ObtenerTodosAsync();
                CasoComboBox.ItemsSource = _casos;

                // Predefined roles for contacts
                var roles = new List<string>
                {
                    "Abogado",
                    "Cliente",
                    "Testigo",
                    "Perito",
                    "Juez",
                    "Secretario Judicial",
                    "Otro"
                };
                RolComboBox.ItemsSource = roles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void GuardarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CasoComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Por favor, seleccione un caso.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(NombreTextBox.Text))
                {
                    MessageBox.Show("Por favor, ingrese el nombre del contacto.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (RolComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Por favor, seleccione un rol.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var casoSeleccionado = (Caso)CasoComboBox.SelectedItem;
                var nuevoContacto = new ContactoInsertDto
                {
                    id_caso = casoSeleccionado.id,
                    nombre = NombreTextBox.Text.Trim(),
                    tipo = RolComboBox.SelectedItem.ToString(),
                    telefono = TelefonoTextBox.Text.Trim(),
                    email = EmailTextBox.Text.Trim()
                };

                await _contactosService.InsertarAsync(nuevoContacto);

                MessageBox.Show("Contacto guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el contacto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 