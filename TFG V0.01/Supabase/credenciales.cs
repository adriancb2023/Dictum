using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace TFG_V0._01.Supabase
{
    internal static class Credenciales
    {
        private static readonly IConfiguration _configuration;

        static Credenciales()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        public static string SupabaseUrl => _configuration["Supabase:Url"];
        public static string AnonKey => _configuration["Supabase:AnonKey"];
        public static string ServiceRoleKey => _configuration["Supabase:ServiceRoleKey"];
    }
}
