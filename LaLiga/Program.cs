using System.Text.Json.Serialization;
using LaLiga.Context;
using LaLiga.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<LaLigaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<StandingsService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(options =>
    {
        // [Required], [Range] gibi otomatik doğrulama hataları da { message, errors } biçiminde dönsün
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                .GroupBy(e => FieldName(e.Key))
                .ToDictionary(
                    g => g.Key,
                    g => g.SelectMany(e => e.Value!.Errors)
                          .Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage) ? "Geçersiz değer." : x.ErrorMessage)
                          .ToArray());

            return new BadRequestObjectResult(new
            {
                message = "Gönderilen bilgilerde hata var. Lütfen alanları kontrol edin.",
                errors
            });
        };
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Beklenmeyen hatalar da { message } biçiminde dönsün (ayrıntı günlüğe yazılır).
// Not: geliştirme ortamında Visual Studio'nun ayrıntılı hata sayfası bunun önüne geçer.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new
        {
            message = "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin."
        });
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// "Code" ya da "$.name" biçimindeki alan anahtarını "code" / "name" (camelCase) yapar
static string FieldName(string key)
{
    var name = key.StartsWith("$.") ? key[2..] : key;
    return string.IsNullOrEmpty(name) ? "input" : char.ToLowerInvariant(name[0]) + name[1..];
}
