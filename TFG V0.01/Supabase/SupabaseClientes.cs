using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using TFG_V0._01.Models;

namespace TFG_V0._01.Supabase
{
    public class SupabaseClientes
    {
        private static Client _client;
        private static Lazy<Task> _initTask;

        public SupabaseClientes()
        {
            _initTask = new Lazy<Task>(() => InicializarAsync());
        }

        private async Task EnsureInitializedAsync() => await _initTask.Value;

        public async Task<List<Cliente>> ObtenerClientesAsync(int limite = 1000)
        {
            await EnsureInitializedAsync();
            var result = await _client.From<Cliente>().Limit(limite).Get();
            return result.Models;
        }

        public async Task<Cliente?> ObtenerClientePorIdAsync(int id)
        {
            await EnsureInitializedAsync();
            var result = await _client.From<Cliente>().Where(x => x.id == id).Single();
            return result;
        }

        public async Task InsertarClienteAsync(Cliente cliente)
        {
            await EnsureInitializedAsync();
            Console.WriteLine($"ID antes de insertar: {cliente.id}");
            await _client.From<Cliente>().Insert(cliente);
        }

        public async Task ActualizarClienteAsync(Cliente cliente)
        {
            await EnsureInitializedAsync();

            await _client
                .From<Cliente>()
                .Where(x => x.id == cliente.id)
                .Set(x => x.nombre, cliente.nombre)
                .Set(x => x.apellido1, cliente.apellido1)
                .Set(x => x.apellido2, cliente.apellido2)
                .Set(x => x.email1, cliente.email1)
                .Set(x => x.email2, cliente.email2)
                .Set(x => x.telf1, cliente.telf1)
                .Set(x => x.telf2, cliente.telf2)
                .Set(x => x.direccion, cliente.direccion)
                .Set(x => x.fecha_contrato, cliente.fecha_contrato)
                .Update();
        }

        public async Task EliminarClienteAsync(int id)
        {
            await EnsureInitializedAsync();
            await _client.From<Cliente>().Where(c => c.id == id).Delete();
        }

        #region ☁ SUPABASE
        public async Task InicializarAsync()
        {
            if (_client == null)
            {
                _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
                await _client.InitializeAsync();
            }
        }
        #endregion

        public async Task<List<Cliente>> ObtenerTodosClientesManualAsync()
        {
            var config = ConfigHelper.GetConfiguration();
            var url = config["Supabase:Url"] + "/rest/v1/clientes";
            var apiKey = config["Supabase:AnonKey"];

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("apikey", apiKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Range-Unit", "items");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Range", "0-49999");

            var response = await client.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<List<Cliente>>(json);
        }
    }
}
