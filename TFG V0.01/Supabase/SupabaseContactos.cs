using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;
using System;
using Supabase.Postgrest;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Responses;

namespace TFG_V0._01.Supabase
{
    public class SupabaseContactos
    {
        private readonly Client _client;

        public SupabaseContactos()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public Task InicializarAsync() => _client.InitializeAsync();

        public async Task<List<Contacto>> ObtenerTodosAsync()
        {
            var result = await _client.From<Contacto>().Limit(50000).Get();
            return result.Models;
        }

        public async Task InsertarAsync(ContactoInsertDto dto)
        {
            try
            {
                var response = await _client.From<ContactoInsertDto>()
                    .Insert(new[] { dto });
                // Si no hay excepción, consideramos éxito aunque no haya modelos devueltos
            }
            catch (Exception ex)
            {
                throw new System.Exception($"Excepción al insertar contacto: {ex.Message}", ex);
            }
        }

        public Task<ModeledResponse<Contacto>> ActualizarAsync(Contacto entidad) =>
            _client.From<Contacto>().Update(entidad);

        public Task EliminarAsync(int id) =>
            _client.From<Contacto>().Where(x => x.id == id).Delete();
    }
}
