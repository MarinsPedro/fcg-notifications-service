using FiapCloudGames.BuildingBlocks.Hosting;
using FiapCloudGames.Notifications.Worker;
using FiapCloudGames.Notifications.Worker.Health;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.AddFiapServiceDefaults();
builder.Services.AddNotificationsWorker(builder.Configuration);

var app = builder.Build();

app.MapWorkerHealthEndpoints();

await app.RunAsync();
