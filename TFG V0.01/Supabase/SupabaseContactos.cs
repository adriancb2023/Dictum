using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;

namespace TFG_V0._01.Supabase
{
    public class SupabaseContactos
    {
        private readonly Client _client;

        public SupabaseContactos()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public Task InicializarAsync() => _client.InitializeAsync();

        public async Task<List<Contacto>> ObtenerTodosAsync()
        {
            var result = await _client.From<Contacto>().Limit(50000).Get();
            return result.Models;
        }

        public Task InsertarAsync(Contacto entidad) =>
            _client.From<Contacto>().Insert(entidad);

        public Task ActualizarAsync(Contacto entidad) =>
            _client.From<Contacto>().Update(entidad);

        public Task EliminarAsync(int id) =>
            _client.From<Contacto>().Where(x => x.id == id).Delete();
    }
}
