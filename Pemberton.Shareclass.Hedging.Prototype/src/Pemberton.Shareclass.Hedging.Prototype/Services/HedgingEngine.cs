using Pemberton.Shareclass.Hedging.Prototype.Models;

namespace Pemberton.Shareclass.Hedging.Prototype.Services;

public class HedgingEngine : IHedgingEngine
{
    public List<Exposure> LoadExposures() =>
        new()
        {
            new Exposure
            {
                ShareClassId = "SC01",
                ShareClassName = "Fund A",
                CurrencyPair = "EURUSD",
                Amount = 1_000_000m
            }
        };

    public List<HedgeInstruction> Calculate(List<Exposure> exposures) =>
        exposures.Select(e => new HedgeInstruction
        {
            Type = "Forward",
            CurrencyPair = e.CurrencyPair,
            Amount = e.Amount,
            Maturity = DateTime.Today.AddMonths(3)
        }).ToList();

    public string Execute(HedgeInstruction instruction) =>
        $"Executed {instruction.Type} {instruction.CurrencyPair} {instruction.Amount:N0} maturing {instruction.Maturity:yyyy-MM-dd}";
}
