using Pemberton.Shareclass.Hedging.Prototype.Models;
using Pemberton.Shareclass.Hedging.Prototype.Services;

namespace Pemberton.Shareclass.Hedging.Prototype.ViewModels;

public interface IHedgingWizardViewModel
{
    int Step { get; }
    List<BnyData> BnyData { get; }
    List<Exposure> FxData { get; }
    List<HedgeInstruction>? Instructions { get; }
    string? ExecutionResult { get; }
    IEnumerable<IDictionary<string, object>> PivotedFxData { get; }
    IEnumerable<IDictionary<string, object>> PivotedBnyData { get; }
    IList<ApproverStatus> Approvals { get; }
    bool AllApproved { get; }
    IEnumerable<object>? ReportingData { get; }

    Task LoadBnyCsvAsync(Stream fileStream);
    Task LoadFxCsvAsync(Stream fileStream);

    void Next();
    void Back();
    bool IsFirst { get; }
    bool IsLast { get; }

    Task ApproveAsync(string approverName);
    Task RejectAsync(string approverName);
}
