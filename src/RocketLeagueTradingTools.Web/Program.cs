using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Core.Application.Alert;
using RocketLeagueTradingTools.Core.Application.Blacklist;
using RocketLeagueTradingTools.Core.Application.Notification;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;
using RocketLeagueTradingTools.Infrastructure.Common;
using RocketLeagueTradingTools.Infrastructure.Persistence;
using RocketLeagueTradingTools.Infrastructure.Persistence.Repositories;
using RocketLeagueTradingTools.Web.Mapping;
using RocketLeagueTradingTools.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((context, config) =>
{
    var env = context.HostingEnvironment.EnvironmentName;

    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
    config.AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false);
});

// Add services to the container.

builder.Services
    .AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        opt.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        );
    });

builder.Services.AddSingletonSqliteDbContext(builder.Configuration);
builder.Services.AddMediatR(typeof(AlertUpdateEvent));
builder.Services.AddSingleton(typeof(ILogger), typeof(Logger<Program>));
builder.Services.AddSingleton<ILog, Log>();
builder.Services.AddSingleton<IDateTime, SystemDateTime>();
builder.Services.AddSingleton<INotificationSessionStorage, NotificationSessionStorage>();
builder.Services.AddScoped<INotificationPersistenceRepository, NotificationPersistenceRepository>();
builder.Services.AddScoped<ITradeOfferPersistenceRepository, TradeOfferPersistenceRepository>();
builder.Services.AddScoped<IAlertPersistenceRepository, AlertPersistenceRepository>();
builder.Services.AddScoped<IBlacklistPersistenceRepository, BlacklistPersistenceRepository>();

// Applications
builder.Services.AddScoped<AlertApplication>();
builder.Services.AddScoped<NotificationApplication>();
builder.Services.AddSingleton<INotificationApplicationSettings, NotificationApplicationSettings>();
builder.Services.AddScoped<BlacklistApplication>();

// Mapping
builder.Services.AddSingleton<DomainToDtoMapper>();

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
