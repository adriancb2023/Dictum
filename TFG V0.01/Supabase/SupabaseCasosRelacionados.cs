using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;

namespace TFG_V0._01.Supabase
{
    public class SupabaseCasosRelacionados
    {
        private readonly Client _client;

        public SupabaseCasosRelacionados()
        {
            _client = new(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public Task InicializarAsync() => _client.InitializeAsync();

        public async Task<List<CasoRelacionado>> ObtenerTodosAsync()
        {
            var result = await _client.From<CasoRelacionado>().Limit(50000).Get();
            return result.Models;
        }

        public Task InsertarAsync(CasoRelacionado entidad) =>
            _client.From<CasoRelacionado>().Insert(entidad);

        public Task ActualizarAsync(CasoRelacionado entidad) =>
            _client.From<CasoRelacionado>().Update(entidad);

        public Task EliminarAsync(int id) =>
            _client.From<CasoRelacionado>().Where(x => x.id == id).Delete();
    }
}
