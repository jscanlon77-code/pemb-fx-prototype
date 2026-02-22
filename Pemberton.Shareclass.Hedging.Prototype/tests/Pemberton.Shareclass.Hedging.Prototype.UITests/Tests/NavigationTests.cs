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
        // Navigate to wizard so the wizard link becomes active
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
        // Home is active; clicking it again must succeed
        var homeLink = Page.Locator(".nav-menu a.active");
        await homeLink.WaitForAsync();
        await homeLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Still on home (accept both localhost and 127.0.0.1)
        Page.Url.Should().MatchRegex(@"(localhost|127\.0\.0\.1):\d+/?$");
    }

    [Test]
    public async Task AfterNavigatingToDiagram_WizardLinkStillWorks()
    {
        await Page.Locator(".nav-menu a[href='diagram']").ClickAsync();
        await Page.WaitForURLAsync("**/diagram");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator(".nav-menu a[href='wizard']").ClickAsync();

        // The wizard nav link must successfully navigate to /wizard — this is the
        // regression being tested (the link must remain clickable after diagram visit).
        await Page.WaitForURLAsync("**/wizard");
        Page.Url.Should().Contain("/wizard",
            "clicking the wizard nav link from the diagram page must navigate to /wizard");
    }
}
