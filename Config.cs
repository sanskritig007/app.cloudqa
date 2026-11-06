using Microsoft.Extensions.Configuration;

namespace CloudQA.ResilientAutomation;

/// <summary>
/// Handles test configuration loading from appsettings.json,
/// with support for command-line argument overrides.
/// </summary>
public class TestSettings
{
    public string BaseUrl { get; set; } = "https://app.cloudqa.io/home/AutomationPracticeForm";
    public bool Headless { get; set; } = false;
    public int TimeoutSeconds { get; set; } = 10;
}

public static class ConfigReader
{
    public static TestSettings Load(string[]? args = null)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        var config = builder.Build();

        var settings = new TestSettings();
        config.GetSection("TestSettings").Bind(settings);

        // Allow passing a custom URL via CLI args (e.g. dotnet run -- https://custom-url.com)
        if (args != null && args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
        {
            settings.BaseUrl = args[0].Trim();
        }

        return settings;
    }
}
