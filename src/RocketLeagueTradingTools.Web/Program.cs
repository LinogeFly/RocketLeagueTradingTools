using RocketLeagueTradingTools.Web;

var app = WebApplication
    .CreateBuilder(args)
    .Configure()
    .Build();

app.ApplyEntityFrameworkMigrations();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();