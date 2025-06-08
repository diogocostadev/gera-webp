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

app.UseCors();
app.UseStaticFiles();
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
