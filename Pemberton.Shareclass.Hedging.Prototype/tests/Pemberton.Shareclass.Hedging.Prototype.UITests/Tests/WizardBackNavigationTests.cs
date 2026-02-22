using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Pemberton.Shareclass.Hedging.Prototype.UITests.Tests;

/// <summary>
/// E2E tests for the wizard Back button navigation.
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class WizardBackNavigationTests : AppPageTest
{
    [SetUp]
    public async Task NavigateToWizard()
    {
        await Page.GotoAsync("/wizard");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    // ── helpers ──────────────────────────────────────────────────────────

    private async Task ClickNextAndWaitFor(ILocator awaitLocator, int timeoutMs = 5000)
    {
        var nextBtn = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Next" });
        await nextBtn.ClickAsync();
        await awaitLocator.WaitForAsync(new LocatorWaitForOptions { Timeout = timeoutMs });
    }

    private async Task ClickBackAndWaitFor(ILocator awaitLocator, int timeoutMs = 5000)
    {
        var backBtn = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Back" });
        await backBtn.ClickAsync();
        await awaitLocator.WaitForAsync(new LocatorWaitForOptions { Timeout = timeoutMs });
    }

    // ── back navigation ─────────────────────────────────────────────────

    [Test]
    public async Task BackFromStep1_ReturnsToStep0()
    {
        // Advance to step 1
        var step1Header = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Record FX Data" });
        await ClickNextAndWaitFor(step1Header);

        // Go back to step 0
        var step0Header = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "BNY Data" });
        await ClickBackAndWaitFor(step0Header);

        (await step0Header.IsVisibleAsync()).Should().BeTrue(
            "pressing Back from step 1 must return to step 0 (BNY Data)");
    }

    [Test]
    public async Task BackFromStep2_ReturnsToStep1()
    {
        // Advance to step 1
        var step1Header = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Record FX Data" });
        await ClickNextAndWaitFor(step1Header);

        // Advance to step 2
        var validationText = Page.GetByText("Validation:");
        await ClickNextAndWaitFor(validationText);

        // Go back to step 1
        await ClickBackAndWaitFor(step1Header);

        (await step1Header.IsVisibleAsync()).Should().BeTrue(
            "pressing Back from step 2 must return to step 1 (Record FX Data)");
    }

    [Test]
    public async Task BackButton_IsDisabled_OnStep0()
    {
        var backBtn = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Back" });
        await backBtn.WaitForAsync();

        (await backBtn.IsDisabledAsync()).Should().BeTrue(
            "Back button must be disabled on step 0");
    }

    [Test]
    public async Task BackThenForward_RoundTrip_ReturnsToSameStep()
    {
        // Advance to step 1
        var step1Header = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Record FX Data" });
        await ClickNextAndWaitFor(step1Header);

        // Advance to step 2
        var validationText = Page.GetByText("Validation:");
        await ClickNextAndWaitFor(validationText);

        // Back to step 1
        await ClickBackAndWaitFor(step1Header);

        // Forward to step 2 again
        await ClickNextAndWaitFor(validationText);

        (await validationText.IsVisibleAsync()).Should().BeTrue(
            "navigating Back then Forward must return to the same step (step 2)");
    }
}
