namespace Pemberton.Shareclass.Hedging.Prototype.Models;

public class HedgeInstruction
{
    public string Type { get; set; } = string.Empty;
    public string CurrencyPair { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Maturity { get; set; }
}
