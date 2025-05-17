using System.Collections.Generic;
using System.Threading.Tasks;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;

namespace TFG_V0._01.Supabase
{
    public class SupabaseTareas
    {
        private readonly Client _client;

        public SupabaseTareas()
            => _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);

        public Task InicializarAsync()
            => _client.InitializeAsync();

        public async Task<List<Tarea>> ObtenerTodosAsync()
            => (await _client.From<Tarea>().Limit(50000).Get()).Models;

        public Task ActualizarAsync(Tarea tarea)
            => _client.From<Tarea>().Update(tarea);
    }
}
