namespace Pemberton.Shareclass.Hedging.Prototype.Models;

public class BnyData
{
    public string ShareClassId { get; set; } = string.Empty;
    public string ShareClassName { get; set; } = string.Empty;
    public decimal Exposure { get; set; }
}
