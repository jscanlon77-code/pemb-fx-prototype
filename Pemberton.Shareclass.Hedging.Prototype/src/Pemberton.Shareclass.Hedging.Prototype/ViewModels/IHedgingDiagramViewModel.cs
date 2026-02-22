using Pemberton.Shareclass.Hedging.Prototype.Models;

namespace Pemberton.Shareclass.Hedging.Prototype.ViewModels;

public interface IHedgingDiagramViewModel
{
    IReadOnlyList<string> Nodes { get; }
    IReadOnlyList<WorkflowStep> WorkflowSteps { get; }
}
