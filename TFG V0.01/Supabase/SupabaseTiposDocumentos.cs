using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Supabase;
using TFG.Models;
using Client = Supabase.Client;
using TFG_V0._01.Supabase;

namespace TFG.Supabase
{
    public class SupabaseTiposDocumentos
    {
        private readonly Client _client;
        private bool _inicializado = false;
        private readonly SemaphoreSlim _initSemaphore = new(1, 1);

        public SupabaseTiposDocumentos()
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

        public async Task<List<TipoDocumento>> ObtenerTiposDocumentos()
        {
            await InicializarAsync().ConfigureAwait(false);
            var response = await _client.From<TipoDocumento>().Get().ConfigureAwait(false);
            return response.Models;
        }

        public async Task<TipoDocumento> ObtenerTipoDocumento(int id)
        {
            await InicializarAsync().ConfigureAwait(false);
            var response = await _client.From<TipoDocumento>().Where(x => x.Id == id).Single().ConfigureAwait(false);
            return response;
        }
    }
} 