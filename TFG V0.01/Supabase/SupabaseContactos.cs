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
    public class SupabaseContactos
    {
        private readonly Client _client;

        public SupabaseContactos()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public async Task InicializarAsync()
        {
            await _client.InitializeAsync();
        }

        public async Task<List<Contacto>> ObtenerTodosAsync()
        {
            var result = await _client.From<Contacto>().Limit(50000).Get();
            return result.Models;
        }

        public async Task InsertarAsync(Contacto entidad)
        {
            await _client.From<Contacto>().Insert(entidad);
        }

        public async Task ActualizarAsync(Contacto entidad)
        {
            await _client.From<Contacto>().Update(entidad);
        }

        public async Task EliminarAsync(int id)
        {
            await _client.From<Contacto>().Where(x => x.id == id).Delete();
        }
    }
}
