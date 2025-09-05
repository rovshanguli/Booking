using Api.HostedServices;
using Api.Repositories;
using Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddSingleton<IHomeRepository, InMemoryHomeRepository>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddHostedService<HomeSeederHostedService>();

builder.WebHost.ConfigureKestrel(o =>
{
    o.ListenLocalhost(5173);
    o.ListenLocalhost(7173, lo => lo.UseHttps());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Booking API v1");
        c.RoutePrefix = string.Empty;
    });
}

if (!app.Environment.IsEnvironment("Testing"))
    app.UseHttpsRedirection();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.Run();

public partial class Program
{ }