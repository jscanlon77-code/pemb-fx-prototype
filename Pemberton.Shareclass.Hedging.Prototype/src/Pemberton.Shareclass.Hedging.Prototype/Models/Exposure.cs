namespace Pemberton.Shareclass.Hedging.Prototype.Models;

public class Exposure
{
    public string ShareClassId { get; set; } = string.Empty;
    public string ShareClassName { get; set; } = string.Empty;
    public string CurrencyPair { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
