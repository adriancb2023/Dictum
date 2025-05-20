using System.Configuration;
using System.Data;
using System.Windows;
using Supabase.Storage;
using Client = Supabase.Client;
using SupabaseOptions = Supabase.SupabaseOptions;

namespace TFG_V0._01
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Client supabaseClient;

        public static Client Supabase => supabaseClient;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false
            };

            supabaseClient = new Client("https://ddwyrkqxpmwlznjfjrwv.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImRkd3lya3F4cG13bHpuamZqcnd2Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQzOTcwNjQsImV4cCI6MjA1OTk3MzA2NH0.G2LzHWbC09LC69bj9wONzhD_a6AfFI1ZYFuQ3KD7XhI", options);
            await supabaseClient.InitializeAsync();
        }
    }
}
