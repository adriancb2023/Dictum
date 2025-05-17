using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;

namespace TFG_V0._01.Supabase
{
    public class SupabaseClientes
    {
        private static readonly Client _client = new(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        private static readonly Lazy<Task> _initTask = new(() => _client.InitializeAsync());

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
            await _client.From<Cliente>().Insert(cliente);
        }

        public async Task ActualizarClienteAsync(Cliente cliente)
        {
            await EnsureInitializedAsync();
            await _client.From<Cliente>().Update(cliente);
        }

        public async Task EliminarClienteAsync(int id)
        {
            await EnsureInitializedAsync();
            await _client.From<Cliente>().Where(c => c.id == id).Delete();
        }
    }
}
