using FiapCloudGames.BuildingBlocks.Hosting;
using FiapCloudGames.BuildingBlocks.Web;
using FiapCloudGames.Notifications.Api.Configuration;
using FiapCloudGames.Notifications.Infrastructure;
using FiapCloudGames.Notifications.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.AddFiapServiceDefaults();
builder.Services.AddFiapApiDefaults();
builder.Services.AddNotificationsInfrastructure(builder.Configuration);
builder.Services.AddNotificationsPresentation();
builder.Services.AddNotificationsOpenApi();

var app = builder.Build();

app.UseFiapApiDefaults();
app.UseNotificationsOpenApi();
app.UseAuthentication();
app.UseAuthorization();

app.MapFiapHealthEndpoints();
app.MapControllers();

await app.RunAsync();

public partial class Program;
