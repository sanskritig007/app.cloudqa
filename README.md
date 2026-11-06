# Resilient Form Automation (CloudQA Practice Form)

An intelligent, self-healing web test automation suite built with **C#** and **Selenium WebDriver** for the [CloudQA Automation Practice Form](https://app.cloudqa.io/home/AutomationPracticeForm).

Designed specifically to survive dynamic page changes — such as altered IDs, swapped field orders, modified CSS classes, and varying DOM hierarchies — without breaking.

---

## 🎯 Target Fields Automated
1. **First Name** (Text box)
2. **Gender** (Radio buttons: Male / Female / Transgender)
3. **State** (Dropdown select element)

---

## 💡 Key Architectural Highlights

### 1. Multi-Tiered Self-Healing Locator Engine (`ResilientLocator.cs`)
Traditional automation scripts break the moment an element's `id` or `xpath` changes. This suite implements a tiered fallback strategy:
* **Tier 1 (Fast Path — Direct Attributes):** Checks standard `id` or `name` attributes first for optimal speed.
* **Tier 2 (Semantic Label Proximity):** If standard attributes change or disappear, it locates the visible human label (e.g. `"First Name"`, `"Gender"`, `"State"`) and traverses to the associated `<input>`, `<select>`, or radio group using relative DOM relationships (`for` attribute, parent container scoping, and sibling proximity).
* **Tier 3 (Fuzzy Heuristics):** Matches based on `placeholder`, `aria-label`, or partial attribute values.
* **Self-Healing Telemetry:** Transparently logs whenever a fallback is activated (`⚡ [Self-Healed]`), detailing which strategy successfully recovered the element.

### 2. Configurable Target URL (`appsettings.json` & CLI)
The page URL is never hardcoded. It can be configured via:
* `appsettings.json` (`TestSettings:BaseUrl`)
* Command-line argument override: `dotnet run -- <URL>`

### 3. Page Object Model (POM) Design (`AutomationPracticePage.cs`)
Clean separation between locator/interaction logic and test assertions, ensuring readability and maintainability.

---

## 🚀 Setup & Execution

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* Google Chrome installed

### 1. Run Against Live CloudQA Practice Form
```bash
dotnet run
```
*Navigates to the live CloudQA form, fills in the fields, verifies the values, and prints execution results.*

### 2. Run Against Modified / Chaos Test Page (Resilience Proof)
To prove that the suite does not rely on fixed locators, a modified version of the page is included at `TestPages/modified_page.html` (where IDs are scrambled, classes altered, and field order inverted):
```bash
dotnet run -- file://$PWD/TestPages/modified_page.html
```
*Observe the console output — the self-healing engine automatically adapts to the changed DOM and passes all verifications!*

---

## 📋 Approach & Limitations Note

### Technical Approach
1. **Prioritize Human Semantics over Fragile IDs:** UI labels change far less frequently than backend-generated IDs or class names. Binding inputs to their visible labels mirrors how a real human interacts with the form.
2. **Scoping within Containers:** To avoid false positives on pages with duplicate field names (such as the Shadow DOM and IFrames present on CloudQA), locators scope elements within their immediate container wrapper.
3. **Graceful Degradation:** Instead of throwing `NoSuchElementException` immediately, each tier is evaluated with lightweight, non-blocking probes.

### Limitations & Trade-offs
* **Label Text Dependency:** If a page undergoes localization (e.g., English to German) and label text changes from "First Name" to "Vorname", label proximity requires an updated dictionary or translation layer.
* **Extremely Flat DOM Structures:** If a form has zero container hierarchy (`<div>`, `<fieldset>`) and places all inputs and labels in a single flat list without `for` attributes, spatial bounding-box heuristics (visual AI or coordinate proximity) would be needed as a next tier.
* **Shadow DOM / Closed Roots:** Elements encapsulated inside closed Shadow DOM boundaries cannot be accessed via standard global DOM traversal without piercing the shadow root via JavaScript.
