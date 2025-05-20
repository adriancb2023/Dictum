using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using TFG_V0._01.Supabase.Models;

namespace TFG_V0._01.Ventanas.SubVentanas
{
    public partial class VentanaDocumento : Window
    {
        public string Nombre => txtNombre.Text;
        public string Descripcion => txtDescripcion.Text;
        public string TipoDocumento => (cmbTipoDocumento.SelectedItem as ComboBoxItem)?.Content.ToString();
        public string RutaArchivo => txtRutaArchivo.Text;

        public VentanaDocumento(Documento documento = null)
        {
            InitializeComponent();

            if (documento != null)
            {
                txtNombre.Text = documento.nombre;
                txtDescripcion.Text = documento.descripcion;
                txtRutaArchivo.Text = documento.ruta;
                
                foreach (ComboBoxItem item in cmbTipoDocumento.Items)
                {
                    if (item.Content.ToString() == documento.tipo_documento)
                    {
                        cmbTipoDocumento.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                cmbTipoDocumento.SelectedIndex = 0;
            }
        }

        private void Examinar_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Documentos|*.pdf;*.doc;*.docx;*.txt|Todos los archivos|*.*",
                Title = "Seleccionar documento"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                txtRutaArchivo.Text = openFileDialog.FileName;
            }
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Por favor, ingrese un nombre para el documento.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtRutaArchivo.Text))
            {
                MessageBox.Show("Por favor, seleccione un archivo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 