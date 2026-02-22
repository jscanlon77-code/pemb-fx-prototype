using FluentAssertions;
using NUnit.Framework;
using Pemberton.Shareclass.Hedging.Prototype.ViewModels;

namespace Pemberton.Shareclass.Hedging.Prototype.Tests;

[TestFixture]
public class HedgingDiagramViewModelTests
{
    [Test]
    public void Nodes_ShouldContainApprovalsAndReporting()
    {
        var vm = new HedgingDiagramViewModel();

        vm.Nodes.Should().Contain(""Approvals"");
        vm.Nodes.Should().Contain(""Reporting"");
    }
}
