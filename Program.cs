using CloudQA.ResilientAutomation;
using CloudQA.ResilientAutomation.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

Console.WriteLine("==================================================");
Console.WriteLine("   CloudQA Resilient Form Automation Test Suite   ");
Console.WriteLine("==================================================");

// 1. Load configuration
var settings = ConfigReader.Load(args);
Console.WriteLine($"[Config] Target URL     : {settings.BaseUrl}");
Console.WriteLine($"[Config] Headless       : {settings.Headless}");
Console.WriteLine($"[Config] Timeout Seconds: {settings.TimeoutSeconds}");

// 2. Setup Chrome Driver
var options = new ChromeOptions();
if (settings.Headless)
{
    options.AddArgument("--headless=new");
}
options.AddArgument("--start-maximized");

using IWebDriver driver = new ChromeDriver(options);
driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

try
{
    Console.WriteLine($"\n[Navigation] Opening: {settings.BaseUrl}");
    driver.Navigate().GoToUrl(settings.BaseUrl);

    var practicePage = new AutomationPracticePage(driver);

    // 3. Automate the 3 fields
    string testFirstName = "Sanskriti";
    string testGender = "Female";
    string testState = "India";

    practicePage.SetFirstName(testFirstName);
    practicePage.SelectGender(testGender);
    practicePage.SelectState(testState);

    // 4. Verify & Assert results
    Console.WriteLine("\n--------------------------------------------------");
    Console.WriteLine("             Execution Verification               ");
    Console.WriteLine("--------------------------------------------------");

    string actualFirstName = practicePage.GetFirstName();
    bool isFirstNameValid = actualFirstName.Equals(testFirstName, StringComparison.OrdinalIgnoreCase);
    Console.WriteLine($"  Field 1 [First Name]: Expected='{testFirstName}', Actual='{actualFirstName}' -> {(isFirstNameValid ? "PASS" : "FAIL")}");

    bool isGenderValid = practicePage.IsGenderSelected(testGender);
    Console.WriteLine($"  Field 2 [Gender]    : Option='{testGender}', Selected={isGenderValid} -> {(isGenderValid ? "PASS" : "FAIL")}");

    string actualState = practicePage.GetSelectedState();
    bool isStateValid = actualState.Equals(testState, StringComparison.OrdinalIgnoreCase);
    Console.WriteLine($"  Field 3 [State]     : Expected='{testState}', Actual='{actualState}' -> {(isStateValid ? "PASS" : "FAIL")}");

    Console.WriteLine("--------------------------------------------------");

    if (isFirstNameValid && isGenderValid && isStateValid)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  TEST RESULT: ALL 3 FIELDS AUTOMATED & VERIFIED SUCCESSFULLY (PASS)");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  TEST RESULT: VERIFICATION FAILED");
        Console.ResetColor();
    }

    if (!Console.IsInputRedirected)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n--> Browser is open showing filled fields. Press ENTER in this terminal to close it...");
        Console.ResetColor();
        Console.ReadLine();
    }
    else
    {
        // Brief sleep so background tasks or video recordings can render
        Thread.Sleep(1500);
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n[Error] Test execution failed: {ex.Message}");
    Console.ResetColor();
}
finally
{
    driver.Quit();
}
