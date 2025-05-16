using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Define CORS policy name
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Configuración radical de culturas (versión corregida)
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("es-ES");
    options.FallBackToParentCultures = false;
    options.FallBackToParentUICultures = false;
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new CustomRequestCultureProvider(async context =>  // Añadido 'async'
        await Task.FromResult(new ProviderCultureResult("es-ES"))));  // Añadido 'await'
});

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configuración básica de servicios API
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("PoderJudicial", client =>
{
    // Puedes configurar encabezados por defecto aquí si algunos son siempre iguales
    // client.BaseAddress = new Uri("https://www.poderjudicial.es/"); // Opcional
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = true, // Generalmente bueno tenerlo
    UseCookies = true,        // CRUCIAL
    CookieContainer = new CookieContainer() // CRUCIAL para almacenar y enviar cookies como JSESSIONID
});

var app = builder.Build();

// Middleware simplificado
app.Use(async (context, next) =>
{
    context.Request.Headers.Remove("Accept-Language");
    await next();
});

app.UseRequestLocalization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Endpoint solo en desarrollo
    app.MapGet("/culture-debug", (HttpContext context) =>
    {
        return new
        {
            Culture = CultureInfo.CurrentCulture.Name,
            Headers = context.Request.Headers
                .Where(h => h.Key == "Accept-Language")
                .ToDictionary(h => h.Key, h => h.Value.ToString())
        };
    });
}

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

app.Run();