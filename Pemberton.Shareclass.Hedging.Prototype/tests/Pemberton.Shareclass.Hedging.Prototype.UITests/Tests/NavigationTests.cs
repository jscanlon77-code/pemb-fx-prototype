using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Pemberton.Shareclass.Hedging.Prototype.UITests.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class NavigationTests : AppPageTest
{
    [SetUp]
    public async Task NavigateToHome()
    {
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    // ── existing navigation tests ───────────────────────────────────────

    [Test]
    public async Task ClickingDiagramLink_NavigatesToDiagramPage()
    {
        await Page.Locator(".nav-menu a[href='diagram']").ClickAsync();
        await Page.WaitForURLAsync("**/diagram");

        var heading = Page.Locator("h3").First;
        (await heading.InnerTextAsync()).Should().Be("Hedging Workflow Diagram");
    }

    [Test]
    public async Task ClickingWizardLink_NavigatesToWizardPage()
    {
        await Page.Locator(".nav-menu a[href='wizard']").ClickAsync();
        await Page.WaitForURLAsync("**/wizard");

        var heading = Page.Locator("h3").First;
        (await heading.InnerTextAsync()).Should().Be("Hedging Execution Wizard");
    }

    [Test]
    [Description("Regression: active sidebar link must remain clickable after CSS fix")]
    public async Task ActiveSidebarLink_HasPointerEventsAuto()
    {
        await Page.GotoAsync("/wizard");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var activeLink = Page.Locator(".nav-menu a.active");
        await activeLink.WaitForAsync();

        var pointerEvents = await activeLink.EvaluateAsync<string>(
            "el => window.getComputedStyle(el).pointerEvents");

        pointerEvents.Should().Be("auto",
            "active nav links must never have pointer-events disabled");
    }

    [Test]
    [Description("Regression: active sidebar link cursor must be pointer")]
    public async Task ActiveSidebarLink_HasPointerCursor()
    {
        await Page.GotoAsync("/wizard");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var activeLink = Page.Locator(".nav-menu a.active");
        await activeLink.WaitForAsync();

        var cursor = await activeLink.EvaluateAsync<string>(
            "el => window.getComputedStyle(el).cursor");

        cursor.Should().Be("pointer");
    }

    [Test]
    [Description("Regression: clicking the active link must not throw or freeze")]
    public async Task ActiveHomeLink_CanBeClicked()
    {
        var homeLink = Page.Locator(".nav-menu a.active");
        await homeLink.WaitForAsync();
        await homeLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Page.Url.Should().MatchRegex(@"(localhost|127\.0\.0\.1):\d+/?$");
    }

    [Test]
    public async Task AfterNavigatingToDiagram_WizardLinkStillWorks()
    {
        await Page.Locator(".nav-menu a[href='diagram']").ClickAsync();
        await Page.WaitForURLAsync("**/diagram");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator(".nav-menu a[href='wizard']").ClickAsync();

        await Page.WaitForURLAsync("**/wizard");
        Page.Url.Should().Contain("/wizard",
            "clicking the wizard nav link from the diagram page must navigate to /wizard");
    }

    // ── sidebar styling tests ───────────────────────────────────────────

    [Test]
    [Description("Nav links must have transition property for smooth hover effects")]
    public async Task SidebarLink_HasTransitionProperty()
    {
        var link = Page.Locator(".nav-menu a").First;
        await link.WaitForAsync();

        var transition = await link.EvaluateAsync<string>(
            "el => window.getComputedStyle(el).transition");

        transition.Should().Contain("color",
            "nav links must transition the color property");
    }

    [Test]
    [Description("Active nav link must have a highlight color")]
    public async Task ActiveLink_HasHighlightColor()
    {
        await Page.GotoAsync("/wizard");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var activeLink = Page.Locator(".nav-menu a.active");
        await activeLink.WaitForAsync();

        var color = await activeLink.EvaluateAsync<string>(
            "el => window.getComputedStyle(el).color");

        // --color-accent: #0ea5e9 = rgb(14, 165, 233)
        color.Should().Be("rgb(14, 165, 233)",
            "active nav link must use the accent color #0ea5e9");
    }

    [Test]
    [Description("Nav links must use flex display for icon + text alignment")]
    public async Task SidebarLink_HasFlexDisplay()
    {
        var link = Page.Locator(".nav-menu a").First;
        await link.WaitForAsync();

        var display = await link.EvaluateAsync<string>(
            "el => window.getComputedStyle(el).display");

        display.Should().Be("flex",
            "nav links must use display:flex for icon + text alignment");
    }

    [Test]
    [Description("Active link must have a left border indicator")]
    public async Task ActiveLink_HasLeftBorderIndicator()
    {
        await Page.GotoAsync("/wizard");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var activeLink = Page.Locator(".nav-menu a.active");
        await activeLink.WaitForAsync();

        var borderLeft = await activeLink.EvaluateAsync<string>(
            "el => window.getComputedStyle(el).borderLeftStyle");

        borderLeft.Should().Be("solid",
            "active nav link must have a solid left border indicator");
    }
}
