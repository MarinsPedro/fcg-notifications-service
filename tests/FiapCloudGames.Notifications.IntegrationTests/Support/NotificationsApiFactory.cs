using FiapCloudGames.Notifications.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace FiapCloudGames.Notifications.IntegrationTests.Support;

public sealed class NotificationsApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"notifications-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        // O appsettings.json dos serviços ficou vazio (a configuração vem do orquestrador).
        // O Program lê estes valores ao montar o host, antes de ConfigureAppConfiguration.
        builder.UseSetting(
            "ConnectionStrings:Database",
            "Host=localhost;Database=integration;Username=test;Password=test");
        builder.UseSetting("Authentication:Authority", "https://localhost");
        builder.UseSetting("Authentication:Audience", "notifications-api");
        builder.UseSetting("Authentication:Issuer", "integration-issuer");
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] =
                    "Host=localhost;Database=integration;Username=test;Password=test",
                ["Authentication:Authority"] = "https://localhost",
                ["Authentication:Audience"] = "notifications-api",
                ["Authentication:Issuer"] = "integration-issuer",
                ["Logging:LogLevel:Default"] = "Warning"
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<NotificationsDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<NotificationsDbContext>>();
            services.AddDbContext<NotificationsDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            });
        });
    }
}

internal sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "integration-test"),
            new Claim("scope", "notifications.read"),
            new Claim("scope", "notifications.retry"),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
