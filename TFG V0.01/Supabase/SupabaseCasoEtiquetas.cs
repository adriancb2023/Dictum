using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;

namespace TFG_V0._01.Supabase
{
    public class SupabaseCasoEtiquetas
    {
        private readonly Client _client = new(Credenciales.SupabaseUrl, Credenciales.AnonKey);

        public Task InicializarAsync() => _client.InitializeAsync();

        public async Task<List<CasoEtiqueta>> ObtenerTodosAsync()
        {
            var result = await _client.From<CasoEtiqueta>().Limit(50000).Get().ConfigureAwait(false);
            return result.Models;
        }

        public Task InsertarAsync(CasoEtiqueta entidad) =>
            _client.From<CasoEtiqueta>().Insert(entidad);

        public Task ActualizarAsync(CasoEtiqueta entidad) =>
            _client.From<CasoEtiqueta>().Update(entidad);

        public Task EliminarAsync(int id) =>
            _client.From<CasoEtiqueta>().Where(x => x.id == id).Delete();
    }
}
