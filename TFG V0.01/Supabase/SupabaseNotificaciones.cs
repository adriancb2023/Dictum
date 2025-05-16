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
    public class SupabaseNotificaciones
    {
        private readonly Client _client;

        public SupabaseNotificaciones()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public async Task InicializarAsync()
        {
            await _client.InitializeAsync();
        }

        public async Task<List<Notificacion>> ObtenerTodosAsync()
        {
            var result = await _client.From<Notificacion>().Limit(50000).Get();
            return result.Models;
        }

        public async Task InsertarAsync(Notificacion entidad)
        {
            await _client.From<Notificacion>().Insert(entidad);
        }

        public async Task ActualizarAsync(Notificacion entidad)
        {
            await _client.From<Notificacion>().Update(entidad);
        }

        public async Task EliminarAsync(int id)
        {
            await _client.From<Notificacion>().Where(x => x.id == id).Delete();
        }
    }
}
