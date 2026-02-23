using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Pemberton.Shareclass.Hedging.Prototype.UITests.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class DesignSystemTests : AppPageTest
{
    [Test]
    public async Task Body_FontFamily_IncludesInter()
    {
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var fontFamily = await Page.Locator("body").EvaluateAsync<string>(
            "el => window.getComputedStyle(el).fontFamily");

        fontFamily.Should().Contain("Inter",
            "body font-family must include 'Inter' from the design system");
    }

    [Test]
    public async Task Root_CssCustomProperties_AreDefined()
    {
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var colorPrimary = await Page.EvaluateAsync<string>(
            "() => getComputedStyle(document.documentElement).getPropertyValue('--color-primary').trim()");
        colorPrimary.Should().NotBeNullOrEmpty(
            "--color-primary CSS custom property must be defined on :root");

        var surfaceCard = await Page.EvaluateAsync<string>(
            "() => getComputedStyle(document.documentElement).getPropertyValue('--surface-card').trim()");
        surfaceCard.Should().NotBeNullOrEmpty(
            "--surface-card CSS custom property must be defined on :root");

        var space4 = await Page.EvaluateAsync<string>(
            "() => getComputedStyle(document.documentElement).getPropertyValue('--space-4').trim()");
        space4.Should().NotBeNullOrEmpty(
            "--space-4 CSS custom property must be defined on :root");
    }

    [Test]
    public async Task Sidebar_IsExpectedWidth()
    {
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var sidebar = Page.Locator(".sidebar");
        await sidebar.WaitForAsync();

        var width = await sidebar.EvaluateAsync<int>(
            "el => el.offsetWidth");

        width.Should().Be(240,
            "sidebar must be 240px wide per the design system layout rules");
    }

    [Test]
    public async Task HomePage_NavCards_HaveExpectedIconsAndTitles()
    {
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var navCards = Page.Locator(".nav-card");
        await navCards.First.WaitForAsync();

        (await navCards.CountAsync()).Should().Be(2,
            "home page must have exactly 2 nav-card elements");

        // Diagram card
        var diagramCard = navCards.Nth(0);
        var diagramIcon = await diagramCard.Locator(".nav-card__icon").InnerTextAsync();
        diagramIcon.Should().Contain("account_tree",
            "diagram nav-card must have account_tree icon");
        var diagramTitle = await diagramCard.Locator(".nav-card__title").InnerTextAsync();
        diagramTitle.Should().Contain("Workflow Diagram",
            "diagram nav-card must have 'Workflow Diagram' title");

        // Wizard card
        var wizardCard = navCards.Nth(1);
        var wizardIcon = await wizardCard.Locator(".nav-card__icon").InnerTextAsync();
        wizardIcon.Should().Contain("play_circle",
            "wizard nav-card must have play_circle icon");
        var wizardTitle = await wizardCard.Locator(".nav-card__title").InnerTextAsync();
        wizardTitle.Should().Contain("Execution Wizard",
            "wizard nav-card must have 'Execution Wizard' title");
    }

    [Test]
    public async Task WizardStep_HasStepContextLabel()
    {
        await Page.GotoAsync("/wizard");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var stepContext = Page.Locator(".step-context");
        await stepContext.WaitForAsync();

        var text = await stepContext.InnerTextAsync();
        text.ToUpperInvariant().Should().Contain("STEP 1 OF 9",
            "wizard step 0 must show 'Step 1 of 9' context label (CSS text-transform: uppercase)");
    }

    [Test]
    public async Task ApprovalStep_ShowsStatusBadges()
    {
        await Page.GotoAsync("/wizard");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Navigate to approval step (step 5) by clicking Next 5 times
        var nextBtn = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Next" });

        var step1 = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Record FX Data" });
        await nextBtn.ClickAsync();
        await step1.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });

        var step2 = Page.GetByText("Validation:");
        await nextBtn.ClickAsync();
        await step2.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });

        var step3 = Page.GetByText("Hedge Instructions Calculated");
        await nextBtn.ClickAsync();
        await step3.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });

        var step4 = Page.GetByText("Trade instructions ready for approval");
        await nextBtn.ClickAsync();
        await step4.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });

        var step5 = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Approvals" });
        await nextBtn.ClickAsync();
        await step5.WaitForAsync(new LocatorWaitForOptions { Timeout = 8000 });

        // Verify status-badge elements exist
        var badges = Page.Locator(".status-badge");
        await badges.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });

        (await badges.CountAsync()).Should().BeGreaterOrEqualTo(4,
            "approval step must show at least 4 status-badge elements (one per approver)");
    }

    [Test]
    public async Task Wizard_HasActionBarWithButtons()
    {
        await Page.GotoAsync("/wizard");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var actionBar = Page.Locator(".action-bar");
        await actionBar.WaitForAsync();

        var backBtn = actionBar.Locator("button").Filter(new LocatorFilterOptions { HasText = "Back" });
        (await backBtn.CountAsync()).Should().BeGreaterOrEqualTo(1,
            "action-bar must contain a Back button");

        var nextBtn = actionBar.Locator("button").Filter(new LocatorFilterOptions { HasText = "Next" });
        (await nextBtn.CountAsync()).Should().BeGreaterOrEqualTo(1,
            "action-bar must contain a Next button");
    }

    [Test]
    public async Task DiagramPage_HasColorLegend()
    {
        await Page.GotoAsync("/diagram");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var legend = Page.Locator(".color-legend");
        await legend.WaitForAsync(new LocatorWaitForOptions { Timeout = 8000 });

        await Expect(legend).ToBeVisibleAsync();

        var items = legend.Locator(".color-legend__item");
        (await items.CountAsync()).Should().Be(4,
            "color legend must have 4 category items (Data Input, Processing, Approval, Output)");
    }
}
