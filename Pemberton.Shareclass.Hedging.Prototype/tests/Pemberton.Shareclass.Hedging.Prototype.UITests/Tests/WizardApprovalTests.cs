using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Pemberton.Shareclass.Hedging.Prototype.UITests.Tests;

/// <summary>
/// End-to-end tests for the approval workflow (step 5) and the full wizard run-through.
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class WizardApprovalTests : AppPageTest
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

    // ── navigation helpers ────────────────────────────────────────────────────

    private async Task ClickNextAndWaitFor(ILocator awaitLocator, int timeoutMs = 5000)
    {
        var nextBtn = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Next" });
        await nextBtn.ClickAsync();
        await awaitLocator.WaitForAsync(new LocatorWaitForOptions { Timeout = timeoutMs });
    }

    /// <summary>
    /// Navigates from step 0 to step 5 (Approvals) by clicking Next four times,
    /// waiting for each step's content before advancing.
    /// </summary>
    private async Task NavigateToApprovalStep()
    {
        // 0 → 1
        var step1 = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Record FX Data" });
        await ClickNextAndWaitFor(step1);

        // 1 → 2
        var step2 = Page.GetByText("Validation:");
        await ClickNextAndWaitFor(step2);

        // 2 → 3
        var step3 = Page.GetByText("hedge instructions");
        await ClickNextAndWaitFor(step3);

        // 3 → 4
        var step4 = Page.GetByText("Trade instructions ready for approval");
        await ClickNextAndWaitFor(step4);

        // 4 → 5
        var step5 = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Approvals" });
        await ClickNextAndWaitFor(step5, timeoutMs: 8000);
    }

    // ── approval step reachability ────────────────────────────────────────────────

    [Test]
    public async Task ApprovalStep_IsReachable_AfterNavigatingThroughPriorSteps()
    {
        await NavigateToApprovalStep();

        var approvalsHeader = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Approvals" });
        (await approvalsHeader.IsVisibleAsync()).Should().BeTrue(
            "step 5 must show the Approvals heading");
    }

    // ── approval grid ─────────────────────────────────────────────────────────

    [Test]
    public async Task ApprovalStep_ShowsAllFourApprovers()
    {
        await NavigateToApprovalStep();

        foreach (var name in ApproverNames)
        {
            // Use exact match so "Fund Finance Approver" doesn't also match
            // the "Fund Finance Approver Originator" row.
            (await Page.GetByText(name, new PageGetByTextOptions { Exact = true }).IsVisibleAsync())
                .Should().BeTrue($"approver '{name}' must appear in the approval grid");
        }
    }

    [Test]
    public async Task ApprovalStep_EachApprover_InitiallyHasPendingStatus()
    {
        await NavigateToApprovalStep();

        // There should be exactly 4 "Pending" status cells visible
        var pendingCells = Page.GetByText("Pending");
        var count = await pendingCells.CountAsync();
        count.Should().Be(4, "all four approvers must start with Pending status");
    }

    // ── blocked advancement ─────────────────────────────────────────────────────────

    [Test]
    public async Task ApprovalStep_ClickingNext_WithoutApprovals_StaysOnApprovalStep()
    {
        await NavigateToApprovalStep();

        // Clicking Next without any approvals must not advance the wizard.
        // Vm.Next() returns early when Step == 5 && !AllApproved.
        var nextBtn = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Next" });
        await nextBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(500); // give Blazor time to re-render

        var approvalsHeader = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Approvals" });
        (await approvalsHeader.IsVisibleAsync()).Should().BeTrue(
            "wizard must remain on step 5 when not all approvals are in place");
    }

    [Test]
    public async Task ApprovalStep_RequiredMessage_IsDisplayed_WhenNotAllApproved()
    {
        await NavigateToApprovalStep();

        var message = Page.GetByText("All four approvals are required before continuing");
        (await message.IsVisibleAsync()).Should().BeTrue(
            "a warning message must be visible while approvals are outstanding");
    }

    // ── full approval and advancement ─────────────────────────────────────────────────

    [Test]
    public async Task ApprovalStep_ApprovingAllFour_AllowsAdvancement()
    {
        await NavigateToApprovalStep();

        // Approve each approver in grid order (0-3).  Using Nth(i) avoids the
        // partial-text ambiguity where "Fund Finance Approver" matches both that
        // row and the "Fund Finance Approver Originator" row.
        // Each ApproveAsync has a 500 ms internal delay, so we wait 700 ms per click.
        var allApproveBtns = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Approve" });
        for (int i = 0; i < ApproverNames.Length; i++)
        {
            var approveBtn = allApproveBtns.Nth(i);
            await approveBtn.WaitForAsync(new LocatorWaitForOptions { Timeout = 3000 });
            await approveBtn.ClickAsync();
            await Page.WaitForTimeoutAsync(700);
        }

        // After all four approvals the required-message must disappear ...
        var message = Page.GetByText("All four approvals are required before continuing");
        (await message.IsHiddenAsync()).Should().BeTrue(
            "the warning message must disappear once all approvals are received");

        // ... and clicking Next must advance to step 6 (the Approvals h4 disappears).
        var nextBtn = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Next" });
        await nextBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        var approvalsHeader = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Approvals" });
        (await approvalsHeader.IsHiddenAsync()).Should().BeTrue(
            "wizard must advance past step 5 once all approvals have been granted");
    }

    // ── last step ─────────────────────────────────────────────────────────────

    [Test]
    public async Task LastStep_NextButtonIsDisabled()
    {
        // Navigate all the way to step 8 (IsLast == true).
        await NavigateToApprovalStep();

        // Approve all four
        foreach (var name in ApproverNames)
        {
            // Locate the exact approver name cell, then walk up to its <tr> to find
            // the Approve button — avoids partial-match issues with "Fund Finance Approver"
            // vs "Fund Finance Approver Originator".
            var nameCell = Page.GetByText(name, new PageGetByTextOptions { Exact = true });
            var approveBtn = nameCell.Locator("xpath=ancestor::tr[1]")
                .Locator("button").Filter(new LocatorFilterOptions { HasText = "Approve" });
            await approveBtn.WaitForAsync(new LocatorWaitForOptions { Timeout = 3000 });
            await approveBtn.ClickAsync();
            await Page.WaitForTimeoutAsync(700);
        }

        // 5 → 6
        var nextBtn = Page.Locator("button").Filter(new LocatorFilterOptions { HasText = "Next" });
        await nextBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(1000); // step 6 fires ExecuteStep (async void BookMovements)

        // 6 → 7 (Reporting h4)
        var step7 = Page.Locator("h4").Filter(new LocatorFilterOptions { HasText = "Reporting" });
        await ClickNextAndWaitFor(step7, timeoutMs: 8000);

        // 7 → 8 ("Movements booked" text)
        var step8 = Page.GetByText("Movements booked");
        await ClickNextAndWaitFor(step8, timeoutMs: 8000);

        // On the last step Next must be disabled (IsLast == true → Disabled="true")
        (await nextBtn.IsDisabledAsync()).Should().BeTrue(
            "Next must be disabled on the final step (step 8)");
    }
}
