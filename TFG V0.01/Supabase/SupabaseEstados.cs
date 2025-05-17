using System.Collections.Generic;
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

        public Task InicializarAsync() => _client.InitializeAsync();

        public async Task<List<Estado>> ObtenerTodosAsync()
        {
            var result = await _client.From<Estado>().Get();
            return result.Models;
        }

        public Task<Estado> ObtenerPorIdAsync(int id) =>
            _client.From<Estado>().Where(x => x.id == id).Single();

        public Task InsertarAsync(Estado estado) =>
            _client.From<Estado>().Insert(estado);

        public Task ActualizarAsync(Estado estado) =>
            _client.From<Estado>().Update(estado);

        public Task EliminarAsync(int id) =>
            _client.From<Estado>().Where(x => x.id == id).Delete();
    }
}
