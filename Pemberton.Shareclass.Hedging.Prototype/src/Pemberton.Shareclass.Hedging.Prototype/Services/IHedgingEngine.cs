using Pemberton.Shareclass.Hedging.Prototype.Models;

namespace Pemberton.Shareclass.Hedging.Prototype.Services;

public interface IHedgingEngine
{
    List<Exposure> LoadExposures();
    List<HedgeInstruction> Calculate(List<Exposure> exposures);
    string Execute(HedgeInstruction instruction);
}
