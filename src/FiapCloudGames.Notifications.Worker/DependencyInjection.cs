using FiapCloudGames.BuildingBlocks.Exceptions;
using FiapCloudGames.Notifications.Application.Abstractions.Delivery;
using FiapCloudGames.Notifications.Application.Abstractions.Persistence;
using FiapCloudGames.Notifications.Application.Features.Process;
using FiapCloudGames.Notifications.Domain.Notifications;
using FiapCloudGames.Notifications.Infrastructure.Delivery;
using FiapCloudGames.Notifications.Infrastructure.Health;
using FiapCloudGames.Notifications.Infrastructure.Persistence;
using FiapCloudGames.Notifications.Infrastructure.Persistence.Repositories;
using FiapCloudGames.Notifications.Worker.Consumers;
using FiapCloudGames.Notifications.Worker.Health;
using FiapCloudGames.Notifications.Worker.Messaging;
using FiapCloudGames.Notifications.Worker.Templates;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FiapCloudGames.Notifications.Worker;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsWorker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddRabbitMqOptions(services, configuration);

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddSingleton<INotificationSender, LogNotificationSender>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<NotificationTemplateRenderer>();
        services.AddScoped<ProcessNotificationService>();

        services.AddHealthChecks()
            .AddCheck<NotificationsDatabaseHealthCheck>(
                "notifications-database",
                tags: [WorkerHealthExtensions.ReadyTag]);

        AddMessaging(services);
        return services;
    }

    private static void AddPersistence(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "A connection string 'Database' não foi configurada.");

        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<INotificationsUnitOfWork>(provider =>
            provider.GetRequiredService<NotificationsDbContext>());
    }

    private static void AddRabbitMqOptions(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Host),
                "RabbitMq:Host é obrigatório.")
            .Validate(options => options.Port > 0,
                "RabbitMq:Port deve ser maior que zero.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.VirtualHost),
                "RabbitMq:VirtualHost é obrigatório.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Username),
                "RabbitMq:Username é obrigatório.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Password),
                "RabbitMq:Password é obrigatório.")
            .Validate(options => HasConfiguredTopology(options.Topology),
                "Todos os nomes em RabbitMq:Topology são obrigatórios.")
            .ValidateOnStart();
    }

    private static void AddMessaging(IServiceCollection services)
    {
        services.AddMassTransit(configurator =>
        {
            configurator.AddConsumer<UserCreatedConsumer>();
            configurator.AddConsumer<PaymentApprovedConsumer>();
            configurator.AddConsumer<PaymentRejectedConsumer>();
            configurator.AddConsumer<OrderCompletedConsumer>();
            configurator.AddConsumer<OrderFailedConsumer>();

            configurator.UsingRabbitMq((context, rabbitMq) =>
            {
                var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                RabbitMqTopologyConfigurator.Configure(
                    rabbitMq,
                    options.Topology.Exchanges);

                rabbitMq.Host(
                    options.Host,
                    options.Port,
                    options.VirtualHost,
                    host =>
                    {
                        host.Username(options.Username);
                        host.Password(options.Password);
                    });

                ConfigureEndpoint<UserCreatedConsumer>(
                    rabbitMq, context, options.Topology.Queues.UserCreated);
                ConfigureEndpoint<PaymentApprovedConsumer>(
                    rabbitMq, context, options.Topology.Queues.PaymentApproved);
                ConfigureEndpoint<PaymentRejectedConsumer>(
                    rabbitMq, context, options.Topology.Queues.PaymentRejected);
                ConfigureEndpoint<OrderCompletedConsumer>(
                    rabbitMq, context, options.Topology.Queues.OrderCompleted);
                ConfigureEndpoint<OrderFailedConsumer>(
                    rabbitMq, context, options.Topology.Queues.OrderFailed);
            });
        });
    }

    private static void ConfigureEndpoint<TConsumer>(
        IRabbitMqBusFactoryConfigurator rabbitMq,
        IBusRegistrationContext context,
        string queueName)
        where TConsumer : class, IConsumer
    {
        rabbitMq.ReceiveEndpoint(queueName, endpoint =>
        {
            endpoint.ConcurrentMessageLimit = 1;
            endpoint.UseMessageRetry(retry =>
            {
                retry.Ignore<AppException>();
                retry.Intervals(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(15));
            });
            endpoint.ConfigureConsumer<TConsumer>(context);
        });
    }

    private static bool HasConfiguredTopology(RabbitMqTopologyOptions topology)
    {
        var exchanges = topology.Exchanges;
        var queues = topology.Queues;

        return !string.IsNullOrWhiteSpace(exchanges.UserCreated) &&
               !string.IsNullOrWhiteSpace(exchanges.PaymentApproved) &&
               !string.IsNullOrWhiteSpace(exchanges.PaymentRejected) &&
               !string.IsNullOrWhiteSpace(exchanges.OrderCompleted) &&
               !string.IsNullOrWhiteSpace(exchanges.OrderFailed) &&
               !string.IsNullOrWhiteSpace(queues.UserCreated) &&
               !string.IsNullOrWhiteSpace(queues.PaymentApproved) &&
               !string.IsNullOrWhiteSpace(queues.PaymentRejected) &&
               !string.IsNullOrWhiteSpace(queues.OrderCompleted) &&
               !string.IsNullOrWhiteSpace(queues.OrderFailed);
    }
}
