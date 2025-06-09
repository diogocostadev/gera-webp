using GeraWebP.Hub;
using GeraWebP.Worker;
using System.Globalization;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Configurar Kestrel para .NET 9 - uploads de 100MB
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 104857600; // 100MB
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    serverOptions.Limits.MaxConcurrentConnections = 100;
    serverOptions.Limits.MaxConcurrentUpgradedConnections = 100;
});

// Configurar FormOptions para uploads grandes
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
    options.ValueLengthLimit = 104857600;
    options.MultipartHeadersLengthLimit = 104857600;
    options.MemoryBufferThreshold = 104857600;
    options.BufferBody = true;
    options.BufferBodyLengthLimit = 104857600;
});

builder.Services.AddHostedService<LimparArquivos>();

// Adicionar Response Compression para melhor performance
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "text/json", "image/svg+xml", "application/javascript" });
});

// Configurar compressão Brotli e Gzip
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

// Adicionar cache em memória
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

builder.Services.AddControllersWithViews();

// Configurar SignalR com limites maiores
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 104857600; // 100MB
    options.StreamBufferCapacity = 10;
    options.MaximumParallelInvocationsPerClient = 1;
    options.EnableDetailedErrors = true;
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configuração de localização
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("pt-BR"),
        new CultureInfo("en-US"),
        new CultureInfo("es-ES")
    };

    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("pt-BR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Configurar páginas de erro customizadas
app.UseStatusCodePagesWithReExecute("/Error/{0}");

// Middleware de performance (ordem importa!)
app.UseResponseCompression();
app.UseResponseCaching();

// Configurar Static Files com cache headers otimizados
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int durationInSeconds = 60 * 60 * 24 * 30; // 30 dias
        ctx.Context.Response.Headers.Append("Cache-Control", $"public,max-age={durationInSeconds}");
        ctx.Context.Response.Headers.Append("Expires", DateTime.UtcNow.AddDays(30).ToString("R"));
        
        // Headers de segurança
        ctx.Context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        
        // Compressão adicional para assets específicos
        if (ctx.File.Name.EndsWith(".css") || ctx.File.Name.EndsWith(".js"))
        {
            ctx.Context.Response.Headers.Append("Vary", "Accept-Encoding");
        }
    }
});

app.UseCors();
app.UseWebSockets(); // Importante para SignalR
app.UseRouting();
app.UseRequestLocalization();
app.UseAuthorization();

app.MapHub<ProgressHub>("/progressHub");

// Rota padrão (português)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapear controladores com atributos de rota
app.MapControllers();

app.Run();
