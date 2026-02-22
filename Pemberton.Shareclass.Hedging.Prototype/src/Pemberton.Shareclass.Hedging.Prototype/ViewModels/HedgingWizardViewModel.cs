using System.Text;
using Pemberton.Shareclass.Hedging.Prototype.Models;
using Pemberton.Shareclass.Hedging.Prototype.Services;

namespace Pemberton.Shareclass.Hedging.Prototype.ViewModels;

public class HedgingWizardViewModel : BaseViewModel, IHedgingWizardViewModel
{
    private readonly IHedgingEngine _engine;
    private readonly IApiService _api;

    private int _step;
    private List<HedgeInstruction>? _instructions;
    private string? _executionResult;
    private List<IDictionary<string, object>> _pivotedFxData = new();
    private List<IDictionary<string, object>> _pivotedBnyData = new();
    private IEnumerable<object>? _reportingData;

    private readonly List<BnyData> _bnyData = new();
    private readonly List<Exposure> _fxData = new();

    private readonly List<ApproverStatus> _approvals =
    [
        new() { ApproverName = "Fund Finance Approver Originator" },
        new() { ApproverName = "Fund Finance Approver" },
        new() { ApproverName = "PM Approver" },
        new() { ApproverName = "PAMSA Approver" }
    ];

    public HedgingWizardViewModel(IHedgingEngine engine, IApiService api)
    {
        _engine = engine;
        _api = api;
        ExecuteStep();
    }

    public int Step
    {
        get => _step;
        set => SetProperty(ref _step, value);
    }

    public List<BnyData> BnyData => _bnyData;
    public List<Exposure> FxData => _fxData;

    public List<HedgeInstruction>? Instructions
    {
        get => _instructions;
        private set => SetProperty(ref _instructions, value);
    }

    public string? ExecutionResult
    {
        get => _executionResult;
        private set => SetProperty(ref _executionResult, value);
    }

    public IEnumerable<IDictionary<string, object>> PivotedFxData => _pivotedFxData;
    public IEnumerable<IDictionary<string, object>> PivotedBnyData => _pivotedBnyData;

    public IList<ApproverStatus> Approvals => _approvals;

    public bool AllApproved => _approvals.All(a => a.Status == "Approved");

    public IEnumerable<object>? ReportingData
    {
        get => _reportingData;
        private set => SetProperty(ref _reportingData, value);
    }

    public bool IsFirst => Step == 0;
    public bool IsLast => Step == 8;

    public void Next()
    {
        if (Step == 5 && !AllApproved)
            return;

        if (!IsLast)
        {
            Step++;
            ExecuteStep();
        }
    }

    public void Back()
    {
        if (!IsFirst)
        {
            Step--;
            ExecuteStep();
        }
    }

    private async void ExecuteStep()
    {
        switch (Step)
        {
            case 0:
                if (BnyData.Any())
                    await _api.SaveBnyDataAsync(BnyData);
                break;
            case 1:
                if (FxData.Any())
                    await _api.SaveFxDataAsync(FxData);
                break;
            case 2:
                var isValid = FxData.Any() && BnyData.Any();
                await _api.SaveValidationAsync(isValid);
                break;
            case 3:
                Instructions = _engine.Calculate(FxData);
                if (Instructions != null)
                    await _api.SaveCalculationAsync(Instructions);
                break;
            case 4:
                if (Instructions != null)
                    await _api.SaveTradeInstructionsAsync(Instructions);
                break;
            case 6:
                if (Instructions != null && Instructions.Any())
                {
                    ExecutionResult = _engine.Execute(Instructions.First());
                    await _api.BookMovementsAsync(Instructions);
                }
                break;
            case 7:
                ReportingData = await _api.GetReportingDataAsync();
                break;
        }
    }

    public async Task LoadBnyCsvAsync(Stream fileStream)
    {
        _bnyData.Clear();

        using var reader = new StreamReader(fileStream, Encoding.UTF8);
        string? line;
        bool headerSkipped = false;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (!headerSkipped)
            {
                headerSkipped = true;
                continue;
            }

            var parts = line.Split(',', StringSplitOptions.TrimEntries);

            if (parts.Length < 3)
                continue;

            _bnyData.Add(new BnyData
            {
                ShareClassId = parts[0],
                ShareClassName = parts[1],
                Exposure = decimal.Parse(parts[2])
            });
        }

        PivotBnyData();
        OnPropertyChanged(nameof(BnyData));
    }

    public async Task LoadFxCsvAsync(Stream fileStream)
    {
        _fxData.Clear();

        using var reader = new StreamReader(fileStream, Encoding.UTF8);
        string? line;
        bool headerSkipped = false;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (!headerSkipped)
            {
                headerSkipped = true;
                continue;
            }

            var parts = line.Split(',', StringSplitOptions.TrimEntries);

            if (parts.Length < 4)
                continue;

            _fxData.Add(new Exposure
            {
                ShareClassId = parts[0],
                ShareClassName = parts[1],
                CurrencyPair = parts[2],
                Amount = decimal.Parse(parts[3])
            });
        }

        PivotFxData();
        OnPropertyChanged(nameof(FxData));
    }

    private void PivotFxData()
    {
        var row = new Dictionary<string, object>();

        foreach (var item in _fxData)
        {
            string col = $"{item.ShareClassName} ({item.CurrencyPair})";

            if (!row.ContainsKey(col))
                row[col] = 0m;

            row[col] = (decimal)row[col] + item.Amount;
        }

        _pivotedFxData = new List<IDictionary<string, object>> { row };
        OnPropertyChanged(nameof(PivotedFxData));
    }

    private void PivotBnyData()
    {
        var row = new Dictionary<string, object>();

        foreach (var item in _bnyData)
        {
            string col = $"{item.ShareClassName} (Exposure)";

            if (!row.ContainsKey(col))
                row[col] = 0m;

            row[col] = (decimal)row[col] + item.Exposure;
        }

        _pivotedBnyData = new List<IDictionary<string, object>> { row };
        OnPropertyChanged(nameof(PivotedBnyData));
    }

    public async Task ApproveAsync(string approverName)
    {
        var approver = _approvals.First(a => a.ApproverName == approverName);

        await Task.Delay(500);

        approver.Status = "Approved";

        await _api.LogApprovalAsync(approverName, "Approved", DateTime.UtcNow);

        OnPropertyChanged(nameof(Approvals));
        OnPropertyChanged(nameof(AllApproved));
    }

    public async Task RejectAsync(string approverName)
    {
        var approver = _approvals.First(a => a.ApproverName == approverName);

        await Task.Delay(500);

        approver.Status = "Rejected";

        await _api.LogApprovalAsync(approverName, "Rejected", DateTime.UtcNow);

        OnPropertyChanged(nameof(Approvals));
        OnPropertyChanged(nameof(AllApproved));
    }
}
