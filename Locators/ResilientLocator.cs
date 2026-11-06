using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace CloudQA.ResilientAutomation.Locators;

/// <summary>
/// A resilient element locator engine that implements a tiered fallback (self-healing) strategy.
/// If primary attributes (ID, Name) change or break, it automatically recovers using
/// semantic label proximity, DOM relationships, and fuzzy attribute heuristics.
/// </summary>
public static class ResilientLocator
{
    private static void LogDirectHit(string fieldName, string strategy)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ [Direct Hit] Located '{fieldName}' using: {strategy}");
        Console.ResetColor();
    }

    private static void LogSelfHealed(string fieldName, string fallbackStrategy, string reason)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ⚡ [Self-Healed] Standard locator failed for '{fieldName}' ({reason}).");
        Console.WriteLine($"     Recovered using: {fallbackStrategy}");
        Console.ResetColor();
    }

    /// <summary>
    /// Attempts to find a single element safely without throwing NoSuchElementException immediately.
    /// </summary>
    private static IWebElement? TryFind(IWebDriver driver, By by)
    {
        try
        {
            var elements = driver.FindElements(by);
            foreach (var el in elements)
            {
                if (el.Displayed && el.Enabled)
                {
                    return el;
                }
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Resiliently finds a text input field (e.g. First Name).
    /// </summary>
    public static IWebElement FindTextInput(
        IWebDriver driver,
        string labelText,
        string? primaryId = null,
        string? primaryName = null,
        string? placeholder = null)
    {
        // Tier 1: Try Primary Direct Attributes (Id / Name) if provided
        if (!string.IsNullOrEmpty(primaryId))
        {
            var el = TryFind(driver, By.Id(primaryId));
            if (el != null)
            {
                LogDirectHit(labelText, $"ID='{primaryId}'");
                return el;
            }
        }

        if (!string.IsNullOrEmpty(primaryName))
        {
            var el = TryFind(driver, By.Name(primaryName));
            if (el != null)
            {
                LogDirectHit(labelText, $"Name='{primaryName}'");
                return el;
            }
        }

        // Tier 2: Semantic Label Proximity (Find label text, then locate nearest input)
        // 2A: Via 'for' attribute matching if present on label
        var labelEl = TryFind(driver, By.XPath($"//label[contains(normalize-space(), '{labelText}')]"));
        if (labelEl != null)
        {
            var forAttr = labelEl.GetAttribute("for");
            if (!string.IsNullOrEmpty(forAttr))
            {
                var inputByFor = TryFind(driver, By.Id(forAttr));
                if (inputByFor != null)
                {
                    LogSelfHealed(labelText, $"Label[for='{forAttr}'] association", "Primary ID/Name missing");
                    return inputByFor;
                }
            }

            // 2B: Proximity inside the same parent container (e.g. form-group)
            var inputInParent = TryFind(driver, By.XPath($"//label[contains(normalize-space(), '{labelText}')]/..//input[not(@type='hidden')]"));
            if (inputInParent != null)
            {
                LogSelfHealed(labelText, $"Parent container proximity to label '{labelText}'", "Primary ID/Name missing");
                return inputInParent;
            }

            // 2C: Immediate following input
            var followingInput = TryFind(driver, By.XPath($"//label[contains(normalize-space(), '{labelText}')]/following::input[not(@type='hidden')][1]"));
            if (followingInput != null)
            {
                LogSelfHealed(labelText, $"Following sibling proximity to label '{labelText}'", "Primary ID/Name missing");
                return followingInput;
            }
        }

        // Tier 3: Fuzzy Attribute / Placeholder Matching
        if (!string.IsNullOrEmpty(placeholder))
        {
            var inputByPlaceholder = TryFind(driver, By.XPath($"//input[contains(translate(@placeholder, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '{placeholder.ToLower()}')]"));
            if (inputByPlaceholder != null)
            {
                LogSelfHealed(labelText, $"Placeholder heuristic ('{placeholder}')", "Label and standard attributes changed");
                return inputByPlaceholder;
            }
        }

        throw new NoSuchElementException($"[ResilientLocator] Failed to locate text input for '{labelText}' using all fallback tiers.");
    }

    /// <summary>
    /// Resiliently finds a radio button (e.g. Gender -> Male / Female / Transgender).
    /// </summary>
    public static IWebElement FindRadioButton(
        IWebDriver driver,
        string groupLabel,
        string optionText)
    {
        // Tier 1: Direct Value or ID match (case-insensitive)
        var directRadio = TryFind(driver, By.XPath($"//input[@type='radio' and (translate(@value, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='{optionText.ToLower()}' or translate(@id, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='{optionText.ToLower()}')]"));
        if (directRadio != null)
        {
            LogDirectHit($"{groupLabel} -> {optionText}", $"Radio input with value/id='{optionText}'");
            return directRadio;
        }

        // Tier 2: Semantic Proximity to option text label/span
        // e.g. <input ... /> <span>Male</span> OR <label>Male <input .../></label>
        var radioByPrecedingText = TryFind(driver, By.XPath($"//*[contains(normalize-space(), '{optionText}')]/preceding-sibling::input[@type='radio'][1]"));
        if (radioByPrecedingText != null)
        {
            LogSelfHealed($"{groupLabel} -> {optionText}", $"Preceding sibling of text '{optionText}'", "Standard value/ID changed");
            return radioByPrecedingText;
        }

        var radioInSameWrapper = TryFind(driver, By.XPath($"//*[contains(normalize-space(), '{optionText}')]/..//input[@type='radio']"));
        if (radioInSameWrapper != null)
        {
            LogSelfHealed($"{groupLabel} -> {optionText}", $"Same wrapper as text '{optionText}'", "Standard value/ID changed");
            return radioInSameWrapper;
        }

        // Tier 3: Proximity within the group container
        var radioInGroup = TryFind(driver, By.XPath($"//*[contains(normalize-space(), '{groupLabel}')]/following::input[@type='radio' and contains(@value, '{optionText}')][1]"));
        if (radioInGroup != null)
        {
            LogSelfHealed($"{groupLabel} -> {optionText}", $"Group proximity under '{groupLabel}'", "Direct radio locators failed");
            return radioInGroup;
        }

        throw new NoSuchElementException($"[ResilientLocator] Failed to locate radio button for '{groupLabel} -> {optionText}'.");
    }

    /// <summary>
    /// Resiliently finds a dropdown select element (e.g. State).
    /// </summary>
    public static IWebElement FindDropdown(
        IWebDriver driver,
        string labelText,
        string? primaryId = null,
        string? primaryName = null)
    {
        // Tier 1: Direct ID / Name match
        if (!string.IsNullOrEmpty(primaryId))
        {
            var el = TryFind(driver, By.Id(primaryId));
            if (el != null)
            {
                LogDirectHit(labelText, $"ID='{primaryId}'");
                return el;
            }
        }

        if (!string.IsNullOrEmpty(primaryName))
        {
            var el = TryFind(driver, By.Name(primaryName));
            if (el != null)
            {
                LogDirectHit(labelText, $"Name='{primaryName}'");
                return el;
            }
        }

        // Tier 2: Semantic Proximity to Dropdown Label
        var selectInContainer = TryFind(driver, By.XPath($"//label[contains(normalize-space(), '{labelText}')]/..//select"));
        if (selectInContainer != null)
        {
            LogSelfHealed(labelText, $"Parent container of label '{labelText}'", "Primary ID/Name missing");
            return selectInContainer;
        }

        var followingSelect = TryFind(driver, By.XPath($"//label[contains(normalize-space(), '{labelText}')]/following::select[1]"));
        if (followingSelect != null)
        {
            LogSelfHealed(labelText, $"Following sibling select under '{labelText}'", "Primary ID/Name missing");
            return followingSelect;
        }

        // Tier 3: Any select whose name/id contains the label text
        var fuzzySelect = TryFind(driver, By.XPath($"//select[contains(translate(@id, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '{labelText.ToLower()}') or contains(translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '{labelText.ToLower()}')]"));
        if (fuzzySelect != null)
        {
            LogSelfHealed(labelText, $"Fuzzy select attribute match for '{labelText}'", "Label proximity missing");
            return fuzzySelect;
        }

        throw new NoSuchElementException($"[ResilientLocator] Failed to locate dropdown for '{labelText}'.");
    }
}

