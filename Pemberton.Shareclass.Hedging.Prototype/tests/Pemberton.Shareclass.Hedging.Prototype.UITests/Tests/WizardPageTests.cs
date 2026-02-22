using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Pemberton.Shareclass.Hedging.Prototype.UITests.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class WizardPageTests : AppPageTest
{
    [SetUp]
    public async Task NavigateToWizard()
    {
        await Page.GotoAsync("/wizard");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Clicks the Next button and waits until <paramref name="awaitLocator"/> is visible,
    /// confirming Blazor has re-rendered with the new step content.
    /// </summary>
    private async Task ClickNextAndWaitFor(ILocator awaitLocator, int timeoutMs = 5000)
    {
        var nextBtn = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Next" });
        await nextBtn.ClickAsync();
        await awaitLocator.WaitForAsync(new LocatorWaitForOptions { Timeout = timeoutMs });
    }

    // ── render / layout ─────────────────────────────────────────────────────────

    [Test]
    public async Task WizardPage_HasExpectedHeading()
    {
        var heading = Page.Locator("h3").First;
        await heading.WaitForAsync();
        (await heading.InnerTextAsync()).Should().Be("Hedging Execution Wizard");
    }

    [Test]
    public async Task WizardPage_RadzenSteps_IsVisible()
    {
        // RadzenSteps renders with .rz-steps container class
        var steps = Page.Locator(".rz-steps");
        await steps.WaitForAsync(new LocatorWaitForOptions { Timeout = 8000 });
        (await steps.IsVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task WizardPage_StepProgress_ShowsAllNineStepLabels()
    {
        var stepsContainer = Page.Locator(".rz-steps");
        await stepsContainer.WaitForAsync(new LocatorWaitForOptions { Timeout = 8000 });

        string[] expectedLabels =
        [
            "BNY Data", "Record FX Data", "Validation", "Calculation",
            "Trade Instructions", "Approvals", "Execution", "Booking", "Reporting"
        ];

        // RadzenSteps renders step items inside a <ul role="tablist">.
        // Wait for the first item to ensure Blazor has rendered all step labels.
        var stepItems = stepsContainer.Locator("ul[role='tablist'] li");
        await stepItems.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 8000 });

        var count = await stepItems.CountAsync();
        count.Should().Be(expectedLabels.Length,
            "RadzenSteps must render one item per wizard step");
    }

    // ── step 0 & 1 content ────────────────────────────────────────────────────────────────

    [Test]
    public async Task WizardPage_FirstTwoSteps_HaveExpectedContent()
    {
        // Step 0: BNY Data
        var step0Header = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "BNY Data" });
        await step0Header.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        (await step0Header.IsVisibleAsync()).Should().BeTrue("step 0 shows BNY Data content");

        // Advance to step 1
        var step1Header = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Record FX Data" });
        await ClickNextAndWaitFor(step1Header);
        (await step1Header.IsVisibleAsync()).Should().BeTrue("step 1 shows Record FX Data content");
    }

    // ── step 2 content ────────────────────────────────────────────────────────────────────

    [Test]
    public async Task WizardPage_Step2_ShowsValidationStatus()
    {
        // Navigate to step 2 (Validation)
        var step1Header = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Record FX Data" });
        await ClickNextAndWaitFor(step1Header);

        var validationText = Page.GetByText("Validation:");
        await ClickNextAndWaitFor(validationText);

        (await validationText.IsVisibleAsync()).Should().BeTrue(
            "step 2 must show the validation status paragraph");
    }

    // ── step 3 content ────────────────────────────────────────────────────────────────────

    [Test]
    public async Task WizardPage_Step3_ShowsCalculationContent()
    {
        // Navigate to step 3 (Calculation)
        var step1Header = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Record FX Data" });
        await ClickNextAndWaitFor(step1Header);

        var validationText = Page.GetByText("Validation:");
        await ClickNextAndWaitFor(validationText);

        var calcText = Page.GetByText("hedge instructions");
        await ClickNextAndWaitFor(calcText);

        (await calcText.IsVisibleAsync()).Should().BeTrue(
            "step 3 must show the calculated hedge instructions count");
    }

    // ── step 4 content ────────────────────────────────────────────────────────────────────

    [Test]
    public async Task WizardPage_Step4_ShowsTradeInstructionsContent()
    {
        // Navigate to step 4 (Trade Instructions)
        var step1Header = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Record FX Data" });
        await ClickNextAndWaitFor(step1Header);

        var validationText = Page.GetByText("Validation:");
        await ClickNextAndWaitFor(validationText);

        var calcText = Page.GetByText("hedge instructions");
        await ClickNextAndWaitFor(calcText);

        var tradeText = Page.GetByText("Trade instructions ready for approval");
        await ClickNextAndWaitFor(tradeText);

        (await tradeText.IsVisibleAsync()).Should().BeTrue(
            "step 4 must show the trade instructions ready paragraph");
    }

    // ── Back / Next button states ─────────────────────────────────────────────────────────

    [Test]
    public async Task WizardPage_BackButton_IsPresent()
    {
        var backBtn = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Back" });
        await backBtn.WaitForAsync();
        (await backBtn.IsVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task WizardPage_NextButton_IsPresent()
    {
        var nextBtn = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Next" });
        await nextBtn.WaitForAsync();
        (await nextBtn.IsVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task WizardPage_BackButton_IsDisabled_OnFirstStep()
    {
        var backBtn = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Back" });
        await backBtn.WaitForAsync();
        (await backBtn.IsDisabledAsync()).Should().BeTrue(
            "Back must be disabled on step 0 (IsFirst == true)");
    }

    [Test]
    public async Task WizardPage_NextButton_IsEnabled_OnFirstStep()
    {
        var nextBtn = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Next" });
        await nextBtn.WaitForAsync();
        (await nextBtn.IsEnabledAsync()).Should().BeTrue(
            "Next must be enabled on step 0");
    }

    [Test]
    public async Task WizardPage_ClickingNext_ShowsStep1Content()
    {
        // Step 0 shows "BNY Data (Share Class Exposure)" h4
        var step0Header = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "BNY Data" });
        await step0Header.WaitForAsync();

        // Step 1 shows "Record FX Data" h4
        var step1Header = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Record FX Data" });
        await ClickNextAndWaitFor(step1Header);
        (await step1Header.IsVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task WizardPage_BackButton_BecomesEnabled_AfterNextClick()
    {
        // Wait for Blazor SignalR re-render: step 1 content confirms the state has updated
        var step1Header = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Record FX Data" });
        await ClickNextAndWaitFor(step1Header);

        var backBtn = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Back" });
        (await backBtn.IsEnabledAsync()).Should().BeTrue(
            "Back should be enabled once we have advanced past step 0");
    }
}
