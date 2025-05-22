using System;
using System.Windows;
using System.Windows.Controls;
using TFG_V0._01.Supabase.Models;
using TFG_V0._01.Supabase;

namespace TFG_V0._01.Ventanas.SubVentanas
{
    public partial class AñadirTareaWindow : Window
    {
        private int _idCaso;
        private readonly SupabaseTareas _tareasService;

        public AñadirTareaWindow(int idCaso)
        {
            InitializeComponent();
            _idCaso = idCaso;
            _tareasService = new SupabaseTareas();
            
            // Set default values
            dpFechaVencimiento.SelectedDate = DateTime.Now.AddDays(7);
            cmbPrioridad.SelectedIndex = 1; // Media por defecto
            cmbEstado.SelectedIndex = 0; // Pendiente por defecto
        }

        private async void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTitulo.Text))
                {
                    MessageBox.Show("Por favor, introduce un título para la tarea.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var tarea = new Tarea
                {
                    titulo = txtTitulo.Text,
                    descripcion = txtDescripcion.Text,
                    fecha_creacion = DateTime.Now,
                    fecha_vencimiento = dpFechaVencimiento.SelectedDate,
                    completada = false,
                    id_caso = _idCaso,
                    prioridad = (cmbPrioridad.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    estado = (cmbEstado.SelectedItem as ComboBoxItem)?.Content.ToString()
                };

                #region ☁ SUPABASE
                await _tareasService.CrearTarea(tarea);
                #endregion

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la tarea: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 