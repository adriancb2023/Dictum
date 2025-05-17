using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;

namespace TFG_V0._01.Supabase
{
    public class SupabaseExpedientes
    {
        private readonly Client _client;

        public SupabaseExpedientes()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public Task InicializarAsync() => _client.InitializeAsync();

        public async Task<List<Expediente>> ObtenerTodosAsync()
        {
            var result = await _client.From<Expediente>().Limit(50000).Get();
            return result.Models;
        }

        public Task InsertarAsync(Expediente entidad) =>
            _client.From<Expediente>().Insert(entidad);

        public Task ActualizarAsync(Expediente entidad) =>
            _client.From<Expediente>().Update(entidad);

        public Task EliminarAsync(int id) =>
            _client.From<Expediente>().Where(x => x.id == id).Delete();
    }
}
