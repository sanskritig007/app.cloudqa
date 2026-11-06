using CloudQA.ResilientAutomation;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

Console.WriteLine("=== Milestone 1 Sanity Check ===");

// 1. Verify Config loading
var settings = ConfigReader.Load(args);
Console.WriteLine($"[Config] Target URL     : {settings.BaseUrl}");
Console.WriteLine($"[Config] Headless Mode  : {settings.Headless}");
Console.WriteLine($"[Config] Timeout Seconds: {settings.TimeoutSeconds}");

// 2. Quick Selenium Browser launch verification
Console.WriteLine("\n[Browser] Launching Chrome to verify Selenium WebDriver...");

var options = new ChromeOptions();
if (settings.Headless)
{
    options.AddArgument("--headless=new");
}
options.AddArgument("--start-maximized");

using IWebDriver driver = new ChromeDriver(options);

try
{
    Console.WriteLine($"[Browser] Navigating to: {settings.BaseUrl}");
    driver.Navigate().GoToUrl(settings.BaseUrl);

    Console.WriteLine($"[Success] Page loaded successfully!");
    Console.WriteLine("=== Milestone 1 Verification Complete! ===");

    // Keeps browser open so you can inspect it visually
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n--> Browser is open. Press ENTER in this terminal to close it...");
    Console.ResetColor();
    Console.ReadLine();
}
finally
{
    driver.Quit();
}
