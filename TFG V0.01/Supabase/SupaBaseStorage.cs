using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using global::Supabase;
using Supabase.Storage;
using Supabase.Storage.Exceptions;
using Supabase.Storage.Interfaces;


namespace TFG_V0._01.Supabase
{
    internal class SupaBaseStorage
    {
        private const int MaxFileSize = 50 * 1024 * 1024; // 50MB
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".txt" };
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".avi", ".mov", ".wmv" };
        private static readonly string[] AllowedAudioExtensions = { ".mp3", ".wav", ".ogg", ".m4a" };

        private readonly global::Supabase.Client _client;
        private readonly IStorageClient<Bucket, FileObject> _storageClient;
        private readonly string _bucketName;

        public SupaBaseStorage()
        {
            _client = new global::Supabase.Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
            _storageClient = _client.Storage;
            _bucketName = "documentos";
        }

        public async Task InicializarAsync()
        {
            try
            {
                await _client.InitializeAsync();
            }
            catch (Exception ex)
            {
                throw new SupabaseStorageException("Error al inicializar el cliente Supabase.", ex);
            }
        }

        public async Task<string> SubirArchivoAsync(string bucketName, string filePath, string fileName)
        {
            try
            {
                ValidateFile(filePath, fileName);

                if (string.IsNullOrWhiteSpace(bucketName))
                    throw new ArgumentException("El nombre del cubo no puede estar vacío.");

                var bucket = _storageClient.From(bucketName);
                using var fileStream = File.OpenRead(filePath);
                var fileBytes = await File.ReadAllBytesAsync(filePath);
                var result = await bucket.Upload(fileBytes, fileName);
                return result;
            }
            catch (Exception ex)
            {
                throw new SupabaseStorageException("Error al subir el archivo a Supabase.", ex);
            }
        }

        public async Task<string> SubirArchivoAutomaticoAsync(string filePath)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                var bucket = _storageClient.From(_bucketName);
                var fileBytes = await File.ReadAllBytesAsync(filePath);
                var result = await bucket.Upload(fileBytes, fileName);
                return result;
            }
            catch (Exception ex)
            {
                throw new SupabaseStorageException("Error al subir archivo automáticamente.", ex);
            }
        }

        public async Task<byte[]> DescargarArchivoAsync(string bucketName, string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bucketName) || string.IsNullOrWhiteSpace(fileName))
                    throw new ArgumentException("El nombre del cubo o archivo no puede estar vacío.");

                var bucket = _storageClient.From(bucketName);
                var result = await bucket.Download(fileName, (TransformOptions)null);
                return result;
            }
            catch (Exception ex)
            {
                throw new SupabaseStorageException("Error al descargar el archivo desde Supabase.", ex);
            }
        }

        public string ObtenerUrlPublica(string bucketName, string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bucketName) || string.IsNullOrWhiteSpace(fileName))
                    throw new ArgumentException("El nombre del cubo o archivo no puede estar vacío.");

                var bucket = _storageClient.From(bucketName);
                return bucket.GetPublicUrl(fileName);
            }
            catch (Exception ex)
            {
                throw new SupabaseStorageException("Error al obtener la URL pública del archivo.", ex);
            }
        }

        public async Task EliminarArchivoAsync(string bucketName, string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bucketName) || string.IsNullOrWhiteSpace(fileName))
                    throw new ArgumentException("El nombre del cubo o archivo no puede estar vacío.");

                var bucket = _storageClient.From(bucketName);
                await bucket.Remove(new List<string> { fileName });
            }
            catch (Exception ex)
            {
                throw new SupabaseStorageException("Error al eliminar el archivo de Supabase.", ex);
            }
        }

        public async Task<List<FileObject>> ListarArchivosAsync(string bucketName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bucketName))
                    throw new ArgumentException("El nombre del cubo no puede estar vacío.");

                var bucket = _storageClient.From(bucketName);
                var result = await bucket.List();
                return result;
            }
            catch (Exception ex)
            {
                throw new SupabaseStorageException("Error al listar archivos del cubo.", ex);
            }
        }

        public async Task<FileObject> BuscarArchivoAsync(string fileName)
        {
            try
            {
                var bucket = _storageClient.From(_bucketName);
                var files = await bucket.List();
                return files.FirstOrDefault(f => f.Name == fileName);
            }
            catch (Exception ex)
            {
                throw new SupabaseStorageException("Error al buscar el archivo en Supabase.", ex);
            }
        }

        public async Task<FileObject> BuscarArchivoEnCuboAsync(string bucketName, string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bucketName) || string.IsNullOrWhiteSpace(fileName))
                    throw new ArgumentException("El nombre del cubo o archivo no puede estar vacío.");

                var bucket = _storageClient.From(bucketName);
                var files = await bucket.List();
                return files.FirstOrDefault(f => f.Name == fileName);
            }
            catch (Exception ex)
            {
                throw new SupabaseStorageException("Error al buscar el archivo en el cubo especificado.", ex);
            }
        }

        public async Task ActualizarArchivoAsync(string bucketName, string oldFileName, string newFilePath)
        {
            try
            {
                ValidateFile(newFilePath, oldFileName);

                if (string.IsNullOrWhiteSpace(bucketName))
                    throw new ArgumentException("El nombre del cubo no puede estar vacío.");

                await EliminarArchivoAsync(bucketName, oldFileName);
                await SubirArchivoAsync(bucketName, newFilePath, oldFileName);
            }
            catch (Exception ex)
            {
                throw new SupabaseStorageException("Error al actualizar el archivo en Supabase.", ex);
            }
        }

        public async Task<FileObject> ObtenerDetallesArchivoAsync(string bucketName, string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bucketName) || string.IsNullOrWhiteSpace(fileName))
                    throw new ArgumentException("El nombre del cubo o archivo no puede estar vacío.");

                var bucket = _storageClient.From(bucketName);
                var files = await bucket.List();
                return files.FirstOrDefault(f => f.Name == fileName);
            }
            catch (Exception ex)
            {
                throw new SupabaseStorageException("Error al obtener detalles del archivo.", ex);
            }
        }

        public async Task CrearCuboAsync(string bucketName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bucketName))
                    throw new ArgumentException("El nombre del cubo no puede estar vacío.");

                await _storageClient.CreateBucket(bucketName);
            }
            catch (Exception ex)
            {
                throw new SupabaseStorageException("Error al crear el cubo en Supabase.", ex);
            }
        }

        public async Task EliminarCuboAsync(string bucketName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bucketName))
                    throw new ArgumentException("El nombre del cubo no puede estar vacío.");

                await _storageClient.DeleteBucket(bucketName);
            }
            catch (Exception ex)
            {
                throw new SupabaseStorageException("Error al eliminar el cubo en Supabase.", ex);
            }
        }

        public string ObtenerCuboPorTipoArchivo(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".pdf" => "pdfs",
                ".doc" or ".docx" => "documentos",
                ".xls" or ".xlsx" => "hojas_calculo",
                ".jpg" or ".jpeg" or ".png" => "imagenes",
                _ => "otros"
            };
        }

        private void ValidateFile(string rutaArchivo, string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
                throw new ArgumentException("La ruta del archivo no puede estar vacía.");

            if (string.IsNullOrWhiteSpace(nombreArchivo))
                throw new ArgumentException("El nombre del archivo no puede estar vacío.");

            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException("El archivo no existe en la ruta especificada.");

            var fileInfo = new FileInfo(rutaArchivo);
            if (fileInfo.Length > MaxFileSize)
                throw new ArgumentException($"El archivo excede el tamaño máximo permitido de {MaxFileSize / 1024 / 1024}MB");

            string extension = Path.GetExtension(nombreArchivo).ToLower();
            if (!IsAllowedExtension(extension))
                throw new ArgumentException($"Tipo de archivo no permitido: {extension}");
        }

        private bool IsAllowedExtension(string extension)
        {
            return AllowedImageExtensions.Contains(extension) ||
                   AllowedDocumentExtensions.Contains(extension) ||
                   AllowedVideoExtensions.Contains(extension) ||
                   AllowedAudioExtensions.Contains(extension);
        }
    }
}
