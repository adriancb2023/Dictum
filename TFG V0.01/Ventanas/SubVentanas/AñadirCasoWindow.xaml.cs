using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using TFG_V0._01.Supabase;
using TFG_V0._01.Supabase.Models;

namespace TFG_V0._01.Ventanas.SubVentanas
{
    public partial class AñadirCasoWindow : UserControl
    {
        private readonly SupabaseCasos _supabaseCasos;
        private readonly SupabaseClientes _supabaseClientes;
        private readonly SupabaseTiposCaso _supabaseTiposCaso;
        private readonly SupabaseEstados _supabaseEstados;

        public event EventHandler CasoGuardado;
        public event EventHandler CasoCancelado;

        public AñadirCasoWindow()
        {
            InitializeComponent();
            _supabaseCasos = new SupabaseCasos();
            _supabaseClientes = new SupabaseClientes();
            _supabaseTiposCaso = new SupabaseTiposCaso();
            _supabaseEstados = new SupabaseEstados();
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                // Cargar clientes
                var clientes = await _supabaseClientes.ObtenerClientesAsync();
                cmbClientes.ItemsSource = clientes;

                // Cargar tipos de caso
                var tiposCaso = await _supabaseTiposCaso.ObtenerTodosAsync();
                cmbTiposCaso.ItemsSource = tiposCaso;

                // Cargar estados
                var estados = await _supabaseEstados.ObtenerTodosAsync();
                cmbEstados.ItemsSource = estados;

                // Establecer valores por defecto
                if (cmbEstados.Items.Count > 0)
                    cmbEstados.SelectedIndex = 0;
                if (cmbTiposCaso.Items.Count > 0)
                    cmbTiposCaso.SelectedIndex = 0;
                dpFechaInicio.SelectedDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ResetFields()
        {
            txtTitulo.Clear();
            txtDescripcion.Clear();
            cmbClientes.SelectedItem = null;
            cmbTiposCaso.SelectedItem = null;
            cmbEstados.SelectedItem = null;
            dpFechaInicio.SelectedDate = DateTime.Now;
        }

        private async void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTitulo.Text))
                {
                    MessageBox.Show("Por favor, introduce un título para el caso.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbClientes.SelectedItem == null)
                {
                    MessageBox.Show("Por favor, selecciona un cliente.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var nuevoCaso = new CasoInsertDto
                {
                    titulo = txtTitulo.Text,
                    descripcion = txtDescripcion.Text,
                    id_cliente = (int)cmbClientes.SelectedValue,
                    id_tipo_caso = (int)cmbTiposCaso.SelectedValue,
                    id_estado = (int)cmbEstados.SelectedValue,
                    fecha_inicio = dpFechaInicio.SelectedDate ?? DateTime.Now,
                    referencia = $"C-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}"
                };

                await _supabaseCasos.InsertarAsync(nuevoCaso);
                CasoGuardado?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el caso: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            CasoCancelado?.Invoke(this, EventArgs.Empty);
        }
    }
} 