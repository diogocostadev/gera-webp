using GeraWebP.Hub;
using GeraWebP.Worker;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<LimparArquivos>();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

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

app.UseStaticFiles();
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
