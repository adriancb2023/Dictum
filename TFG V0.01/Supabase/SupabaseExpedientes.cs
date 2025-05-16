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
    public class SupabaseExpedientes
    {
        private readonly Client _client;

        public SupabaseExpedientes()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public async Task InicializarAsync()
        {
            await _client.InitializeAsync();
        }

        public async Task<List<Expediente>> ObtenerTodosAsync()
        {
            var result = await _client.From<Expediente>().Limit(50000).Get();
            return result.Models;
        }

        public async Task InsertarAsync(Expediente entidad)
        {
            await _client.From<Expediente>().Insert(entidad);
        }

        public async Task ActualizarAsync(Expediente entidad)
        {
            await _client.From<Expediente>().Update(entidad);
        }

        public async Task EliminarAsync(int id)
        {
            await _client.From<Expediente>().Where(x => x.id == id).Delete();
        }
    }
}
