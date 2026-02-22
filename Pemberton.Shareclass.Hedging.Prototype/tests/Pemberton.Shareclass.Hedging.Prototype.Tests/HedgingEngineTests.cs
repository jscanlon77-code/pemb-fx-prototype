using FluentAssertions;
using NUnit.Framework;
using Pemberton.Shareclass.Hedging.Prototype.Models;
using Pemberton.Shareclass.Hedging.Prototype.Services;

namespace Pemberton.Shareclass.Hedging.Prototype.Tests;

[TestFixture]
public class HedgingEngineTests
{
    [Test]
    public void Calculate_ShouldReturnInstructionsForExposures()
    {
        var engine = new HedgingEngine();
        var exposures = new List<Exposure>
        {
            new() { ShareClassId = "SC01", ShareClassName = "Fund A", CurrencyPair = "EURUSD", Amount = 1_000_000m }
        };

        var result = engine.Calculate(exposures);

        result.Should().HaveCount(1);
        result[0].CurrencyPair.Should().Be("EURUSD");
    }
}
