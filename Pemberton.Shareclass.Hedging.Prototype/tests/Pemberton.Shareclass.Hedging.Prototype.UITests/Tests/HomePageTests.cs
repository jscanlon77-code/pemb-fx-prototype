using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Pemberton.Shareclass.Hedging.Prototype.UITests.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class HomePageTests : AppPageTest
{
    [SetUp]
    public async Task NavigateToHome()
    {
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Test]
    public async Task HomePage_HasExpectedHeading()
    {
        var heading = Page.Locator("h3").First;
        await heading.WaitForAsync();
        (await heading.InnerTextAsync()).Should().Be("FX Hedging Dashboard");
    }

    [Test]
    public async Task HomePage_TopRowShowsAppTitle()
    {
        var title = Page.Locator(".top-row h2");
        await title.WaitForAsync();
        (await title.InnerTextAsync()).Should().Contain("Pemberton");
    }

    [Test]
    public async Task HomePage_SidebarContainsHomeLink()
    {
        var link = Page.Locator(".nav-menu a").Filter(new LocatorFilterOptions { HasText = "Home" });
        await link.WaitForAsync();
        (await link.IsVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task HomePage_SidebarContainsDiagramLink()
    {
        var link = Page.Locator(".nav-menu a[href='diagram']");
        await link.WaitForAsync();
        (await link.IsVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task HomePage_SidebarContainsWizardLink()
    {
        var link = Page.Locator(".nav-menu a[href='wizard']");
        await link.WaitForAsync();
        (await link.IsVisibleAsync()).Should().BeTrue();
    }

    [Test]
    public async Task HomePage_HasLinksToWizardAndDiagram()
    {
        // Index.razor renders NavLinks in the page body as well
        var diagramLink = Page.Locator("article a[href='diagram']");
        var wizardLink = Page.Locator("article a[href='wizard']");
        await diagramLink.WaitForAsync();
        await wizardLink.WaitForAsync();
        (await diagramLink.IsVisibleAsync()).Should().BeTrue();
        (await wizardLink.IsVisibleAsync()).Should().BeTrue();
    }
}
