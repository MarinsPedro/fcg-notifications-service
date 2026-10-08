using Microsoft.OpenApi;

namespace FiapCloudGames.Notifications.Api.Configuration;

internal static class NotificationsOpenApiExtensions
{
    public static IServiceCollection AddNotificationsOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "Fiap Cloud Games Notifications API",
                    Version = "v1",
                    Description =
                        "Serviço de notificações por e-mail e outros canais."
                });

            options.AddSecurityDefinition(
                "bearer",
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Informe somente o JWT, sem o prefixo Bearer."
                });

            options.AddSecurityRequirement(document =>
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("bearer", document)] = []
                });
        });

        return services;
    }

    public static WebApplication UseNotificationsOpenApi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "Fiap Cloud Games Notifications API v1");
            options.EnablePersistAuthorization();
        });

        return app;
    }
}
