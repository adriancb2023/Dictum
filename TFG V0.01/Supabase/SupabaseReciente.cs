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
    public class SupabaseReciente
    {
        private readonly Client _client;

        public SupabaseReciente()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public async Task InicializarAsync()
        {
            await _client.InitializeAsync();
        }

        public async Task<List<Reciente>> ObtenerTodosAsync()
        {
            var result = await _client.From<Reciente>().Limit(50000).Get();
            return result.Models;
        }

        public async Task InsertarAsync(Reciente entidad)
        {
            await _client.From<Reciente>().Insert(entidad);
        }

        public async Task ActualizarAsync(Reciente entidad)
        {
            await _client.From<Reciente>().Update(entidad);
        }

        public async Task EliminarAsync(int id)
        {
            await _client.From<Reciente>().Where(x => x.id == id).Delete();
        }
    }
}
