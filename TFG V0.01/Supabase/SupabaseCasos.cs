using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace TFG_V0._01.Supabase
{
    public class SupabaseCasos
    {
        private readonly Client _client;
        private readonly SupabaseClientes _clientesService;
        public readonly SupabaseEstados _estadosService;
        public readonly SupabaseTiposCaso _tiposCasoService;

        private bool _inicializado = false;
        private readonly SemaphoreSlim _initSemaphore = new(1, 1);

        public SupabaseCasos()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
            _clientesService = new SupabaseClientes();
            _estadosService = new SupabaseEstados();
            _tiposCasoService = new SupabaseTiposCaso();
        }

        public async Task InicializarAsync()
        {
            if (_inicializado) return;
            await _initSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_inicializado) return;
                await _client.InitializeAsync().ConfigureAwait(false);
                await Task.WhenAll(
                    _estadosService.InicializarAsync(),
                    _tiposCasoService.InicializarAsync()
                ).ConfigureAwait(false);
                _inicializado = true;
            }
            finally
            {
                _initSemaphore.Release();
            }
        }

        public async Task<List<Caso>> ObtenerTodosAsync()
        {
            await InicializarAsync().ConfigureAwait(false);

            var allCasos = new List<Caso>();
            const int pageSize = 1000;
            int offset = 0;
            while (true)
            {
                var casos = await _client.From<Caso>().Range(offset, offset + pageSize - 1).Get().ConfigureAwait(false);
                if (casos.Models.Count == 0)
                    break;
                allCasos.AddRange(casos.Models);
                if (casos.Models.Count < pageSize)
                    break;
                offset += pageSize;
            }

            // Cargar datos relacionados en paralelo
            var clientesTask = _clientesService.ObtenerClientesAsync();
            var estadosTask = _estadosService.ObtenerTodosAsync();
            var tiposTask = _tiposCasoService.ObtenerTodosAsync();
            await Task.WhenAll(clientesTask, estadosTask, tiposTask).ConfigureAwait(false);

            var clientes = await clientesTask.ConfigureAwait(false);
            var estados = await estadosTask.ConfigureAwait(false);
            var tipos = await tiposTask.ConfigureAwait(false);

            foreach (var caso in allCasos)
            {
                caso.Cliente = clientes.FirstOrDefault(c => c.id == caso.id_cliente);
                caso.Estado = estados.FirstOrDefault(e => e.id == caso.id_estado);
                caso.TipoCaso = tipos.FirstOrDefault(t => t.id == caso.id_tipo_caso);
            }

            return allCasos;
        }

        public async Task<Caso> ObtenerPorIdAsync(int id)
        {
            await InicializarAsync().ConfigureAwait(false);

            var caso = await _client.From<Caso>().Where(x => x.id == id).Single().ConfigureAwait(false);
            if (caso != null)
            {
                var clienteTask = _clientesService.ObtenerClientePorIdAsync(caso.id_cliente);
                var estadoTask = _estadosService.ObtenerPorIdAsync(caso.id_estado);
                var tipoTask = _tiposCasoService.ObtenerPorIdAsync(caso.id_tipo_caso);
                await Task.WhenAll(clienteTask, estadoTask, tipoTask).ConfigureAwait(false);

                caso.Cliente = await clienteTask.ConfigureAwait(false);
                caso.Estado = await estadoTask.ConfigureAwait(false);
                caso.TipoCaso = await tipoTask.ConfigureAwait(false);
            }
            return caso;
        }

        public async Task InsertarAsync(Caso caso)
        {
            await InicializarAsync().ConfigureAwait(false);
            await _client.From<Caso>().Insert(caso).ConfigureAwait(false);
        }

        public async Task ActualizarAsync(Caso caso)
        {
            await InicializarAsync().ConfigureAwait(false);
            await _client.From<Caso>()
                .Where(x => x.id == caso.id)
                .Set(x => x.titulo, caso.titulo)
                .Set(x => x.descripcion, caso.descripcion)
                .Set(x => x.fecha_inicio, caso.fecha_inicio)
                .Set(x => x.id_estado, caso.id_estado)
                .Set(x => x.id_cliente, caso.id_cliente)
                .Set(x => x.id_tipo_caso, caso.id_tipo_caso)
                .Set(x => x.referencia, caso.referencia)
                .Update().ConfigureAwait(false);
        }

        public async Task EliminarAsync(int id)
        {
            await InicializarAsync().ConfigureAwait(false);
            await _client.From<Caso>().Where(x => x.id == id).Delete().ConfigureAwait(false);
        }

        public async Task<List<Caso>> ObtenerTodosCasosManualAsync()
        {
            var config = ConfigHelper.GetConfiguration();
            var url = config["Supabase:Url"] + "/rest/v1/casos";
            var apiKey = config["Supabase:AnonKey"];

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("apikey", apiKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Range-Unit", "items");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Range", "0-49999");

            var response = await client.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<List<Caso>>(json);
        }

        public async Task<List<Caso>> ObtenerCasosActivosPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            await InicializarAsync().ConfigureAwait(false);

            var estados = await _estadosService.ObtenerTodosAsync().ConfigureAwait(false);
            var idsEstadosActivos = estados
                .Where(e => e.nombre.Equals("abierto", StringComparison.OrdinalIgnoreCase) ||
                            e.nombre.Equals("en proceso", StringComparison.OrdinalIgnoreCase))
                .Select(e => e.id)
                .ToList();

            var casos = await _client.From<Caso>()
                .Where(x => idsEstadosActivos.Contains(x.id_estado) &&
                            x.fecha_inicio >= fechaInicio &&
                            x.fecha_inicio < fechaFin)
                .Get().ConfigureAwait(false);

            var clientesTask = _clientesService.ObtenerClientesAsync();
            var tiposTask = _tiposCasoService.ObtenerTodosAsync();
            await Task.WhenAll(clientesTask, tiposTask).ConfigureAwait(false);

            var clientes = await clientesTask.ConfigureAwait(false);
            var tipos = await tiposTask.ConfigureAwait(false);

            foreach (var caso in casos.Models)
            {
                caso.Cliente = clientes.FirstOrDefault(c => c.id == caso.id_cliente);
                caso.Estado = estados.FirstOrDefault(e => e.id == caso.id_estado);
                caso.TipoCaso = tipos.FirstOrDefault(t => t.id == caso.id_tipo_caso);
            }

            return casos.Models;
        }
    }

    public class CasoUpdateDto
    {
        public string titulo { get; set; }
        public string descripcion { get; set; }
        public DateTime fecha_inicio { get; set; }
        public int id_estado { get; set; }
        public int id_cliente { get; set; }
        public int id_tipo_caso { get; set; }
        public string referencia { get; set; }
    }

    public static class ConfigHelper
    {
        public static IConfigurationRoot GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            return builder.Build();
        }
    }
}
