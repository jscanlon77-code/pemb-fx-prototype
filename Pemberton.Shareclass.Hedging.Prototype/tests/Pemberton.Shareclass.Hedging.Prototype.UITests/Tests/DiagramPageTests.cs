using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Pemberton.Shareclass.Hedging.Prototype.UITests.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class DiagramPageTests : AppPageTest
{
    [SetUp]
    public async Task NavigateToDiagram()
    {
        await Page.GotoAsync("/diagram");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Test]
    public async Task DiagramPage_HasExpectedHeading()
    {
        var heading = Page.Locator("h3").First;
        await heading.WaitForAsync();
        (await heading.InnerTextAsync()).Should().Be("Hedging Workflow Diagram");
    }

    [Test]
    public async Task DiagramPage_CanvasContainer_IsPresent()
    {
        // HedgingDiagram.razor: <div style="width:100%; height:500px;">
        var container = Page.Locator("div[style*='height:500px'], div[style*='height: 500px']");
        await container.WaitForAsync(new LocatorWaitForOptions { Timeout = 8000 });
        (await container.IsVisibleAsync()).Should().BeTrue(
            "the diagram canvas container div must be rendered and visible");
    }

    [Test]
    public async Task DiagramPage_SvgCanvas_IsRendered()
    {
        // Z.Blazor.Diagrams renders nodes into an SVG canvas after Blazor hydration
        var svg = Page.Locator("svg").First;
        await svg.WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        (await svg.IsVisibleAsync()).Should().BeTrue(
            "the diagram library must render an SVG element");
    }

    [Test]
    public async Task DiagramPage_SidebarLinks_StillPresent()
    {
        // Sidebar should be visible on every page
        (await Page.Locator(".nav-menu a[href='diagram']").IsVisibleAsync()).Should().BeTrue();
        (await Page.Locator(".nav-menu a[href='wizard']").IsVisibleAsync()).Should().BeTrue();
    }
}
