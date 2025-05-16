using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase;
using Supabase.Postgrest;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;


namespace TFG_V0._01.Supabase
{
    public class SupabaseClientes
    {
        private readonly Client _client;

        public SupabaseClientes()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public async Task InicializarAsync()
        {
            await _client.InitializeAsync();
        }

        public async Task<List<Cliente>> ObtenerClientesAsync()
        {
            var result = await _client.From<Cliente>().Limit(50000).Get();
            return result.Models;
        }

        public async Task<Cliente> ObtenerClientePorIdAsync(int id)
        {
            var result = await _client.From<Cliente>().Where(x => x.id == id).Single();
            return result;
        }

        public async Task InsertarClienteAsync(Cliente cliente)
         {
            await _client.From<Cliente>().Insert(cliente);
        }

        public async Task ActualizarClienteAsync(Cliente cliente)
        {
            await _client.From<Cliente>().Update(cliente);
        }

        public async Task EliminarClienteAsync(int id)
        {
            await _client.From<Cliente>().Where(c => c.id == id).Delete();
        }
    }
}
