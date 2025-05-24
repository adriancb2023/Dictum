using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;

namespace TFG_V0._01.Supabase
{
    public class SupabaseEtiquetas
    {
        private readonly Client _client;

        public SupabaseEtiquetas()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public Task InicializarAsync() => _client.InitializeAsync();

        public async Task<List<Etiqueta>> ObtenerTodosAsync()
        {
            var result = await _client.From<Etiqueta>().Limit(50000).Get();
            return result.Models;
        }

        public Task InsertarAsync(Etiqueta entidad) =>
            _client.From<Etiqueta>().Insert(entidad);

        public Task ActualizarAsync(Etiqueta entidad) =>
            _client.From<Etiqueta>().Update(entidad);

        public Task EliminarAsync(int id) =>
            _client.From<Etiqueta>().Where(x => x.id == id).Delete();
    }
}
