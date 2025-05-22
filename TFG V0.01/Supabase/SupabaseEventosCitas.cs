using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace TFG_V0._01.Supabase
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

        public async Task<List<EventoCita>> ObtenerTodosEventosManualAsync()
        {
            var config = ConfigHelper.GetConfiguration();
            var url = config["Supabase:Url"] + "/rest/v1/eventos_citas";
            var apiKey = config["Supabase:AnonKey"];

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("apikey", apiKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Range-Unit", "items");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Range", "0-49999");

            var response = await client.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<List<EventoCita>>(json);
        }
    }
} 