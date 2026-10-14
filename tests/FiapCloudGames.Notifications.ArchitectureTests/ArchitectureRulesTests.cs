using System.Reflection;
using System.Xml.Linq;
using FiapCloudGames.Notifications.Domain.Notifications;
using FiapCloudGames.Notifications.Presentation.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiapCloudGames.Notifications.ArchitectureTests;

public sealed class ArchitectureRulesTests
{
    [Fact]
    public void ProjectReferences_FollowCleanArchitectureDirection()
    {
        AssertProjectReferences("Domain");
        AssertProjectReferences("Application", "Domain");
        AssertProjectReferences("Infrastructure", "Application", "Domain");
        AssertProjectReferences("Presentation", "Application");
        AssertProjectReferences("Api", "Infrastructure", "Presentation");
        AssertProjectReferences("Migrations", "Infrastructure");
        AssertProjectReferences("Worker", "Infrastructure");
    }

    [Fact]
    public void Domain_DoesNotReferenceOuterLayersOrFrameworks()
    {
        AssertDoesNotReference(
            typeof(Notification).Assembly,
            "Application",
            "Infrastructure",
            "Presentation",
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public void Application_DoesNotReferenceOuterLayersOrHttpAndDataFrameworks()
    {
        AssertDoesNotReference(
            typeof(FiapCloudGames.Notifications.Application.DependencyInjection).Assembly,
            "Infrastructure",
            "Presentation",
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public void Controllers_LiveOnlyInPresentation()
    {
        var presentationControllers = typeof(NotificationsPolicies).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && typeof(ControllerBase).IsAssignableFrom(type))
            .ToArray();
        var apiControllers = typeof(Program).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && typeof(ControllerBase).IsAssignableFrom(type))
            .ToArray();

        Assert.NotEmpty(presentationControllers);
        Assert.Empty(apiControllers);
    }

    [Fact]
    public void EfMigrations_LiveOnlyInMigrationsExecutable()
    {
        var root = FindRepositoryRoot();
        var infrastructureMigrations = Path.Combine(
            root,
            "src",
            "FiapCloudGames.Notifications.Infrastructure",
            "Persistence",
            "Migrations");
        var migrationsDirectory = Path.Combine(
            root,
            "src",
            "FiapCloudGames.Notifications.Migrations",
            "Migrations");
        var migrationsProjectFile = Path.Combine(
            root,
            "src",
            "FiapCloudGames.Notifications.Migrations",
            "FiapCloudGames.Notifications.Migrations.csproj");
        var migrationsAppSettings = Path.Combine(
            root,
            "src",
            "FiapCloudGames.Notifications.Migrations",
            "appsettings.json");

        Assert.False(
            Directory.Exists(infrastructureMigrations) &&
            Directory.GetFiles(
                infrastructureMigrations,
                "*.cs",
                SearchOption.AllDirectories).Length > 0);
        Assert.True(File.Exists(migrationsAppSettings));

        var migrationsProject = XDocument.Load(migrationsProjectFile);
        Assert.Equal(
            "Exe",
            migrationsProject.Descendants("OutputType")
                .Single()
                .Value);
        Assert.Equal(
            "PreserveNewest",
            migrationsProject
                .Descendants("None")
                .Single(element =>
                    element.Attribute("Update")?.Value == "appsettings*.json")
                .Element("CopyToOutputDirectory")
                ?.Value);
    }

    private static void AssertDoesNotReference(Assembly assembly, params string[] forbiddenParts)
    {
        var references = assembly.GetReferencedAssemblies().Select(reference => reference.Name ?? string.Empty);
        var violations = references
            .Where(reference => forbiddenParts.Any(
                part => reference.Contains(part, StringComparison.Ordinal)))
            .ToArray();

        Assert.Empty(violations);
    }

    private static void AssertProjectReferences(string layer, params string[] expectedLayers)
    {
        var root = FindRepositoryRoot();
        var projectPath = Path.Combine(
            root,
            "src",
            $"FiapCloudGames.Notifications.{layer}",
            $"FiapCloudGames.Notifications.{layer}.csproj");
        var references = XDocument.Load(projectPath)
            .Descendants("ProjectReference")
            .Select(element => Path.GetFileNameWithoutExtension((string?)element.Attribute("Include")))
            .Select(name => name?.Split('.').Last())
            .Where(name => name is not null)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expectedLayers.Order(StringComparer.Ordinal), references);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null &&
               !File.Exists(Path.Combine(directory.FullName, "FiapCloudGames.Notifications.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("A raiz do repositório não foi encontrada.");
    }
}
