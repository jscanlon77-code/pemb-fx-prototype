using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pemberton.Shareclass.Hedging.Prototype.Models;
using Pemberton.Shareclass.Hedging.Prototype.Services;
using Pemberton.Shareclass.Hedging.Prototype.ViewModels;

namespace Pemberton.Shareclass.Hedging.Prototype.Tests;

[TestFixture]
public class HedgingWizardViewModelTests
{
    [Test]
    public async Task ApproveAsync_ShouldLogApproval()
    {
        var engine = new Mock<IHedgingEngine>();
        var api = new Mock<IApiService>();
        var vm = new HedgingWizardViewModel(engine.Object, api.Object);

        await vm.ApproveAsync("PM Approver");

        api.Verify(a => a.LogApprovalAsync("PM Approver", "Approved", It.IsAny<DateTime>()), Times.Once);
    }

    [Test]
    public void Next_ShouldNotAdvance_WhenNotAllApproved()
    {
        var engine = new Mock<IHedgingEngine>();
        var api = new Mock<IApiService>();
        var vm = new HedgingWizardViewModel(engine.Object, api.Object);

        vm.Next(); // 0 -> 1
        vm.Step = 5; // force to approvals

        vm.Next();

        vm.Step.Should().Be(5);
    }
}
