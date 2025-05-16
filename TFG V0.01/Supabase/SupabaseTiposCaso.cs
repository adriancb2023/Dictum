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
    public class SupabaseTiposCaso
    {
        private readonly Client _client;

        public SupabaseTiposCaso()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public async Task InicializarAsync()
        {
            await _client.InitializeAsync();
        }

        public async Task<List<TipoCaso>> ObtenerTodosAsync()
        {
            var result = await _client.From<TipoCaso>().Limit(50000).Get();
            return result.Models;
        }

        public async Task<TipoCaso> ObtenerPorIdAsync(int id)
        {
            var result = await _client.From<TipoCaso>().Where(x => x.id == id).Single();
            return result;
        }
    }
}
