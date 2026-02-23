using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Pemberton.Shareclass.Hedging.Prototype.UITests.Tests;

/// <summary>
/// E2E tests for wizard steps 6-8 (post-approval content).
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class WizardPostApprovalTests : AppPageTest
{
    private static readonly string[] ApproverNames =
    [
        "Fund Finance Approver Originator",
        "Fund Finance Approver",
        "PM Approver",
        "PAMSA Approver"
    ];

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

    /// <summary>
    /// Navigate from step 0 through approvals and approve all four, landing on step 6.
    /// </summary>
    private async Task NavigateToStep6()
    {
        // 0 → 1
        await ClickNextAndWaitFor(
            Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Record FX Data" }));
        // 1 → 2
        await ClickNextAndWaitFor(Page.GetByText("Validation:"));
        // 2 → 3
        await ClickNextAndWaitFor(Page.GetByText("hedge instructions"));
        // 3 → 4
        await ClickNextAndWaitFor(Page.GetByText("Trade instructions ready for approval"));
        // 4 → 5
        await ClickNextAndWaitFor(
            Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Approvals" }),
            timeoutMs: 8000);

        // Approve all four
        foreach (var name in ApproverNames)
        {
            var nameCell = Page.GetByText(name, new PageGetByTextOptions { Exact = true });
            var approveBtn = nameCell.Locator("xpath=ancestor::tr[1]")
                .Locator("button").Filter(new LocatorFilterOptions { HasText = "Approve" });
            await approveBtn.WaitForAsync(new LocatorWaitForOptions { Timeout = 3000 });
            await approveBtn.ClickAsync();
            await Expect(approveBtn).ToBeDisabledAsync(new() { Timeout = 3000 });
        }

        // 5 → 6
        var nextBtn = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Next" });
        await nextBtn.ClickAsync();

        // Wait for approval header to disappear (confirming we left step 5)
        var approvalsHeader = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Approvals" });
        await Expect(approvalsHeader).ToBeHiddenAsync(new() { Timeout = 5000 });
    }

    // ── step 6 ──────────────────────────────────────────────────────────

    [Test]
    public async Task Step6_ShowsExecutionContent()
    {
        await NavigateToStep6();

        // Step 6 renders a result-card (if ExecutionResult set) or empty-state (if null).
        // With no uploaded data, ExecutionResult is null, so check empty-state or result-card exists
        // and the approval heading is gone (confirming we advanced past step 5).
        var approvalsHeader = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Approvals" });
        await Expect(approvalsHeader).ToBeHiddenAsync(new() { Timeout = 5000 });

        var content = Page.Locator(".wizard-content").Locator(".empty-state, .result-card");
        (await content.CountAsync()).Should().BeGreaterThanOrEqualTo(1,
            "step 6 must render a result-card or empty-state element for execution content");
    }

    // ── step 7 ──────────────────────────────────────────────────────────

    [Test]
    public async Task Step7_ShowsReportingHeading()
    {
        await NavigateToStep6();

        var reportingHeader = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Reporting" });
        await ClickNextAndWaitFor(reportingHeader, timeoutMs: 8000);

        (await reportingHeader.IsVisibleAsync()).Should().BeTrue(
            "step 7 must show the Reporting heading");
    }

    [Test]
    public async Task Step7_ShowsReportingContent()
    {
        await NavigateToStep6();

        var reportingHeader = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Reporting" });
        await ClickNextAndWaitFor(reportingHeader, timeoutMs: 8000);

        // Either shows reporting data or "No reporting data loaded."
        var noDataMsg = Page.GetByText("No reporting data loaded");
        (await noDataMsg.IsVisibleAsync()).Should().BeTrue(
            "step 7 must show reporting content (data or fallback message)");
    }

    // ── step 8 ──────────────────────────────────────────────────────────

    [Test]
    public async Task Step8_ShowsMovementsBookedText()
    {
        await NavigateToStep6();

        // 6 → 7
        var reportingHeader = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Reporting" });
        await ClickNextAndWaitFor(reportingHeader, timeoutMs: 8000);

        // 7 → 8
        var movementsText = Page.GetByText("Movements booked");
        await ClickNextAndWaitFor(movementsText, timeoutMs: 8000);

        (await movementsText.IsVisibleAsync()).Should().BeTrue(
            "step 8 must show the 'Movements booked' text");
    }

    [Test]
    public async Task Step8_NextButton_IsDisabled()
    {
        await NavigateToStep6();

        // 6 → 7
        var reportingHeader = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Reporting" });
        await ClickNextAndWaitFor(reportingHeader, timeoutMs: 8000);

        // 7 → 8
        var movementsText = Page.GetByText("Movements booked");
        await ClickNextAndWaitFor(movementsText, timeoutMs: 8000);

        var nextBtn = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Next" });
        (await nextBtn.IsDisabledAsync()).Should().BeTrue(
            "Next must be disabled on the final step (step 8)");
    }
}
