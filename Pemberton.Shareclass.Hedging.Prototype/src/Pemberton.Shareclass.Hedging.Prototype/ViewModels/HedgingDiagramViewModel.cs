namespace Pemberton.Shareclass.Hedging.Prototype.ViewModels;

public class HedgingDiagramViewModel : BaseViewModel, IHedgingDiagramViewModel
{
    private readonly List<string> _nodes =
    [
        ""BNY Data"",
        ""Record FX Data"",
        ""Validate Data"",
        ""Calculate Hedge Requirements"",
        ""Generate Trade Instructions"",
        ""Approvals"",
        ""Execute Trades"",
        ""Book Movements"",
        ""Reporting""
    ];

    public IReadOnlyList<string> Nodes => _nodes;
}
