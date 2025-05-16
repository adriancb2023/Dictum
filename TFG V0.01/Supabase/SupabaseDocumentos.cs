using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase;
using TFG_V0._01.Supabase.Models;
using Client = Supabase.Client;
using System.Linq;

namespace TFG_V0._01.Supabase
{
    public class SupabaseDocumentos
    {
        private readonly Client _client;

        public SupabaseDocumentos()
        {
            _client = new Client(Credenciales.SupabaseUrl, Credenciales.AnonKey);
        }

        public async Task InicializarAsync()
        {
            await _client.InitializeAsync();
        }

        public async Task<List<Documento>> ObtenerTodosAsync()
        {
            var result = await _client.From<Documento>().Limit(50000).Get();
            return result.Models;
        }

        public async Task InsertarAsync(Documento entidad)
        {
            await _client.From<Documento>().Insert(entidad);
        }

        public async Task ActualizarAsync(Documento entidad)
        {
            await _client.From<Documento>().Update(entidad);
        }

        public async Task EliminarAsync(int id)
        {
            await _client.From<Documento>().Where(x => x.id == id).Delete();
        }

        public async Task<List<Documento>> ObtenerPorClienteAsync(int clienteId)
        {
            // Obtener todos los casos del cliente
            var supabaseCasos = new SupabaseCasos();
            await supabaseCasos.InicializarAsync();
            var casos = await supabaseCasos.ObtenerTodosAsync();
            var casosCliente = casos.Where(c => c.id_cliente == clienteId).Select(c => c.id).ToList();

            if (!casosCliente.Any())
                return new List<Documento>();

            // Obtener todos los documentos cuyo id_caso esté en la lista de casos del cliente
            var result = await _client.From<Documento>().Limit(50000).Get();
            return result.Models.Where(d => casosCliente.Contains(d.id_caso)).ToList();
        }

        public async Task<List<Documento>> ObtenerPorCasoAsync(int casoId)
        {
            var result = await _client.From<Documento>().Limit(50000).Get();
            return result.Models.Where(d => d.id_caso == casoId).ToList();
        }
    }
}
