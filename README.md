# AeroGrid B2B Renewable Digital Twin & IoT Micro-Grid Simulator
> An enterprise-grade, high-fidelity **IoT-Sim: Virtual Energy Grid** and **Smart Energy Grid Manager** web application built natively in .NET 10.0 using modern Object-Oriented Design patterns, decoupled data structures, and persistent database storage.

---

## 🏗️ Architectural Overview
AeroGrid is engineered using a strictly decoupled, asynchronous multi-project architecture designed to simulate and manage live industrial green-energy asset distributions for corporate facilities.

```text
                    [ Physical Simulator Engine ] 
                         (AeroGrid.Simulator)
                                  │
                       Committed Data Transfers
                                  ▼
                     [ SQLite Persistent Ledger ]
                           (aerogrid.db)
                                  │
                         Entity Framework Core
                                  ▼
                   [ Executive Blazor Web Portal ]
                       (AeroGrid.WebDashboard)
```

### 🔬 Core Project Infrastructure
1. **`AeroGrid.Simulator` (Backend Orchestration Engine)**: A high-performance console application utilizing multi-layered physics modeling (including trigonometric daylight trajectory scaling) to simulate historical grid outputs based on user atmospheric choices.
2. **`AeroGrid.WebDashboard` (Smart Energy Grid Manager Portal)**: A premium, cybernetic dark-themed Blazor Server Web UI structured on a decoupled **Three-File Code-Behind Pattern** to completely separate presentation logic from data processing rules with 0% JavaScript overhead.

---

## ⚡ Key Engineering Implementation Metrics

### 🧬 Object-Oriented Architecture & Polymorphism
Instead of using fragile, rigid scripting patterns, all virtual energy options are modeled polymorphically. Child classes (`SolarInverter`, `WindTurbine`) inherit calculations from a single abstract base blueprint (`EnergyAsset`). This architecture ensures compliance with the **Open-Closed Principle**, allowing developers to hook up new green assets (like Hydro-Electric Dams) seamlessly without modifying the core simulation running loop.

### 📊 Data Structures & Persistence (DSA)
- **Hourly Logging Ledger**: Every single operational calculation is bundled inside a strongly typed `GridLog` snapshot model packet and committed into an in-memory `List<GridLog>` tracking data structure.
- **SQLite Database Architecture**: Memory collections are systematically extracted and serialized down into permanent rows using **Entity Framework Core (EF Core)** within an automated resource management `using` loop, keeping local RAM overhead to near-zero bytes.
- **Business Intelligence Analytics**: Implements native **LINQ** query optimization loops to perform linear lookups and aggregate filters to identify peak operational outputs and dependency durations.

---

## 🛠️ Technology Stack Matrix
- **Framework**: Native .NET 10.0 SDK Core Runtime
- **Frontend Architecture**: Blazor Server Component Workspace (C# Razor Views)
- **Design System Framework**: Integrated Bootstrap 5 Corporate Layout Utilities
- **Database Engine**: Microsoft Entity Framework Core with SQLite adapters
- **IDE Tools**: Visual Studio Code, Git/GitHub Monorepo Management

---

## 📋 Sprint 6 Changelog — Weather API Integration (Completed)

Sprint 6 is now complete. Full breakdown of every change, organized against the original task list:

### 🚨 Critical Fix — Interactivity was never actually enabled (found during user testing)
**Symptom:** typing a new city and clicking "Sync Live Grid" had no effect whatsoever — not even an error, just nothing. Toggling an asset switch would not have worked either, for the identical reason.
**Root cause:** `Program.cs` registers interactive server components (`AddInteractiveServerComponents()` / `.AddInteractiveServerRenderMode()`), but nothing in the component tree ever actually *requested* that render mode. `Components/App.razor` rendered `<Routes />` with no `@rendermode` attribute, so every page — including `/gridmanager` — was pure static server-rendered HTML. `OnInitializedAsync` still runs once during that static render (which is why the very first page load correctly showed real weather for the default city), but without a live interactive circuit, two-way data binding and click handlers have nothing to talk to: typing into the city box and clicking the sync button were both silently inert.
This was a pre-existing gap in the original project — `App.razor`/`Routes.razor` were not touched by any of the other Sprint 6 changes — not something introduced along the way. It also means the asset toggle switches from Bug Fix 2 would not have worked either, independent of whether the underlying calculation logic was correct.
**Fix:** Added `@rendermode="InteractiveServer"` to `<Routes />` in `App.razor`, matching the standard "global server interactivity" pattern the project's own `Program.cs` was already otherwise set up for. Every page is now genuinely interactive, so both the weather search and the asset toggles actually reach the server.

### 🐞 Bug Fix 1 — Weather UI stuck on Faisalabad
**Root cause:** `WeatherApiService` was calling OpenWeatherMap with a hardcoded placeholder API key that was never a real, active credential. Every request came back `401 Unauthorized`, so the UI silently fell back to the same canned values no matter what city was typed in.
**Fix:** Rebuilt on **Open-Meteo** (see API Task below). Any city now resolves to real coordinates and real current conditions, with no API key required at all — so there's no credential to expire or misconfigure.

### 🐞 Bug Fix 2 — Asset toggle logic
**Root cause:** Turning an asset off *did* zero out a multiplier, but the generation number it was zeroing was only an approximation — a fixed 60/40 (solar/wind) split applied to one pre-baked historical total, not real per-asset output. That split is also hardcoded to exactly 2 assets, which conflicts directly with "the code must be flexible and not hardcoded for just 2 assets."
**Fix:** The dashboard now calls each asset's own real `SimulateOutput(hour, weather)` — the same polymorphic method `AeroGrid.Simulator/Program.cs` already uses — and sums only the assets currently switched on. `IsSolarActive`/`IsWindActive` were replaced with a single `List<EnergyAsset> Assets` (`GridManagerBase.cs`), and the toggle UI (`GridManager.razor`) now renders one row per item in that list via `@foreach` instead of two copy-pasted blocks.
**Adding a future asset:** add one line to the `Assets` list in `GridManagerBase.cs` (e.g. `new HydroDam(40.0)`), and — if it's a genuinely new type — one line to the icon `switch` in `GetAssetIconMarkup`. The toggle switch, the "assets online" count, and the generation math all pick it up automatically; nothing else needs to change.

### 🔌 API Task — Live weather integration
Integrated **Open-Meteo** (`open-meteo.com`), the provider this README already named for Sprint 6. Two free, keyless calls:
1. **Geocoding API** — free-text city name → latitude/longitude (any city: London, Lahore, Paris, Faisalabad, Tokyo, ...).
2. **Forecast API** — coordinates → real-time temperature, cloud cover, wind speed, and WMO weather code.

The WMO code is mapped to one of the exact weather words `SolarInverter`/`WindTurbine` already understand ("Sunny", "Cloudy", "Rainy", "Stormy", "Snowy", "Windy", "Foggy"). The same string drives both what's displayed on screen and what's fed into the simulation, so the numbers always match the condition shown next to them. A live **Temperature** reading was also added, since Open-Meteo returns it for free.

### 🎨 UI/UX
The existing dashboard visual design (dark SaaS theme, cards, progress bars) was kept as-is — it was already solid and didn't need a redesign. Two real gaps were fixed instead:
- **`/gridmanager` had no navigation link at all.** The only functional page in the app was unreachable from the UI without typing the URL manually. Added a "Grid Manager" nav entry (now first in the menu) and a link from the Home page.
- Added a live **Temperature** stat alongside the existing Cloud Cover / Wind Speed / Condition readouts.

### 🐞 Bug Fix 3 — Live sync race condition
**Symptom:** typing a new city (e.g. "London") and clicking "Sync Live Grid" could leave the dashboard showing the *previous* city's data instead of the new one.
**Root cause:** `OnInitializedAsync` automatically fires a weather fetch for the default city the instant the page loads. If the user typed a different city and clicked Sync before that automatic fetch finished, two network calls were in flight at once. Network latency doesn't guarantee start order = finish order, so the automatic (first-started) fetch could complete *after* the manual (second-started) one — and since both wrote to the same `DisplayedCity`/`LiveWeather*` fields, whichever finished last silently won, regardless of which one the user actually asked for.
**Fix:** `FetchLiveWeatherAndRecalculateAsync` now tags every call with a monotonically increasing sequence number and captures the requested city up front. When a call's network request returns, it only applies its result if it's still the most recently *started* call — an older, superseded fetch discards its result instead of overwriting a newer one.
**Note:** this is a real, correct fix and still worth having, but it turned out not to be what the user was actually hitting when they retested — see the Critical Fix above, found immediately afterward, which was the real blocker.

### 🛠️ Additional fixes required to make the above work correctly
- **Critical — hardcoded database path.** `GridDbContext` pointed at a hardcoded, machine-specific absolute path (`D:\AeroGrid_Project\...`), so the app could only ever run on the original machine's D: drive. `AeroGrid.Simulator` (console) and `AeroGrid.WebDashboard` (web) now both dynamically resolve the same shared `aerogrid.db` file at runtime, on any machine, OS, or drive letter.
- **Weather string case-sensitivity.** `SolarInverter`/`WindTurbine` compared weather strings with capitalized words ("Sunny", "Cloudy", ...), but `Program.cs` lowercases console input before passing it in — so no weather condition ever actually matched, and every simulation silently used the generic "unknown weather" default. Confirmed empirically: the previously-shipped `aerogrid.db` had identical generation in every single hour regardless of hour-of-day. Comparisons are now case-insensitive, and **`aerogrid.db` has been regenerated with correct data** (see the QA report).

### ✅ Testing
See the separate QA / Performance / Optimization report for full detail. Short version: the case-sensitivity and portability fixes were verified empirically (isolated execution, plus a documented before/after comparison against the actual previously-shipped database); the new weather-code mapping was unit-tested against 18 cases. The ASP.NET Core / EF Core-dependent code — including the interactivity fix above — was verified by careful manual review rather than a live build, since this environment's network access doesn't extend to NuGet or to live weather APIs. **Please do one full `dotnet run` smoke test on your own machine before considering Sprint 6 fully closed — specifically, test both the city search AND the asset toggle switches**, since both were silently non-functional for the same underlying reason (see the Critical Fix above) and only one of those two symptoms had been reported before this fix.

---

## 🚀 Future Roadmap: Month 4 - Month 6
- [x] **Sprint 6: Live REST API Integrations**: Building native internal web API controller endpoints and utilizing `HttpClient` to pull real-world weather metrics from the global Open-Meteo internet data network.
- [ ] **Sprint 7: Predictive AI Forecasting**: Implementing embedded local predictive machine regression calculations in C# to calculate forecasted optimization paths for downstream machinery layouts.
