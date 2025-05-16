using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public SupabaseCasos()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
            _clientesService = new SupabaseClientes();
            _estadosService = new SupabaseEstados();
            _tiposCasoService = new SupabaseTiposCaso();
        }

        public async Task InicializarAsync()
        {
            await _client.InitializeAsync();
            await _clientesService.InicializarAsync();
            await _estadosService.InicializarAsync();
            await _tiposCasoService.InicializarAsync();
        }

        public async Task<List<Caso>> ObtenerTodosAsync()
        {
            var allCasos = new List<Caso>();
            int pageSize = 50000;
            int offset = 0;
            while (true)
            {
                var casos = await _client.From<Caso>().Range(offset, offset + pageSize - 1).Get();
                if (casos.Models.Count == 0)
                    break;
                allCasos.AddRange(casos.Models);
                if (casos.Models.Count < pageSize)
                    break;
                offset += pageSize;
            }
            var clientes = await _clientesService.ObtenerClientesAsync();
            var estados = await _estadosService.ObtenerTodosAsync();
            var tipos = await _tiposCasoService.ObtenerTodosAsync();

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
            var caso = await _client.From<Caso>().Where(x => x.id == id).Single();
            if (caso != null)
            {
                caso.Cliente = await _clientesService.ObtenerClientePorIdAsync(caso.id_cliente);
                caso.Estado = await _estadosService.ObtenerPorIdAsync(caso.id_estado);
                caso.TipoCaso = await _tiposCasoService.ObtenerPorIdAsync(caso.id_tipo_caso);
            }
            return caso;
        }

        public async Task InsertarAsync(Caso caso)
        {
            await _client.From<Caso>().Insert(caso);
        }

        public async Task ActualizarAsync(Caso caso)
        {
            await _client.From<Caso>()
                .Where(x => x.id == caso.id)
                .Set(x => x.titulo, caso.titulo)
                .Set(x => x.descripcion, caso.descripcion)
                .Set(x => x.fecha_inicio, caso.fecha_inicio)
                .Set(x => x.id_estado, caso.id_estado)
                .Set(x => x.id_cliente, caso.id_cliente)
                .Set(x => x.id_tipo_caso, caso.id_tipo_caso)
                .Set(x => x.referencia, caso.referencia)
                .Update();
        }

        public async Task EliminarAsync(int id)
        {
            await _client.From<Caso>().Where(x => x.id == id).Delete();
        }

        public async Task<List<Caso>> ObtenerTodosCasosManualAsync()
        {
            var config = ConfigHelper.GetConfiguration();
            var url = config["Supabase:Url"] + "/rest/v1/casos";
            var apiKey = config["Supabase:AnonKey"];

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("apikey", apiKey);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Range-Unit", "items");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Range", "0-49999");

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var casos = JsonConvert.DeserializeObject<List<Caso>>(json);
                return casos;
            }
        }

        public async Task<List<Caso>> ObtenerCasosActivosPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            await InicializarAsync();
            var todos = await ObtenerTodosAsync();

            // Filtrar por estado y rango de fechas
            var activos = todos
                .Where(c =>
                    (c.estado_nombre.Equals("abierto", StringComparison.OrdinalIgnoreCase) ||
                     c.estado_nombre.Equals("en proceso", StringComparison.OrdinalIgnoreCase)) &&
                    c.fecha_inicio >= fechaInicio &&
                    c.fecha_inicio < fechaFin)
                .ToList();

            return activos;
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

    // Clase auxiliar para leer la configuración desde appsettings.json
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
