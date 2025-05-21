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

        public SupabaseTareas(Client client)
            => _client = client;

        public Task InicializarAsync()
            => _client.InitializeAsync();

        public async Task<List<Tarea>> ObtenerTodosAsync()
            => (await _client.From<Tarea>().Limit(50000).Get()).Models;

        public async Task<List<Tarea>> ObtenerTareasDelCaso(int casoId)
            => (await _client.From<Tarea>().Where(x => x.id_caso == casoId).Get()).Models;

        public Task ActualizarTarea(Tarea tarea)
            => _client.From<Tarea>().Update(tarea);

        public Task ActualizarAsync(Tarea tarea)
            => _client.From<Tarea>().Update(tarea);

        public Task CrearTarea(Tarea tarea)
            => _client.From<Tarea>().Insert(tarea);

        public Task EliminarTarea(int id)
            => _client.From<Tarea>().Where(x => x.id == id).Delete();

        public Task ActualizarEstadoTarea(int id, bool completada)
            => _client.From<Tarea>().Where(x => x.id == id).Set(x => x.completada, completada).Update();
    }
}
