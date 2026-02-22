using Pemberton.Shareclass.Hedging.Prototype.Models;

namespace Pemberton.Shareclass.Hedging.Prototype.ViewModels;

public class HedgingDiagramViewModel : BaseViewModel, IHedgingDiagramViewModel
{
    private readonly List<string> _nodes =
    [
        "BNY Data",
        "Record FX Data",
        "Validate Data",
        "Calculate Hedge Requirements",
        "Generate Trade Instructions",
        "Approvals",
        "Execute Trades",
        "Book Movements",
        "Reporting"
    ];

    private readonly List<WorkflowStep> _workflowSteps =
    [
        new() { StepIndex = 0, Title = "BNY Data",                     Category = "data-input",  Icon = "upload" },
        new() { StepIndex = 1, Title = "Record FX Data",               Category = "data-input",  Icon = "currency_exchange" },
        new() { StepIndex = 2, Title = "Validate Data",                Category = "processing",  Icon = "check_circle" },
        new() { StepIndex = 3, Title = "Calculate Hedge Requirements",  Category = "processing",  Icon = "calculate" },
        new() { StepIndex = 4, Title = "Generate Trade Instructions",   Category = "processing",  Icon = "description" },
        new() { StepIndex = 5, Title = "Approvals",                     Category = "approval",    Icon = "approval" },
        new() { StepIndex = 6, Title = "Execute Trades",                Category = "output",      Icon = "swap_horiz" },
        new() { StepIndex = 7, Title = "Book Movements",                Category = "output",      Icon = "book" },
        new() { StepIndex = 8, Title = "Reporting",                     Category = "output",      Icon = "bar_chart" },
    ];

    public IReadOnlyList<string> Nodes => _nodes;
    public IReadOnlyList<WorkflowStep> WorkflowSteps => _workflowSteps;
}
