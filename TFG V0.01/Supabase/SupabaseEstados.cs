using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;

namespace TFG_V0._01.Supabase
{
    public class SupabaseEstados
    {
        private readonly Client _client;

        public SupabaseEstados()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public async Task InicializarAsync()
        {
            await _client.InitializeAsync();
        }

        public async Task<List<Estado>> ObtenerTodosAsync()
        {
            var result = await _client.From<Estado>().Limit(50000).Get();
            return result.Models;
        }

        public async Task<Estado> ObtenerPorIdAsync(int id)
        {
            var result = await _client.From<Estado>().Where(x => x.id == id).Single();
            return result;
        }

        public async Task InsertarAsync(Estado estado)
        {
            await _client.From<Estado>().Insert(estado);
        }

        public async Task ActualizarAsync(Estado estado)
        {
            await _client.From<Estado>().Update(estado);
        }

        public async Task EliminarAsync(int id)
        {
            await _client.From<Estado>().Where(x => x.id == id).Delete();
        }
    }
} 