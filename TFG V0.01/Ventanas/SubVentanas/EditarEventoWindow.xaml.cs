using System;
using System.Collections.Generic;
using System.Windows;
using TFG_V0._01.Supabase.Models;
using System.Windows.Input;

namespace TFG_V0._01.Ventanas.SubVentanas
{
    public partial class EditarEventoWindow : Window
    {
        public string TituloEvento => txtTitulo.Text;
        public string DescripcionEvento => txtDescripcion.Text;
        public DateTime? HoraMinuto => timePickerHoraMinuto.SelectedTime;
        public EstadoEvento EstadoSeleccionado => cbEstadoEvento.SelectedItem as EstadoEvento;

        public EditarEventoWindow(List<EstadoEvento> estados, string titulo = "", string descripcion = "", DateTime? horaMinuto = null, int? idEstado = null)
        {
            InitializeComponent();
            txtTitulo.Text = titulo;
            txtDescripcion.Text = descripcion;
            timePickerHoraMinuto.SelectedTime = horaMinuto ?? DateTime.Now;
            cbEstadoEvento.ItemsSource = estados;
            if (idEstado.HasValue)
                cbEstadoEvento.SelectedValue = idEstado.Value;
            else if (estados.Count > 0)
                cbEstadoEvento.SelectedIndex = 0;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitulo.Text))
            {
                MessageBox.Show("El título es obligatorio.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (cbEstadoEvento.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un estado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            this.DialogResult = true;
            this.Close();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
} 