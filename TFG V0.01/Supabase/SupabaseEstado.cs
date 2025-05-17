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
            _client = new(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public Task InicializarAsync() => _client.InitializeAsync();

        public async Task<List<Estado>> ObtenerTodosAsync()
        {
            var result = await _client.From<Estado>().Get();
            return result.Models;
        }

        public Task InsertarAsync(Estado entidad) =>
            _client.From<Estado>().Insert(entidad);

        public Task ActualizarAsync(Estado entidad) =>
            _client.From<Estado>().Update(entidad);

        public Task EliminarAsync(int id) =>
            _client.From<Estado>().Where(x => x.id == id).Delete();
    }
}
