using LaLiga.WebUI.Services;
using LaLiga.WebUI.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<LeagueSettings>(builder.Configuration.GetSection("League"));

// API adresi appsettings.json > ApiSettings:BaseUrl (sonunda "/" olmalı)
var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>()
    ?? throw new InvalidOperationException("ApiSettings section is missing in appsettings.json.");

builder.Services.AddHttpClient<LaLigaApiClient>(client =>
{
    client.BaseAddress = new Uri(apiSettings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
