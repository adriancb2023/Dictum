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
        private static readonly Client _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        private static bool _inicializado = false;
        private static readonly object _lock = new();

        public async Task InicializarAsync()
        {
            if (!_inicializado)
            {
                lock (_lock)
                {
                    if (!_inicializado)
                    {
                        _client.InitializeAsync().Wait();
                        _inicializado = true;
                    }
                }
            }
        }

        public async Task<List<Cliente>> ObtenerClientesAsync(int limite = 1000)
        {
            await InicializarAsync();
            var result = await _client.From<Cliente>().Limit(limite).Get();
            return result.Models;
        }

        public async Task<Cliente?> ObtenerClientePorIdAsync(int id)
        {
            await InicializarAsync();
            var result = await _client.From<Cliente>().Where(x => x.id == id).Single();
            return result;
        }

        public async Task InsertarClienteAsync(Cliente cliente)
        {
            await InicializarAsync();
            await _client.From<Cliente>().Insert(cliente);
        }

        public async Task ActualizarClienteAsync(Cliente cliente)
        {
            await InicializarAsync();
            await _client.From<Cliente>().Update(cliente);
        }

        public async Task EliminarClienteAsync(int id)
        {
            await InicializarAsync();
            await _client.From<Cliente>().Where(c => c.id == id).Delete();
        }
    }
}
