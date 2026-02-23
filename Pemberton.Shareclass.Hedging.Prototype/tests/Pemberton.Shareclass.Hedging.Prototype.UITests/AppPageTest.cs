using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Pemberton.Shareclass.Hedging.Prototype.UITests;

/// <summary>
/// Base class for all Playwright page tests.
/// Reads the app base URL from the TEST_BASE_URL environment variable,
/// defaulting to http://localhost:5050.
/// </summary>
public abstract class AppPageTest : PageTest
{
    protected static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "http://localhost:5050";

    public override BrowserNewContextOptions ContextOptions() =>
        new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
            IgnoreHTTPSErrors = true
        };
}
