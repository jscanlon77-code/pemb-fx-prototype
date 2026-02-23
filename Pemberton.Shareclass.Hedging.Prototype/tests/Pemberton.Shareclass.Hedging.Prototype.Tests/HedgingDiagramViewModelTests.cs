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

        vm.Nodes.Should().Contain("Approvals");
        vm.Nodes.Should().Contain("Reporting");
    }

    [Test]
    public void WorkflowSteps_ShouldContainNineSteps()
    {
        var vm = new HedgingDiagramViewModel();

        vm.WorkflowSteps.Should().HaveCount(9);
    }

    [Test]
    public void WorkflowSteps_ShouldHaveExpectedCategories()
    {
        var vm = new HedgingDiagramViewModel();

        vm.WorkflowSteps[0].Category.Should().Be("data-input");
        vm.WorkflowSteps[1].Category.Should().Be("data-input");
        vm.WorkflowSteps[2].Category.Should().Be("processing");
        vm.WorkflowSteps[3].Category.Should().Be("processing");
        vm.WorkflowSteps[4].Category.Should().Be("processing");
        vm.WorkflowSteps[5].Category.Should().Be("approval");
        vm.WorkflowSteps[6].Category.Should().Be("output");
        vm.WorkflowSteps[7].Category.Should().Be("output");
        vm.WorkflowSteps[8].Category.Should().Be("output");
    }

    [Test]
    public void WorkflowSteps_ShouldHaveSequentialStepIndices()
    {
        var vm = new HedgingDiagramViewModel();

        for (int i = 0; i < 9; i++)
        {
            vm.WorkflowSteps[i].StepIndex.Should().Be(i);
        }
    }

    [Test]
    public void WorkflowSteps_ShouldHaveNonEmptyIcons()
    {
        var vm = new HedgingDiagramViewModel();

        foreach (var step in vm.WorkflowSteps)
        {
            step.Icon.Should().NotBeNullOrEmpty(
                $"step {step.StepIndex} ({step.Title}) must have an icon");
        }
    }
}
