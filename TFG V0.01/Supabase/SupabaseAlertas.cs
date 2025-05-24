using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;

namespace TFG_V0._01.Supabase
{
    public class SupabaseAlertas
    {
        private readonly Client _client;
        private Task _initTask;
        private bool _initialized;

        public SupabaseAlertas()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
            _initialized = false;
        }

        private async Task EnsureInitializedAsync()
        {
            if (!_initialized)
            {
                if (_initTask == null)
                    _initTask = _client.InitializeAsync();
                await _initTask;
                _initialized = true;
            }
        }

        public async Task<List<Alerta>> ObtenerTodosAsync()
        {
            await EnsureInitializedAsync();
            var result = await _client.From<Alerta>().Limit(50000).Get();
            return result.Models;
        }

        public async Task InsertarAsync(Alerta entidad)
        {
            await EnsureInitializedAsync();
            await _client.From<Alerta>().Insert(entidad);
        }

        public async Task ActualizarAsync(Alerta entidad)
        {
            await EnsureInitializedAsync();
            await _client.From<Alerta>().Update(entidad);
        }

        public async Task EliminarAsync(int id)
        {
            await EnsureInitializedAsync();
            await _client.From<Alerta>().Where(x => x.id == id).Delete();
        }
    }
}
