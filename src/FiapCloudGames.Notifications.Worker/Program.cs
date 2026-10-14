using FiapCloudGames.Notifications.Application.Abstractions.Delivery;
using FiapCloudGames.Notifications.Application.Abstractions.Persistence;
using FiapCloudGames.Notifications.Application.Features.Process;
using FiapCloudGames.Notifications.Domain.Notifications;
using FiapCloudGames.Notifications.Infrastructure.Delivery;
using FiapCloudGames.Notifications.Infrastructure.Persistence;
using FiapCloudGames.Notifications.Infrastructure.Persistence.Repositories;
using FiapCloudGames.Notifications.Worker.Consumers;
using FiapCloudGames.Notifications.Worker.Events;
using FiapCloudGames.Notifications.Worker.Templates;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException(
        "A connection string 'Database' não foi configurada.");

builder.Services.AddDbContext<NotificationsDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<INotificationsUnitOfWork>(provider =>
    provider.GetRequiredService<NotificationsDbContext>());
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddSingleton<INotificationSender, LogNotificationSender>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<NotificationTemplateRenderer>();
builder.Services.AddScoped<ProcessNotificationService>();

var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
var rabbitUser = builder.Configuration["RabbitMq:Username"] ?? "guest";
var rabbitPass = builder.Configuration["RabbitMq:Password"] ?? "guest";

builder.Services.AddMassTransit(bus =>
{
    bus.AddConsumer<UserCreatedConsumer>();
    bus.AddConsumer<PaymentApprovedConsumer>();
    bus.AddConsumer<PaymentRejectedConsumer>();
    bus.AddConsumer<OrderCompletedConsumer>();
    bus.AddConsumer<OrderFailedConsumer>();

    bus.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        cfg.ReceiveEndpoint("notifications.user-created", e =>
        {
            e.ConfigureConsumer<UserCreatedConsumer>(context);
            e.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15)));
        });

        cfg.ReceiveEndpoint("notifications.payment-approved", e =>
        {
            e.ConfigureConsumer<PaymentApprovedConsumer>(context);
            e.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15)));
        });

        cfg.ReceiveEndpoint("notifications.payment-rejected", e =>
        {
            e.ConfigureConsumer<PaymentRejectedConsumer>(context);
            e.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15)));
        });

        cfg.ReceiveEndpoint("notifications.order-completed", e =>
        {
            e.ConfigureConsumer<OrderCompletedConsumer>(context);
            e.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15)));
        });

        cfg.ReceiveEndpoint("notifications.order-failed", e =>
        {
            e.ConfigureConsumer<OrderFailedConsumer>(context);
            e.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15)));
        });
    });
});

var host = builder.Build();
await host.RunAsync();
