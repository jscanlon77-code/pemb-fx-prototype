# Agent Learnings & Conventions

Living document updated by Ralph and interactive sessions with discovered patterns.

## Architecture

- **Pattern:** MVVM with dependency injection
- **ViewModels** inherit `BaseViewModel` which provides `SetProperty<T>()` for change notification
- **Services** are behind interfaces (`IHedgingEngine`, `IApiService`) registered as Transient
- **Pages** inject ViewModels via `@inject IHedgingWizardViewModel VM`
- **MockApiService** implements `IApiService` with no-ops for prototyping

## Key File Paths

| Concern | Path |
|---------|------|
| Solution | `Pemberton.Shareclass.Hedging.Prototype/Pemberton.Shareclass.Hedging.Prototype.sln` |
| Main app | `Pemberton.Shareclass.Hedging.Prototype/src/Pemberton.Shareclass.Hedging.Prototype/` |
| Program.cs (DI) | `…/src/Pemberton.Shareclass.Hedging.Prototype/Program.cs` |
| Wizard page | `…/src/Pemberton.Shareclass.Hedging.Prototype/Pages/HedgingWizard.razor` |
| Wizard VM | `…/src/Pemberton.Shareclass.Hedging.Prototype/ViewModels/HedgingWizardViewModel.cs` |
| Unit tests | `…/tests/Pemberton.Shareclass.Hedging.Prototype.Tests/` |
| UI tests | `…/tests/Pemberton.Shareclass.Hedging.Prototype.UITests/` |

## Wizard Step Mapping

| Step | Name | ExecuteStep Action |
|------|------|--------------------|
| 0 | BNY Data | Save BNY data to API |
| 1 | FX Data | Save FX data to API |
| 2 | Validation | Validate both datasets |
| 3 | Calculation | Calculate hedge via IHedgingEngine |
| 4 | Trade Instructions | Save trade instructions |
| 5 | Approvals | View only — blocks Next until AllApproved |
| 6 | Execution | Execute trades |
| 7 | Booking | Fetch reporting data |
| 8 | Reporting | Book movements |

## Testing Patterns

### Unit Tests (NUnit + Moq + FluentAssertions)
- Mock services via `new Mock<IApiService>()` and inject into ViewModel constructors
- Assert with FluentAssertions: `.Should().Be()`, `.Should().HaveCount()`
- Test ViewModel state transitions (e.g., `Next()` blocked at step 5)

### E2E Tests (Playwright.NUnit)
- Inherit from `AppPageTest` (extends Playwright `PageTest`)
- `GlobalSetup` auto-starts the Blazor app on a free port
- Use `Page.GotoAsync()` then assert with Playwright locators
- Pattern: `ClickNextAndWaitFor()` for async Blazor re-renders

## Known Gotchas

- **`async void ExecuteStep()`**: Called from `Next()`/`Back()` — exceptions won't propagate. Wrap in try-catch when modifying.
- **RadzenSteps navigation**: Step index is 0-based. The `Change` event fires the step index.
- **RadzenDataGrid dynamic columns**: When data shape changes (e.g., pivoted columns), the grid needs `@key` or manual refresh.
- **Blazor re-render timing**: After `StateHasChanged()`, DOM updates are async. E2E tests need explicit waits.
- **Approval gate**: `Next()` at step 5 checks `AllApproved` — all 4 approvers must have Status != "Pending".
- **CSV parsing**: `LoadBnyCsvAsync()` and `LoadFxCsvAsync()` expect specific column headers.
