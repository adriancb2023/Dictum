using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;


namespace TFG_V0._01.Supabase
{
    public class SupabaseTareas
    {
        private readonly Client _client;

        public SupabaseTareas()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public async Task InicializarAsync()
        {
            await _client.InitializeAsync();
        }

        public async Task<List<Tarea>> ObtenerTodosAsync()
        {
            var result = await _client.From<Tarea>().Limit(50000).Get();
            return result.Models;
        }

        public async Task ActualizarAsync(Tarea tarea)
        {
            await _client.From<Tarea>().Update(tarea);
        }
        
        
        

    }
}
