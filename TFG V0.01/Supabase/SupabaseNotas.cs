using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;
using Supabase.Postgrest;

namespace TFG_V0._01.Supabase
{
    public class SupabaseNotas
    {
        private readonly Client _client;
        private bool _inicializado = false;
        private readonly SemaphoreSlim _initSemaphore = new(1, 1);

        public SupabaseNotas()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public async Task InicializarAsync()
        {
            if (_inicializado) return;
            await _initSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_inicializado) return;
                await _client.InitializeAsync().ConfigureAwait(false);
                _inicializado = true;
            }
            finally
            {
                _initSemaphore.Release();
            }
        }

        public async Task<List<Nota>> ObtenerNotas()
        {
            await InicializarAsync().ConfigureAwait(false);
            var response = await _client.From<Nota>().Get().ConfigureAwait(false);
            return response.Models;
        }

        public async Task<Nota> ObtenerNota(int id)
        {
            await InicializarAsync().ConfigureAwait(false);
            var response = await _client.From<Nota>().Where(x => x.Id == id).Single().ConfigureAwait(false);
            return response;
        }

        public async Task<List<Nota>> ObtenerNotasPorCaso(int idCaso)
        {
            await InicializarAsync().ConfigureAwait(false);
            var response = await _client.From<Nota>().Where(x => x.IdCaso == idCaso).Get().ConfigureAwait(false);
            return response.Models;
        }

        public async Task<Nota> InsertarAsync(Nota nota)
        {
            await InicializarAsync().ConfigureAwait(false);
            var response = await _client.From<Nota>().Insert(nota).ConfigureAwait(false);
            return response.Models.Count > 0 ? response.Models[0] : null;
        }

        public async Task ActualizarAsync(Nota nota)
        {
            await InicializarAsync().ConfigureAwait(false);
            await _client.From<Nota>()
                .Where(x => x.Id == nota.Id)
                .Set(x => x.Nombre, nota.Nombre)
                .Set(x => x.Descripcion, nota.Descripcion)
                .Update()
                .ConfigureAwait(false);
        }

        public async Task EliminarAsync(int id)
        {
            await InicializarAsync().ConfigureAwait(false);
            await _client.From<Nota>().Where(x => x.Id == id).Delete().ConfigureAwait(false);
        }
    }
} 