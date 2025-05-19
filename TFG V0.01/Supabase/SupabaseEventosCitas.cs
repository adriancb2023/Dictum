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
    public class SupabaseEventosCitas
    {
        private readonly Client _client;
        private bool _inicializado = false;
        private readonly SemaphoreSlim _initSemaphore = new(1, 1);

        public SupabaseEventosCitas()
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

        public async Task<List<EventoCita>> ObtenerEventosCitas()
        {
            await InicializarAsync().ConfigureAwait(false);
            var response = await _client.From<EventoCita>().Get().ConfigureAwait(false);
            return response.Models;
        }

        public async Task<EventoCita> ObtenerEventoCita(int id)
        {
            await InicializarAsync().ConfigureAwait(false);
            var response = await _client.From<EventoCita>().Where(x => x.Id == id).Single().ConfigureAwait(false);
            return response;
        }

        public async Task<List<EventoCita>> ObtenerEventosCitasPorCaso(int idCaso)
        {
            await InicializarAsync().ConfigureAwait(false);
            var response = await _client.From<EventoCita>().Where(x => x.IdCaso == idCaso).Get().ConfigureAwait(false);
            return response.Models;
        }

        public async Task InsertarEventoCita(EventoCita evento)
        {
            await InicializarAsync().ConfigureAwait(false);
            await _client.From<EventoCita>().Insert(evento).ConfigureAwait(false);
        }

        public async Task ActualizarEventoCita(EventoCita evento)
        {
            await InicializarAsync().ConfigureAwait(false);
            await _client.From<EventoCita>().Update(evento).ConfigureAwait(false);
        }

        public async Task EliminarEventoCita(int id)
        {
            await InicializarAsync().ConfigureAwait(false);
            await _client.From<EventoCita>().Where(x => x.Id == id).Delete().ConfigureAwait(false);
        }
    }
} 