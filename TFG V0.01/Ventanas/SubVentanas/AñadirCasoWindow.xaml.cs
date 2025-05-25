using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TFG_V0._01.Supabase;
using TFG_V0._01.Supabase.Models;

namespace TFG_V0._01.Ventanas.SubVentanas
{
    public partial class AñadirCasoWindow : Window
    {
        private readonly SupabaseCasos _casosService;
        private readonly SupabaseClientes _clientesService;
        private readonly SupabaseEstados _estadosService;
        private readonly SupabaseTiposCaso _tiposCasoService;

        public AñadirCasoWindow()
        {
            InitializeComponent();
            _casosService = new SupabaseCasos();
            _clientesService = new SupabaseClientes();
            _estadosService = new SupabaseEstados();
            _tiposCasoService = new SupabaseTiposCaso();

            Loaded += AñadirCasoWindow_Loaded;
        }
         
        private async void AñadirCasoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarDatosAsync()
        {
            // Cargar clientes
            var clientes = await _clientesService.ObtenerClientesAsync();
            cmbClientes.ItemsSource = clientes;
            cmbClientes.DisplayMemberPath = "nombre";
            cmbClientes.SelectedValuePath = "id";

            // Cargar tipos de caso
            var tiposCaso = await _tiposCasoService.ObtenerTodosAsync();
            cmbTiposCaso.ItemsSource = tiposCaso;
            cmbTiposCaso.DisplayMemberPath = "nombre";
            cmbTiposCaso.SelectedValuePath = "id";

            // Cargar estados
            var estados = await _estadosService.ObtenerTodosAsync();
            cmbEstados.ItemsSource = estados;
            cmbEstados.DisplayMemberPath = "nombre";
            cmbEstados.SelectedValuePath = "id";

            // Establecer fecha actual por defecto
            dpFechaInicio.SelectedDate = DateTime.Now;
        }

        private async void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidarCampos())
                {
                    MessageBox.Show("Por favor, complete todos los campos requeridos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var nuevoCaso = new CasoInsertDto
                {
                    titulo = txtTitulo.Text.Trim(),
                    descripcion = txtDescripcion.Text.Trim(),
                    id_cliente = (int)cmbClientes.SelectedValue,
                    id_tipo_caso = (int)cmbTiposCaso.SelectedValue,
                    id_estado = (int)cmbEstados.SelectedValue,
                    fecha_inicio = dpFechaInicio.SelectedDate ?? DateTime.Now,
                    referencia = null // o lo que corresponda
                };

                await _casosService.InsertarAsync(nuevoCaso);
                MessageBox.Show("Caso creado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el caso: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarCampos()
        {
            return !string.IsNullOrWhiteSpace(txtTitulo.Text) &&
                   !string.IsNullOrWhiteSpace(txtDescripcion.Text) &&
                   cmbClientes.SelectedValue != null &&
                   cmbTiposCaso.SelectedValue != null &&
                   cmbEstados.SelectedValue != null &&
                   dpFechaInicio.SelectedDate != null;
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 