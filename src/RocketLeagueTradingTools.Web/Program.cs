using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Core.Application;
using RocketLeagueTradingTools.Core.Application.Notifications;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Infrastructure.Common;
using RocketLeagueTradingTools.Infrastructure.Persistence;
using RocketLeagueTradingTools.Web.Mapping;
using RocketLeagueTradingTools.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((context, config) =>
{
    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
});

// Add services to the container.

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        opt.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        );
    });

builder.Services.AddSQLiteDbContext();
builder.Services.AddSingleton(typeof(ILogger), typeof(Logger<Program>));
builder.Services.AddSingleton<ILog, Log>();
builder.Services.AddSingleton<IDateTime, SystemDateTime>();
builder.Services.AddSingleton<INotificationApplicationSettings, NotificationApplicationSettings>();
builder.Services.AddScoped<IPersistenceRepository, PersistenceRepository>();
builder.Services.AddScoped<AlertApplication>();
builder.Services.AddScoped<NotificationApplication>();
builder.Services.AddSingleton<DomainToDtoProfile.TradeOfferAgeResolver>();
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Apply Entity Framework migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();
    context.Database.Migrate();
}

app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
