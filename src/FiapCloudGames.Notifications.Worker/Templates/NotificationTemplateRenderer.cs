using System.Reflection;
using System.Text.RegularExpressions;

namespace FiapCloudGames.Notifications.Worker.Templates;

public sealed partial class NotificationTemplateRenderer
{
    private static readonly Assembly Assembly = typeof(NotificationTemplateRenderer).Assembly;
    private static readonly string Namespace = typeof(NotificationTemplateRenderer).Namespace!;

    public string Render(string templateName, Dictionary<string, string> placeholders)
    {
        var html = LoadTemplate(templateName);

        foreach (var (key, value) in placeholders)
        {
            html = html.Replace($"{{{{{key}}}}}", value, StringComparison.Ordinal);
        }

        return html;
    }

    private static string LoadTemplate(string templateName)
    {
        var resourceName = $"{Namespace}.{templateName}.html";

        using var stream = Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Template '{templateName}' não encontrado como recurso embutido ({resourceName}).");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
