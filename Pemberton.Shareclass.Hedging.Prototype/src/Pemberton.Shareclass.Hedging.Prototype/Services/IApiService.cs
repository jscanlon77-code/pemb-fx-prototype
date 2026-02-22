using Pemberton.Shareclass.Hedging.Prototype.Models;

namespace Pemberton.Shareclass.Hedging.Prototype.Services;

public interface IApiService
{
    Task<bool> SaveBnyDataAsync(IEnumerable<BnyData> data);
    Task<bool> SaveFxDataAsync(IEnumerable<Exposure> data);
    Task<bool> SaveValidationAsync(bool isValid);
    Task<bool> SaveCalculationAsync(IEnumerable<HedgeInstruction> instructions);
    Task<bool> SaveTradeInstructionsAsync(IEnumerable<HedgeInstruction> instructions);
    Task<bool> LogApprovalAsync(string approverName, string status, DateTime timestamp);
    Task<IEnumerable<object>> GetReportingDataAsync();
    Task<bool> BookMovementsAsync(IEnumerable<HedgeInstruction> instructions);
}
