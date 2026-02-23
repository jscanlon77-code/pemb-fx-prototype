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

    // ── heading ─────────────────────────────────────────────────────────

    [Test]
    public async Task DiagramPage_HasExpectedHeading()
    {
        var heading = Page.Locator("h3").First;
        await heading.WaitForAsync();
        (await heading.InnerTextAsync()).Should().Be("Hedging Workflow Diagram");
    }

    // ── grid container ──────────────────────────────────────────────────

    [Test]
    public async Task DiagramPage_GridContainer_IsPresent()
    {
        var grid = Page.Locator(".workflow-grid");
        await grid.WaitForAsync(new LocatorWaitForOptions { Timeout = 8000 });
        (await grid.IsVisibleAsync()).Should().BeTrue(
            "the workflow grid container must be rendered and visible");
    }

    [Test]
    public async Task DiagramPage_GridContainer_UsesGridDisplay()
    {
        var grid = Page.Locator(".workflow-grid");
        await grid.WaitForAsync(new LocatorWaitForOptions { Timeout = 8000 });

        var display = await grid.EvaluateAsync<string>(
            "el => window.getComputedStyle(el).display");
        display.Should().Be("grid", "the workflow container must use CSS Grid layout");
    }

    // ── nodes ───────────────────────────────────────────────────────────

    [Test]
    public async Task DiagramPage_RendersNineNodes()
    {
        var nodes = Page.Locator(".hedge-node");
        await nodes.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 8000 });
        (await nodes.CountAsync()).Should().Be(9, "the diagram must render exactly 9 workflow nodes");
    }

    [Test]
    public async Task DiagramPage_NodesHaveExpectedLabels()
    {
        string[] expectedTitles =
        [
            "BNY Data", "Record FX Data", "Validate Data",
            "Calculate Hedge Requirements", "Generate Trade Instructions", "Approvals",
            "Execute Trades", "Book Movements", "Reporting"
        ];

        var nodes = Page.Locator(".hedge-node-title");
        await nodes.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 8000 });

        for (int i = 0; i < expectedTitles.Length; i++)
        {
            (await nodes.Nth(i).InnerTextAsync()).Should().Be(expectedTitles[i],
                $"node {i} must have the expected title");
        }
    }

    [Test]
    public async Task DiagramPage_NodesHaveCategoryClasses()
    {
        var nodes = Page.Locator(".hedge-node");
        await nodes.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 8000 });

        // Steps 0-1: data-input, 2-4: processing, 5: approval, 6-8: output
        string[] expectedCategories =
        [
            "data-input", "data-input",
            "processing", "processing", "processing",
            "approval",
            "output", "output", "output"
        ];

        for (int i = 0; i < expectedCategories.Length; i++)
        {
            var classes = await nodes.Nth(i).GetAttributeAsync("class");
            classes.Should().Contain(expectedCategories[i],
                $"node {i} must have the '{expectedCategories[i]}' category class");
        }
    }

    [Test]
    public async Task DiagramPage_NodesHaveStepIndices()
    {
        var nodes = Page.Locator(".hedge-node");
        await nodes.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 8000 });

        for (int i = 0; i < 9; i++)
        {
            var dataStep = await nodes.Nth(i).GetAttributeAsync("data-step");
            dataStep.Should().Be(i.ToString(),
                $"node {i} must have data-step=\"{i}\"");
        }
    }

    [Test]
    public async Task DiagramPage_NodesHaveIcons()
    {
        var icons = Page.Locator(".hedge-node-icon");
        await icons.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 8000 });

        var count = await icons.CountAsync();
        count.Should().Be(9, "every node must have an icon element");

        for (int i = 0; i < count; i++)
        {
            var text = (await icons.Nth(i).InnerTextAsync()).Trim();
            text.Should().NotBeEmpty($"icon {i} must contain a Material Icon name");
        }
    }

    // ── sidebar ─────────────────────────────────────────────────────────

    [Test]
    public async Task DiagramPage_SidebarLinks_StillPresent()
    {
        (await Page.Locator(".nav-menu a[href='diagram']").IsVisibleAsync()).Should().BeTrue();
        (await Page.Locator(".nav-menu a[href='wizard']").IsVisibleAsync()).Should().BeTrue();
    }

    // ── Material Icons ────────────────────────────────────────────────

    [Test]
    public async Task DiagramPage_MaterialIcons_FontIsLoaded()
    {
        var icons = Page.Locator(".hedge-node-icon");
        await icons.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 8000 });

        var fontFamily = await icons.First.EvaluateAsync<string>(
            "el => window.getComputedStyle(el).fontFamily");
        fontFamily.Should().Contain("Material Icons",
            "icon elements must use the 'Material Icons' font-family");

        var width = await icons.First.EvaluateAsync<int>(
            "el => el.offsetWidth");
        width.Should().BeGreaterThan(0,
            "icon elements must have non-zero width (confirming the font rendered)");
    }

    // ── hover transition ─────────────────────────────────────────────

    [Test]
    public async Task DiagramPage_Nodes_HaveHoverTransition()
    {
        var nodes = Page.Locator(".hedge-node");
        await nodes.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 8000 });

        var transition = await nodes.First.EvaluateAsync<string>(
            "el => window.getComputedStyle(el).transition");
        transition.Should().Contain("transform",
            "hedge-node elements must have a transition that includes 'transform' for the hover elevation effect");
    }

    // ── category colors ───────────────────────────────────────────────

    [Test]
    public async Task DiagramPage_Nodes_HaveCategoryBackgroundColors()
    {
        var nodes = Page.Locator(".hedge-node");
        await nodes.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 8000 });

        // data-input: --category-data-input: #1565c0 = rgb(21, 101, 192) — node 0
        var dataInputBg = await nodes.Nth(0).EvaluateAsync<string>(
            "el => window.getComputedStyle(el).backgroundColor");
        dataInputBg.Should().Be("rgb(21, 101, 192)",
            "data-input nodes must have background #1565c0");

        // processing: --category-processing: #00796b = rgb(0, 121, 107) — node 2
        var processingBg = await nodes.Nth(2).EvaluateAsync<string>(
            "el => window.getComputedStyle(el).backgroundColor");
        processingBg.Should().Be("rgb(0, 121, 107)",
            "processing nodes must have background #00796b");

        // approval: --category-approval: #e65100 = rgb(230, 81, 0) — node 5
        var approvalBg = await nodes.Nth(5).EvaluateAsync<string>(
            "el => window.getComputedStyle(el).backgroundColor");
        approvalBg.Should().Be("rgb(230, 81, 0)",
            "approval nodes must have background #e65100");

        // output: --category-output: #2e7d32 = rgb(46, 125, 50) — node 6
        var outputBg = await nodes.Nth(6).EvaluateAsync<string>(
            "el => window.getComputedStyle(el).backgroundColor");
        outputBg.Should().Be("rgb(46, 125, 50)",
            "output nodes must have background #2e7d32");
    }
}
