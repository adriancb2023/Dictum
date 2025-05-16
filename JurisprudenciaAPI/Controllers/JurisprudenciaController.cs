using Microsoft.AspNetCore.Mvc;
using JurisprudenciaApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using HtmlAgilityPack;
using System.Linq;
using System;
using System.Net.Http.Headers;
using System.Web;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace JurisprudenciaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JurisprudenciaController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<JurisprudenciaController> _logger;
        private readonly CookieContainer _cookieContainer = new CookieContainer();

        // Mapeo de Órganos Judiciales
        private static readonly Dictionary<string, string> TipoOrganoMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Tribunal Supremo (Agrupador y opciones individuales)
            { "Tribunal Supremo", "11|12|13|14|15|16" },
            { "Tribunal Supremo. Sala de lo Civil", "11" },
            { "Tribunal Supremo. Sala de lo Penal", "12" },
            { "Tribunal Supremo. Sala de lo Contencioso", "13" },
            { "Tribunal Supremo. Sala de lo Social", "14" },
            { "Tribunal Supremo. Sala de lo Militar", "15" },
            { "Tribunal Supremo. Sala de lo Especial", "16" },
            // Audiencia Nacional (Agrupador y opciones individuales)
            { "Audiencia Nacional", "22|2264|23|24|25|26|27|28|29" },
            { "Audiencia Nacional. Sala de lo Penal", "22" },
            { "Sala de Apelación de la Audiencia Nacional", "2264" },
            { "Audiencia Nacional. Sala de lo Contencioso", "23" },
            { "Audiencia Nacional. Sala de lo Social", "24" },
            { "Audiencia Nacional. Juzgados Centrales de Instrucción", "27" },
            { "Audiencia Nacional. Juzgado Central de Menores", "26" },
            { "Audiencia Nacional. Juzgado Central de Vigilancia Penitenciaria", "25" },
            { "Audiencia Nacional. Juzgados Centrales de lo Contencioso", "29" },
            { "Audiencia Nacional. Juzgados Centrales de lo Penal", "28" },
            // Tribunal Superior de Justicia (Agrupador y opciones individuales)
            { "Tribunal Superior de Justicia", "31|31201202|33|34" },
            { "Tribunal Superior de Justicia. Sala de lo Civil y Penal", "31" },
            { "Sección de Apelación Penal. TSJ Sala de lo Civil y Penal", "31201202" },
            { "Tribunal Superior de Justicia. Sala de lo Contencioso", "33" },
            { "Tribunal Superior de Justicia. Sala de lo Social", "34" },
            // Otros órganos
            { "Audiencia Provincial", "37" },
            { "Audiencia Provincial. Tribunal Jurado", "38" },
            { "Tribunal de Marca de la UE", "1001" },
            { "Juzgado de Primera Instancia", "42" },
            { "Juzgado de Instrucción", "43" },
            { "Juzgado de lo Contencioso Administrativo", "45" },
            { "Juzgado de Menores", "53" },
            { "Juzgado de Primera Instancia e Instrucción", "41" },
            { "Juzgado de lo Mercantil", "47" },
            { "Juzgados de Marca de la UE", "1002" },
            { "Juzgado de lo Penal", "51" },
            { "Juzgado de lo Social", "44" },
            { "Juzgado de Vigilancia Penitenciaria", "52" },
            { "Juzgado de Violencia sobre la Mujer", "48" },
            { "Tribunal Militar Territorial", "83" },
            { "Tribunal Militar Central", "85" },
            { "Consejo Supremo de Justicia Militar", "75" },
            { "Audiencia Territorial", "36" }
        };

        // Mapa de idiomas
        private static readonly Dictionary<string, string> IdiomaMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Todos", "" },
            { "Español", "1" }, // Guessed code
            { "Català", "2" }, // Guessed code
            { "Galego", "3" }, // Guessed code
            { "Euskera", "4" }  // Guessed code
        };

        // Política de reintentos
        private static readonly AsyncRetryPolicy<HttpResponseMessage> RetryPolicy =
            Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));



        public static readonly Dictionary<string, string> SubTipoResolucionOptions = new Dictionary<string, string>
        {
            // Autos
            { "Auto aclaratorio", "AUTO ACLARATORIO" },
            { "Auto de admisión (recurso de casación L.O. 7/2015)", "AUTO ADMISION" },
            { "Auto de inadmisión (recurso de casación L.O. 7/2015)", "AUTO INADMISION" },
            { "Otros Autos", "AUTO OTROS" },
            // Sentencias
            { "Sentencia de casación (L.O. 7/2015)", "SENTENCIA CASACION" },
            { "Otras Sentencias", "SENTENCIA OTRAS" },
            // Acuerdo
            { "Acuerdo", "ACUERDO" }
            // Nota: Los valores "AUTO RECURSO" no son finales, sino agrupadores en el UI.
        };

        // Opciones para el tipo principal (que se enviarían en el parámetro TIPORESOLUCION)
        // Podrías necesitar enviar esto si la API lo requiere explícitamente además del SUBTIPORESOLUCION.
        public static readonly Dictionary<string, string> TipoResolucionPrincipalOptions = new Dictionary<string, string>
        {
            { "Auto", "AUTO" },
            { "Sentencia", "SENTENCIA" }
            // "Acuerdo" también podría ser un valor para TIPORESOLUCION si se selecciona "Acuerdo" arriba.
        };

        public static readonly Dictionary<string, string> SeccionAutoOptions = new Dictionary<string, string>
        {
            { "Todas", "" },
            { "Segunda", "2" },
            { "Tercera", "3" },
            { "Cuarta", "4" },
            { "Quinta", "1" }
        };

        public static readonly Dictionary<string, string> ComunidadOptions = new Dictionary<string, string>
        {
            { "ANDALUCÍA", "ALL@ALL@ANDALUCÍA" },
            { "ARAGÓN", "ALL@ALL@ARAGÓN" },
            { "ASTURIAS", "ALL@ALL@ASTURIAS" },
            { "BALEARES", "ALL@ALL@BALEARES" },
            { "CANARIAS", "ALL@ALL@CANARIAS" },
            { "CANTABRIA", "ALL@ALL@CANTABRIA" },
            { "CASTILLA LA MANCHA", "ALL@ALL@CASTILLA_LA_MANCHA" },
            { "CASTILLA Y LEÓN", "ALL@ALL@CASTILLA_Y_LEÓN" },
            { "CATALUÑA", "ALL@ALL@CATALUÑA" },
            { "CEUTA", "ALL@ALL@CEUTA" },
            { "COMUNIDAD VALENCIANA", "ALL@ALL@COMUNIDAD_VALENCIANA" },
            { "EXTREMADURA", "ALL@ALL@EXTREMADURA" },
            { "GALICIA", "ALL@ALL@GALICIA" },
            { "LA RIOJA", "ALL@ALL@LA_RIOJA" },
            { "MADRID", "ALL@ALL@MADRID" },
            { "MELILLA", "ALL@ALL@MELILLA" },
            { "MURCIA", "ALL@ALL@MURCIA" },
            { "NAVARRA", "ALL@ALL@NAVARRA" },
            { "PAÍS VASCO", "ALL@ALL@PAÍS_VASCO" }
        };

        public JurisprudenciaController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger<JurisprudenciaController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _logger = logger;
        }

        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<JurisprudenciaResult>>> Search([FromBody] JurisprudenciaSearchParameters parameters)
        {
            if (parameters == null)
                return BadRequest("Parámetros de búsqueda no pueden ser nulos");

            if (parameters.FechaDesde.HasValue && parameters.FechaHasta.HasValue &&
                parameters.FechaDesde > parameters.FechaHasta)
            {
                return BadRequest("FechaDesde no puede ser mayor que FechaHasta");
            }

            var cacheKey = $"search_{System.Text.Json.JsonSerializer.Serialize(parameters)}";
            if (_cache.TryGetValue(cacheKey, out List<JurisprudenciaResult> cachedResults))
            {
                _logger.LogInformation("Retornando resultados desde caché");
                return Ok(cachedResults);
            }

            try
            {
                // Usa el mismo nombre de cliente que configuraste en Program.cs
                var client = _httpClientFactory.CreateClient("PoderJudicial"); 

                // PASO 1: Realizar una petición GET inicial para obtener cookies (JSESSIONID)
                // Usaremos la URL del Referer que vimos en los logs del navegador, o la página principal de búsqueda.
                // Es importante que esta petición use el mismo 'client' para que las cookies se almacenen en su CookieContainer.
                var initialGetUrl = "https://www.poderjudicial.es/search/indexAN.jsp"; // O la URL del Referer
                _logger.LogInformation("Realizando petición GET inicial a {Url} para obtener cookies.", initialGetUrl);
                
                try
                {
                    // No necesitamos configurar todos los encabezados para esta petición GET inicial,
                    // el User-Agent que podría estar configurado por defecto en el HttpClient puede ser suficiente,
                    // o puedes añadir los mínimos necesarios si da problemas.
                    var initialRequest = new HttpRequestMessage(HttpMethod.Get, initialGetUrl);
                    // Podrías añadir algunos encabezados si es necesario, pero a menudo no lo es para la obtención de cookies.
                    // ConfigureRequestHeaders(initialRequest); // Probablemente no necesites todos los encabezados aquí.
                    initialRequest.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36");
                    initialRequest.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                    initialRequest.Headers.Add("Accept-Language", "en-US,en;q=0.8"); // O es-ES si prefieres
                    initialRequest.Headers.Add("Sec-Fetch-Dest", "document");
                    initialRequest.Headers.Add("Sec-Fetch-Mode", "navigate");
                    initialRequest.Headers.Add("Sec-Fetch-Site", "none"); // O "same-origin" si vienes de otra página del mismo sitio
                    initialRequest.Headers.Add("Sec-Fetch-User", "?1");
                    initialRequest.Headers.Add("Upgrade-Insecure-Requests", "1");


                    var initialResponse = await client.SendAsync(initialRequest);
                    initialResponse.EnsureSuccessStatusCode(); // Asegura que la petición GET fue exitosa
                    _logger.LogInformation("Petición GET inicial completada. Cookies deberían estar establecidas.");
                    // No necesitas leer el contenido de initialResponse, solo nos importan las cookies que el servidor estableció.
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Error en la petición GET inicial para obtener cookies. Status: {StatusCode}", ex.StatusCode);
                    // Decide si quieres continuar o retornar un error aquí.
                    // Por ahora, podríamos continuar e intentar el POST, pero es probable que falle.
                    // return StatusCode((int?)ex.StatusCode ?? 503, "Error al establecer sesión inicial con el servicio del Poder Judicial.");
                }


                // PASO 2: Proceder con la petición POST de búsqueda (como la tenías)
                var requestUrl = "https://www.poderjudicial.es/search/search.action";

                var response = await RetryPolicy.ExecuteAsync(async () =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                    ConfigureRequestHeaders(request); // Aquí sí aplicas todos tus encabezados para el POST

                    var formData = BuildFormData(parameters);
                    request.Content = new FormUrlEncodedContent(formData);
                    // request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded"); // FormUrlEncodedContent lo hace por ti

                    _logger.LogDebug("Enviando petición POST al Poder Judicial...");
                    var res = await client.SendAsync(request); // 'client' ahora debería tener las cookies

                    if (!res.IsSuccessStatusCode)
                    {
                        var errorContent = await res.Content.ReadAsStringAsync();
                        _logger.LogWarning("Error en la respuesta POST: {StatusCode} - {Content}",
                            res.StatusCode, errorContent.Length > 500 ? errorContent.Substring(0, 500) : errorContent);
                    }

                    // Podrías querer lanzar la excepción solo para ciertos códigos de error aquí si el RetryPolicy no lo hace
                    // res.EnsureSuccessStatusCode(); 
                    return res;
                });

                // NUEVO: Verifica si después de los reintentos, la respuesta final fue un 403 u otro error persistente
                if (!response.IsSuccessStatusCode)
                {
                     var errorContentFinal = await response.Content.ReadAsStringAsync();
                    _logger.LogError("La petición POST falló después de los reintentos con Status: {StatusCode}. Contenido: {Content}", 
                        response.StatusCode, errorContentFinal.Length > 500 ? errorContentFinal.Substring(0, 500) : errorContentFinal);
                    // Lanza una excepción o devuelve un error apropiado
                    // Esto asegura que no intentes parsear una respuesta de error.
                    response.EnsureSuccessStatusCode(); 
                }


                string responseBody = await response.Content.ReadAsStringAsync();
                // _logger.LogDebug("Respuesta POST recibida. Longitud: {Length} chars", responseBody.Length); // Puedes mantener esto

                // NUEVO: Loguea una porción mayor del responseBody, o todo si no es demasiado grande para un test
                _logger.LogInformation("Respuesta HTML completa recibida (o una porción grande): {HtmlBody}", responseBody.Length > 4000 ? responseBody.Substring(0, 4000) : responseBody); // Loguea los primeros 4000 caracteres

                var parsedResults = ParseHtmlResponse(responseBody);
                _logger.LogInformation("Parseados {Count} resultados", parsedResults.Count);

                if (!parsedResults.Any() && responseBody.Contains("Por favor, especifique algún criterio para su búsqueda"))
                {
                    _logger.LogWarning("La búsqueda no devolvió resultados y el servidor indicó que faltan criterios.");
                    // Podrías querer devolver un BadRequest o un Ok con una lista vacía y un mensaje.
                }


                _cache.Set(cacheKey, parsedResults, TimeSpan.FromMinutes(30));
                return Ok(parsedResults);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Error de HttpRequestException al consultar el Poder Judicial. Status: {StatusCode}", e.StatusCode);
                return StatusCode((int?)e.StatusCode ?? 500, $"Error al conectar con el servicio del Poder Judicial: {e.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al procesar la búsqueda");
                return StatusCode(500, $"Error interno al procesar la solicitud: {ex.Message}");
            }
        }

        private void ConfigureRequestHeaders(HttpRequestMessage request)
        {
            request.Headers.Clear();
            request.Headers.Add("Accept", "text/html, */*; q=0.01");
            request.Headers.Add("Accept-Language", "en-US,en;q=0.8");
            request.Headers.Add("Origin", "https://www.poderjudicial.es");
            request.Headers.Add("Referer", "https://www.poderjudicial.es/search/openDocument/14b7e2ad6029040e");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Sec-Fetch-Dest", "empty");
            request.Headers.Add("Sec-Fetch-Mode", "cors");
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36");
            request.Headers.Add("Sec-GPC", "1");
            request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"136\", \"Brave\";v=\"136\", \"Not.A/Brand\";v=\"99\"");
            request.Headers.Add("sec-ch-ua-mobile", "?0");
            request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
        }

        private Dictionary<string, string> BuildFormData(JurisprudenciaSearchParameters parameters)
        {
            // Calcular el 'start' record basado en la página actual y registros por página
            int startRecord = ((parameters.PaginaActual - 1) * parameters.RegistrosPorPagina) + 1;

            var formData = new Dictionary<string, string>
            {
                { "action", "query" },
                { "sort", "IN_FECHARESOLUCION:decreasing" },
                { "recordsPerPage", parameters.RegistrosPorPagina.ToString() }, // Usar el parámetro
                { "databasematch", "AN" }, // O el que corresponda
                { "start", startRecord.ToString() } // Usar el 'start' calculado
            };

            // Mapeo de parámetros (asegúrate que esta parte esté restaurada y funcionando)
            if (!string.IsNullOrEmpty(parameters.TextoLibre))
                formData["TEXT"] = parameters.TextoLibre;

            if (!string.IsNullOrEmpty(parameters.Jurisdiccion))
                formData["JURISDICCION"] = $"|{parameters.Jurisdiccion.ToUpper()}|";

            if (parameters.TiposResolucion?.Any() == true) // Asumiendo que envías el nombre o código correcto
                formData["TIPORESOLUCION"] = string.Join("||", parameters.TiposResolucion.Select(tr => $"|{tr.ToUpper()}|"));


            if (parameters.OrganosJudiciales?.Any() == true)
            {
                var mappedOrganoValues = new List<string>();
                foreach (var orgName in parameters.OrganosJudiciales) // Asumimos que OrganosJudiciales contiene NOMBRES
                {
                    if (TipoOrganoMap.TryGetValue(orgName, out var code))
                    {
                        mappedOrganoValues.Add(code); // code puede ser "11|12|13"
                    }
                    else
                    {
                        _logger.LogWarning($"Nombre de órgano judicial '{orgName}' no encontrado en TipoOrganoMap. Será ignorado.");
                    }
                }
                if (mappedOrganoValues.Any())
                {
                    string combinedCodes = string.Join("|", mappedOrganoValues.SelectMany(c => c.Split('|', StringSplitOptions.RemoveEmptyEntries)).Distinct());
                    formData["TIPOORGANOPUB"] = $"|{combinedCodes}|";
                }
            }
            // ... (resto de tus AddIfNotNull para ROJ, ECLI, Fechas, etc.) ...
            AddIfNotNull(formData, "ROJ", parameters.Roj);
            AddIfNotNull(formData, "ECLI", parameters.Ecli);
            AddIfNotNull(formData, "NUMERO_RESOLUCION", parameters.NumeroResolucion);
            AddIfNotNull(formData, "NUMERO_RECURSO", parameters.NumeroRecurso);
            AddIfNotNull(formData, "PONENTE", parameters.Ponente);
            AddIfNotNull(formData, "SECCION", parameters.Seccion);
            AddIfNotNull(formData, "NORMA_CITADA", parameters.Legislacion);

            if (parameters.FechaDesde.HasValue)
                formData["FECHARESOLUCIONDESDE"] = parameters.FechaDesde.Value.ToString("dd/MM/yyyy");

            if (parameters.FechaHasta.HasValue)
                formData["FECHARESOLUCIONHASTA"] = parameters.FechaHasta.Value.ToString("dd/MM/yyyy");

            if (!string.IsNullOrEmpty(parameters.Idioma) && IdiomaMap.TryGetValue(parameters.Idioma, out var idiomaCode))
                AddIfNotNull(formData, "IDIOMA", idiomaCode);

            if (parameters.Localizaciones?.Any() == true) // Asumiendo que envías el nombre o código correcto
                formData["AMBITOTERRITORIAL"] = string.Join("||", parameters.Localizaciones.Select(l => $"|{l}|"));


            _logger.LogInformation("API FormData construido para Pagina: {Pagina}, Registros: {Registros}, Start: {StartRecord} -> {@FormData}",
                parameters.PaginaActual, parameters.RegistrosPorPagina, startRecord, formData);
            return formData;
        }

        private List<JurisprudenciaResult> ParseHtmlResponse(string html)
        {
            var results = new List<JurisprudenciaResult>();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            // Selector principal actualizado
            var resultNodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='jurisprudenciaresults_searchresults']/div[contains(@class, 'searchresult') and contains(@class, 'doc')]");

            if (resultNodes == null || !resultNodes.Any())
            {
                _logger.LogWarning("No se encontraron nodos de resultados con el selector principal. HTML recibido (inicio): {HtmlStart}",
                    html.Length > 1000 ? html.Substring(0, 1000) + "..." : html); // Loguea más si es necesario
                return results;
            }

            _logger.LogInformation("Encontrados {Count} nodos de resultado con el selector principal.", resultNodes.Count);

            foreach (var node in resultNodes)
            {
                try
                {
                    var titleNode = node.SelectSingleNode(".//div[contains(@class, 'title')]/a[@data-roj]");
                    var metadatosNode = node.SelectSingleNode(".//div[contains(@class, 'metadatos')]/ul");

                    string rojCompleto = titleNode?.GetAttributeValue("data-roj", "")?.Trim() ?? "";
                    string textoTitulo = titleNode?.InnerText?.Trim() ?? "";

                    string tipoResolucion = "";
                    if (rojCompleto.StartsWith("ATS") || textoTitulo.StartsWith("ATS"))
                    {
                        tipoResolucion = "Auto";
                    }
                    else if (rojCompleto.StartsWith("STS") || textoTitulo.StartsWith("STS"))
                    {
                        tipoResolucion = "Sentencia";
                    }
                    // Añadir más lógicas si hay otros tipos como Providencia (ej. "PTS")

                    string ecliText = metadatosNode?.SelectSingleNode("./li[b[starts-with(text(), 'ECLI:')]]/b")?.InnerText?.Trim() ?? "";
                    
                    string organoJudicialText = metadatosNode?.SelectSingleNode("./li[2]/b")?.InnerText?.Trim() ?? "";
                    // Fallback o alternativa si la posición 2 no siempre es el órgano:
                    if (string.IsNullOrWhiteSpace(organoJudicialText) || organoJudicialText.Contains("ECLI:") || organoJudicialText.Contains("Municipio:") || organoJudicialText.Contains("Ponente:") || organoJudicialText.Contains("Nº Recurso:"))
                    {
                         // Intentar buscar un li que contenga "Sala" o "Juzgado" y que no sea otro campo conocido
                        var organoLiNode = metadatosNode?.SelectNodes("./li/b")
                                     ?.FirstOrDefault(bNode => 
                                         (bNode.InnerText.Contains("Sala") || bNode.InnerText.Contains("Juzgado") || bNode.InnerText.Contains("Tribunal")) &&
                                         !bNode.InnerText.Contains("ECLI:") && 
                                         !bNode.InnerText.Contains("Municipio:") &&
                                         !bNode.InnerText.Contains("Ponente:") &&
                                         !bNode.InnerText.Contains("Nº Recurso:"));
                        organoJudicialText = organoLiNode?.InnerText?.Trim() ?? "Órgano no extraído";
                    }


                    string fechaResAttr = titleNode?.GetAttributeValue("data-fechares", "")?.Trim() ?? "";
                    string fechaFormateada = "";
                    if (DateTime.TryParseExact(fechaResAttr, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                    {
                        fechaFormateada = parsedDate.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        // Intentar extraer del texto del título si data-fechares falla o no está
                        // Ejemplo: "ATS, a 30 de abril de 2025 - ROJ: ATS 4223/2025"
                        var match = System.Text.RegularExpressions.Regex.Match(textoTitulo, @"a (\d{1,2} de \w+ de \d{4})");
                        if (match.Success)
                        {
                            fechaFormateada = match.Groups[1].Value;
                        } else {
                            fechaFormateada = "Fecha no extraída";
                        }
                    }


                    var result = new JurisprudenciaResult
                    {
                        Roj = rojCompleto,
                        Ecli = CleanText(ecliText, "ECLI:"),
                        FechaResolucion = fechaFormateada,
                        TipoResolucion = tipoResolucion,
                        OrganoJudicial = CleanText(organoJudicialText),
                        Ponente = CleanText(metadatosNode?.SelectSingleNode("./li[starts-with(normalize-space(.), 'Ponente:')]/b")?.InnerText?.Trim() ?? ""),
                        Resumen = CleanText(node.SelectSingleNode(".//div[contains(@class, 'summary')]")?.InnerText?.Trim() ?? "", "RESUMEN:"),
                        UrlDocumento = GetAbsoluteUrl(titleNode?.GetAttributeValue("href", ""))
                    };

                    if (!string.IsNullOrWhiteSpace(result.Roj) || !string.IsNullOrWhiteSpace(result.Ecli))
                    {
                        results.Add(result);
                    }
                    else
                    {
                        _logger.LogWarning("Resultado parseado sin ROJ ni ECLI, descartado. HTML del nodo: {NodeHtml}", node.OuterHtml.Length > 500 ? node.OuterHtml.Substring(0,500) : node.OuterHtml);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al parsear un resultado individual. HTML del nodo: {NodeHtml}", node.OuterHtml.Length > 500 ? node.OuterHtml.Substring(0,500) : node.OuterHtml);
                }
            }
            _logger.LogInformation("Parseados {Count} resultados válidos de {TotalNodes} nodos encontrados.", results.Count, resultNodes.Count);
            return results;
        }

        // Modifica CleanText para que acepte un prefijo a quitar
        private string CleanText(string? text, string? prefixToRemove = null)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var cleanedText = text.Trim();
            if (!string.IsNullOrEmpty(prefixToRemove) && cleanedText.StartsWith(prefixToRemove, StringComparison.OrdinalIgnoreCase))
            {
                cleanedText = cleanedText.Substring(prefixToRemove.Length).Trim();
            }
            
            // Quita los prefijos genéricos que ya tenías, si es necesario después del específico
            cleanedText = cleanedText.Replace("Roj:", "", StringComparison.OrdinalIgnoreCase)
                                     .Replace("ECLI:", "", StringComparison.OrdinalIgnoreCase) // ECLI específico ya se manejó
                                     .Replace("Fecha:", "", StringComparison.OrdinalIgnoreCase)
                                     .Trim();
            return cleanedText;
        }

        [HttpGet("initialData")]
        public ActionResult<InitialDataResponse> GetInitialData()
        {
            return Ok(new InitialDataResponse
            {
                Jurisdicciones = new List<string> { "Civil", "Penal", "Contencioso", "Social" ,"Militar", "Especial" },
                TiposResolucion = new List<string> { "Sentencia", "Auto", "Providencia" },
                OrganosJudiciales = TipoOrganoMap.Keys.OrderBy(x => x).ToList(),
                Localizaciones = new List<string> { "Madrid", "Barcelona", "Sevilla", "Valencia", "Bilbao" }
            });
        }
        private void AddIfNotNull(Dictionary<string, string> dict, string key, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                dict[key] = value;
            }
        }
        private string GetAbsoluteUrl(string? relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl) || relativeUrl.StartsWith("http"))
                return relativeUrl ?? string.Empty;

            try
            {
                return new Uri(new Uri("https://www.poderjudicial.es"), relativeUrl).AbsoluteUri;
            }
            catch
            {
                return relativeUrl ?? string.Empty;
            }
        }
    }

    public class InitialDataResponse
    {
        public List<string> Jurisdicciones { get; set; } = new List<string>();
        public List<string> TiposResolucion { get; set; } = new List<string>();
        public List<string> OrganosJudiciales { get; set; } = new List<string>();
        public List<string> Localizaciones { get; set; } = new List<string>();
    }

    public class JurisprudenciaApiResponse
    {
        public List<JurisprudenciaResult> Resultados { get; set; }
        public int TotalResultados { get; set; } // Total de resultados encontrados por CENDOJ
        public int PaginaActual { get; set; }
        public int RegistrosPorPagina { get; set; }
        public int TotalPaginas { get; set; }
    }
}