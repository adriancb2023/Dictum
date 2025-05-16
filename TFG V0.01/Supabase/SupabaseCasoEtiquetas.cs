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
    public class SupabaseCasoEtiquetas
    {
        private readonly Client _client;

        public SupabaseCasoEtiquetas()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public async Task InicializarAsync()
        {
            await _client.InitializeAsync();
        }

        public async Task<List<CasoEtiqueta>> ObtenerTodosAsync()
        {
            var result = await _client.From<CasoEtiqueta>().Limit(50000).Get();
            return result.Models;
        }

        public async Task InsertarAsync(CasoEtiqueta entidad)
        {
            await _client.From<CasoEtiqueta>().Insert(entidad);
        }

        public async Task ActualizarAsync(CasoEtiqueta entidad)
        {
            await _client.From<CasoEtiqueta>().Update(entidad);
        }

        public async Task EliminarAsync(int id)
        {
            await _client.From<CasoEtiqueta>().Where(x => x.id == id).Delete();
        }
    }
}
