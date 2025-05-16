using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;

namespace TFG_V0._01.Supabase
{
    public class SupabaseHistorialEstados
    {
        private readonly Client _client;

        public SupabaseHistorialEstados()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public async Task InicializarAsync()
        {
            await _client.InitializeAsync();
        }

        public async Task<List<HistorialEstado>> ObtenerTodosAsync()
        {
            var result = await _client.From<HistorialEstado>().Limit(50000).Get();
            return result.Models;
        }

        public async Task InsertarAsync(HistorialEstado entidad)
        {
            await _client.From<HistorialEstado>().Insert(entidad);
        }

        public async Task ActualizarAsync(HistorialEstado entidad)
        {
            await _client.From<HistorialEstado>().Update(entidad);
        }

        public async Task EliminarAsync(int id)
        {
            await _client.From<HistorialEstado>().Where(x => x.id == id).Delete();
        }
    }
}
