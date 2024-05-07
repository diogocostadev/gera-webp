using GeraWebP.Hub;
using GeraWebP.Worker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<LimparArquivos>();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

//TODO: não está funcionando
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ProgressHub>("/progressHub");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
