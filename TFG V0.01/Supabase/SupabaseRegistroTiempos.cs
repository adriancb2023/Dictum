using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;

namespace TFG_V0._01.Supabase
{
    public class SupabaseRegistroTiempos
    {
        private readonly Client _client;

        public SupabaseRegistroTiempos()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public Task InicializarAsync() => _client.InitializeAsync();

        public async Task<List<RegistroTiempo>> ObtenerTodosAsync()
        {
            var result = await _client.From<RegistroTiempo>().Limit(50000).Get();
            return result.Models;
        }

        public Task InsertarAsync(RegistroTiempo entidad) =>
            _client.From<RegistroTiempo>().Insert(entidad);

        public Task ActualizarAsync(RegistroTiempo entidad) =>
            _client.From<RegistroTiempo>().Update(entidad);

        public Task EliminarAsync(int id) =>
            _client.From<RegistroTiempo>().Where(x => x.id == id).Delete();
    }
}
