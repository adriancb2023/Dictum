using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using TFG_V0._01.Supabase.Models;

namespace TFG_V0._01.Ventanas.SubVentanas
{
    public partial class DetallesDocumentoWindow : Window
    {
        public DetallesDocumentoWindow(Documento documento)
        {
            InitializeComponent();
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

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
} 