using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using TFG_V0._01.Supabase.Models;
using Microsoft.Win32;

namespace TFG_V0._01.Ventanas.SubVentanas
{
    public partial class DetallesDocumentoWindow : Window
    {
        private readonly Documento _documento;

        public DetallesDocumentoWindow(Documento documento)
        {
            InitializeComponent();
            _documento = documento;
            // Crear un objeto anónimo con propiedades extra para el binding
            DataContext = new
            {
                documento.nombre,
                documento.fecha_subid,
                documento.ruta,
                documento.descripcion,
                documento.tipo_documento,
                TamanoHumano = ObtenerTamanoHumano(documento.ruta)
            };
        }

        private string ObtenerTamanoHumano(string ruta)
        {
            try
            {
                if (File.Exists(ruta))
                {
                    long length = new FileInfo(ruta).Length;
                    if (length < 1024) return $"{length} bytes";
                    if (length < 1024 * 1024) return $"{length / 1024.0:F2} KB";
                    if (length < 1024 * 1024 * 1024) return $"{length / 1024.0 / 1024.0:F2} MB";
                    return $"{length / 1024.0 / 1024.0 / 1024.0:F2} GB";
                }
                else
                {
                    return "Archivo no encontrado";
                }
            }
            catch
            {
                return "Error al obtener tamaño";
            }
        }

        private void Examinar_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Todos los archivos|*.*",
                Title = "Seleccionar archivo"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _documento.ruta = openFileDialog.FileName;
                // Actualizar el binding
                DataContext = new
                {
                    _documento.nombre,
                    _documento.fecha_subid,
                    _documento.ruta,
                    _documento.descripcion,
                    _documento.tipo_documento,
                    TamanoHumano = ObtenerTamanoHumano(_documento.ruta)
                };
            }
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            // Aquí implementarías la lógica para guardar los cambios
            DialogResult = true;
            Close();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region Window Controls
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        #endregion
    }
} 