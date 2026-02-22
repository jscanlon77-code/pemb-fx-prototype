using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

namespace Pemberton.Shareclass.Hedging.Prototype.UITests;

/// <summary>
/// NUnit assembly-level fixture that starts the Blazor Server application once
/// before any Playwright tests run and stops it afterwards.
///
/// If TEST_BASE_URL is already set in the environment (e.g. CI running against a
/// deployed instance, or a developer who started the app manually), the auto-start
/// is skipped and that URL is used as-is.
/// </summary>
[SetUpFixture]
public class GlobalSetup
{
    private Process? _appProcess;

    [OneTimeSetUp]
    public async Task StartApplicationAsync()
    {
        // Codespace environments export BROWSER pointing to a VSCode helper script.
        // Playwright reads that env var to select the browser — override it to chromium
        // whenever the value is not one of Playwright's recognised browser names.
        var validBrowsers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "chromium", "firefox", "webkit" };
        var browserEnv = Environment.GetEnvironmentVariable("BROWSER");
        if (browserEnv is not null && !validBrowsers.Contains(browserEnv))
            Environment.SetEnvironmentVariable("BROWSER", "chromium");

        if (Environment.GetEnvironmentVariable("TEST_BASE_URL") is not null)
            return;

        var port = FindFreePort();
        var serverUrl = $"http://127.0.0.1:{port}";
        var appDll = FindAppDll();

        _appProcess = new Process
        {
            StartInfo = new ProcessStartInfo("dotnet", $"\"{appDll}\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };
        _appProcess.StartInfo.EnvironmentVariables["ASPNETCORE_URLS"] = serverUrl;
        _appProcess.StartInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";
        _appProcess.Start();

        // Poll until the app responds (max 30 s)
        using var httpClient = new HttpClient();
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await httpClient.GetAsync(serverUrl);
                if ((int)response.StatusCode < 500)
                    break;
            }
            catch { /* not ready yet */ }
            await Task.Delay(250);
        }

        Environment.SetEnvironmentVariable("TEST_BASE_URL", serverUrl);
    }

    [OneTimeTearDown]
    public void StopApplication()
    {
        if (_appProcess is null) return;
        try { _appProcess.Kill(entireProcessTree: true); } catch { }
        _appProcess.Dispose();
    }

    /// <summary>
    /// Finds the main app's compiled DLL relative to this test assembly's location.
    /// Directory layout (from solution root):
    ///   src/MainApp/bin/{Config}/net8.0/MainApp.dll
    ///   tests/UITests/bin/{Config}/net8.0/UITests.dll   ← this assembly
    /// </summary>
    private static string FindAppDll()
    {
        var testDir = Path.GetDirectoryName(typeof(GlobalSetup).Assembly.Location)!;
        // testDir: .../tests/UITests/bin/{Config}/net8.0
        // Walk up 5 levels: net8.0 → {Config} → bin → UITests → tests → solution root
        var solutionRoot = testDir;
        for (int i = 0; i < 5; i++)
            solutionRoot = Path.GetDirectoryName(solutionRoot)!;

        var config = (testDir.Contains("/Release/") || testDir.Contains("\\Release\\"))
            ? "Release"
            : "Debug";

        var appDll = Path.GetFullPath(Path.Combine(
            solutionRoot,
            "src",
            "Pemberton.Shareclass.Hedging.Prototype",
            "bin",
            config,
            "net8.0",
            "Pemberton.Shareclass.Hedging.Prototype.dll"
        ));

        if (!File.Exists(appDll))
            throw new FileNotFoundException(
                $"Main app DLL not found at '{appDll}'. " +
                "Run 'dotnet build' on the solution before running E2E tests.", appDll);

        return appDll;
    }

    private static int FindFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
