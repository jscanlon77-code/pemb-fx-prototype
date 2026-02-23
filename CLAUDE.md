# Pemberton FX Hedging Prototype

## Project Overview

.NET 8 Blazor Server prototype for share-class FX hedging workflows.
Uses MVVM pattern with Radzen Blazor UI components.

## Solution Layout

```
Pemberton.Shareclass.Hedging.Prototype/
├── Pemberton.Shareclass.Hedging.Prototype.sln
├── src/Pemberton.Shareclass.Hedging.Prototype/   # Main Blazor Server app
│   ├── Models/          # Data classes (BnyData, Exposure, HedgeInstruction, ApproverStatus)
│   ├── ViewModels/      # MVVM ViewModels with INotifyPropertyChanged
│   ├── Services/        # Business logic (HedgingEngine) and API abstractions
│   ├── Pages/           # Razor pages (Index, HedgingWizard, HedgingDiagram)
│   ├── Components/      # Reusable Razor components
│   └── Shared/          # Layout and navigation
├── tests/Pemberton.Shareclass.Hedging.Prototype.Tests/     # Unit tests
└── tests/Pemberton.Shareclass.Hedging.Prototype.UITests/   # Playwright E2E tests
```

## Quality Commands

```bash
# Build (must pass before any commit)
dotnet build Pemberton.Shareclass.Hedging.Prototype/Pemberton.Shareclass.Hedging.Prototype.sln

# Unit tests
dotnet test Pemberton.Shareclass.Hedging.Prototype/tests/Pemberton.Shareclass.Hedging.Prototype.Tests/

# E2E tests (requires app running or uses GlobalSetup auto-start)
dotnet test Pemberton.Shareclass.Hedging.Prototype/tests/Pemberton.Shareclass.Hedging.Prototype.UITests/
```

## Coding Conventions

- **Architecture:** MVVM — ViewModels inherit `BaseViewModel` (INotifyPropertyChanged), injected into Pages via DI
- **UI Components:** Radzen Blazor (RadzenSteps, RadzenDataGrid, RadzenButton)
- **DI Registration:** Services as Transient in `Program.cs`; Radzen services as Scoped
- **Testing:** NUnit 3 + Moq + FluentAssertions for unit tests; Playwright.NUnit for E2E
- **Naming:** PascalCase for public members, camelCase for locals, interfaces prefixed with `I`
- **Async:** Methods returning Task use `Async` suffix; beware `async void` in `ExecuteStep`

## Wizard Steps (0–8)

0. BNY Data upload → 1. FX Data upload → 2. Validation → 3. Calculate hedge →
4. Trade instructions → 5. Approvals (blocks until all 4 approve) → 6. Execute trades →
7. Book movements → 8. Reporting
