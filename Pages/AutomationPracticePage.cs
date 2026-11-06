using CloudQA.ResilientAutomation.Locators;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace CloudQA.ResilientAutomation.Pages;

/// <summary>
/// Page Object Model representing the CloudQA Automation Practice Form.
/// Encapsulates interactions with the 3 target fields:
/// 1. First Name (text box)
/// 2. Gender (radio button)
/// 3. State (dropdown)
/// </summary>
public class AutomationPracticePage
{
    private readonly IWebDriver _driver;

    public AutomationPracticePage(IWebDriver driver)
    {
        _driver = driver;
    }

    /// <summary>
    /// Fills in the First Name field resiliently.
    /// </summary>
    public void SetFirstName(string firstName)
    {
        Console.WriteLine($"\n[Action] Setting First Name to: '{firstName}'");
        
        var input = ResilientLocator.FindTextInput(
            _driver,
            labelText: "First Name",
            primaryId: "fname",
            primaryName: "First Name",
            placeholder: "Name"
        );

        input.Clear();
        // Clear via JS fallback in case autofill or virtual DOM retains old values
        if (!string.IsNullOrEmpty(input.GetAttribute("value")))
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].value = '';", input);
        }

        input.SendKeys(firstName);
    }

    /// <summary>
    /// Gets the current value from the First Name field.
    /// </summary>
    public string GetFirstName()
    {
        var input = ResilientLocator.FindTextInput(
            _driver,
            labelText: "First Name",
            primaryId: "fname",
            primaryName: "First Name"
        );
        return input.GetAttribute("value") ?? string.Empty;
    }

    /// <summary>
    /// Selects a Gender radio option (e.g. "Male", "Female", "Transgender").
    /// </summary>
    public void SelectGender(string genderOption)
    {
        Console.WriteLine($"\n[Action] Selecting Gender: '{genderOption}'");

        var radio = ResilientLocator.FindRadioButton(
            _driver,
            groupLabel: "Gender",
            optionText: genderOption
        );

        // Attempt normal user click
        try
        {
            radio.Click();
        }
        catch (WebDriverException)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", radio);
        }

        // If click was swallowed by an overlapping label wrapper, trigger direct event
        if (!radio.Selected)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].checked = true; arguments[0].dispatchEvent(new Event('change'));", radio);
        }
    }

    /// <summary>
    /// Checks if a particular gender radio button is selected.
    /// </summary>
    public bool IsGenderSelected(string genderOption)
    {
        var radio = ResilientLocator.FindRadioButton(
            _driver,
            groupLabel: "Gender",
            optionText: genderOption
        );
        return radio.Selected;
    }

    /// <summary>
    /// Selects a State from the dropdown by its visible text (e.g. "India", "United States", "Australia").
    /// </summary>
    public void SelectState(string stateName)
    {
        Console.WriteLine($"\n[Action] Selecting State: '{stateName}'");

        var dropdownElement = ResilientLocator.FindDropdown(
            _driver,
            labelText: "State",
            primaryId: "state",
            primaryName: "State"
        );

        var select = new SelectElement(dropdownElement);
        select.SelectByText(stateName);
    }

    /// <summary>
    /// Gets the currently selected option text from the State dropdown.
    /// </summary>
    public string GetSelectedState()
    {
        var dropdownElement = ResilientLocator.FindDropdown(
            _driver,
            labelText: "State",
            primaryId: "state",
            primaryName: "State"
        );

        var select = new SelectElement(dropdownElement);
        return select.SelectedOption.Text.Trim();
    }
}
