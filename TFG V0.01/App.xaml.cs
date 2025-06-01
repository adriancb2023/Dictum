using System.Configuration;
using System.Data;
using System.Windows;
using Supabase.Storage;
using Client = Supabase.Client;
using SupabaseOptions = Supabase.SupabaseOptions;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;

namespace TFG_V0._01
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Client supabaseClient;
        private static IConfiguration configuration;

        public static Client Supabase => supabaseClient;
        public static IConfiguration Configuration => configuration;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Get the directory where the application is running
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            
            // Build configuration
            configuration = new ConfigurationBuilder()
                .SetBasePath(baseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false
            };

            // Get Supabase configuration from appsettings.json
            var supabaseUrl = configuration["Supabase:Url"];
            var supabaseKey = configuration["Supabase:AnonKey"];

            supabaseClient = new Client(supabaseUrl, supabaseKey, options);
            await supabaseClient.InitializeAsync();
        }
    }
}
