using Pemberton.Shareclass.Hedging.Prototype.Models;

namespace Pemberton.Shareclass.Hedging.Prototype.Services;

public class MockApiService : IApiService
{
    public async Task<bool> SaveBnyDataAsync(IEnumerable<BnyData> data)
    {
        await Task.Delay(300);
        return true;
    }

    public async Task<bool> SaveFxDataAsync(IEnumerable<Exposure> data)
    {
        await Task.Delay(300);
        return true;
    }

    public async Task<bool> SaveValidationAsync(bool isValid)
    {
        await Task.Delay(300);
        return true;
    }

    public async Task<bool> SaveCalculationAsync(IEnumerable<HedgeInstruction> instructions)
    {
        await Task.Delay(300);
        return true;
    }

    public async Task<bool> SaveTradeInstructionsAsync(IEnumerable<HedgeInstruction> instructions)
    {
        await Task.Delay(300);
        return true;
    }

    public async Task<bool> LogApprovalAsync(string approverName, string status, DateTime timestamp)
    {
        await Task.Delay(300);
        return true;
    }

    public async Task<IEnumerable<object>> GetReportingDataAsync()
    {
        await Task.Delay(300);
        return new List<object>
        {
            new { Report = ""Mock reporting data"", Created = DateTime.UtcNow }
        };
    }

    public async Task<bool> BookMovementsAsync(IEnumerable<HedgeInstruction> instructions)
    {
        await Task.Delay(300);
        return true;
    }
}
