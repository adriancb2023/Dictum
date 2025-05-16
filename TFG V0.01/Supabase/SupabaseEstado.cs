using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;

namespace TFG_V0._01.Supabase
{
    public class SupabaseEstado
    {
        private readonly Client _client;

        public SupabaseEstado()
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

        public async Task InsertarAsync(Estado entidad)
        {
            await _client.From<Estado>().Insert(entidad);
        }

        public async Task ActualizarAsync(Estado entidad)
        {
            await _client.From<Estado>().Update(entidad);
        }

        public async Task EliminarAsync(int id)
        {
            await _client.From<Estado>().Where(x => x.id == id).Delete();
        }
    }
}
