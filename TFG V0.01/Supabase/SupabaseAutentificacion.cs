using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TFG_V0._01.Supabase
{
    internal class SupabaseAutentificacion
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _supabaseUrl;
        private readonly string _anonKey;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public SupabaseAutentificacion()
        {
            _supabaseUrl = Credenciales.SupabaseUrl;
            _anonKey = Credenciales.AnonKey;

            if (!_httpClient.DefaultRequestHeaders.Contains("apikey"))
                _httpClient.DefaultRequestHeaders.Add("apikey", _anonKey);
        }

        public async Task<AuthResponse> SignUpAsync(string email, string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            ArgumentException.ThrowIfNullOrWhiteSpace(password);

            var url = $"{_supabaseUrl}/auth/v1/signup";
            var content = new StringContent(
                JsonSerializer.Serialize(new { email, password }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error en el registro: {responseContent}");

            return JsonSerializer.Deserialize<AuthResponse>(responseContent, _jsonOptions);
        }

        public async Task<AuthResponse> SignInAsync(string email, string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            ArgumentException.ThrowIfNullOrWhiteSpace(password);

            var url = $"{_supabaseUrl}/auth/v1/token?grant_type=password";
            var content = new StringContent(
                JsonSerializer.Serialize(new { email, password }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error en el inicio de sesión: {responseContent}");

            return JsonSerializer.Deserialize<AuthResponse>(responseContent, _jsonOptions);
        }

        public async Task SignOutAsync(string accessToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

            var url = $"{_supabaseUrl}/auth/v1/logout";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Error al cerrar sesión");
        }

        public async Task ResetPasswordAsync(string email)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email);

            var url = $"{_supabaseUrl}/auth/v1/recover";
            var content = new StringContent(
                JsonSerializer.Serialize(new { email }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Error al solicitar el restablecimiento de contraseña");
        }

        public async Task<User> GetUserAsync(string accessToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

            var url = $"{_supabaseUrl}/auth/v1/user";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al obtener el usuario: {responseContent}");

            return JsonSerializer.Deserialize<User>(responseContent, _jsonOptions);
        }
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public User User { get; set; }
    }

    public class User
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
