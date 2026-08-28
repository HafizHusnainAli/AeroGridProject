using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using AeroGrid.Simulator;
using AeroGrid.WebDashboard.Services;
using AeroGrid.WebDashboard.Models;

namespace AeroGrid.WebDashboard.Components.Pages
{
    public class GridManagerBase : ComponentBase
    {
        [Inject] protected WeatherApiService WeatherService { get; set; } = default!;

        // REST API State Management Bindings
        protected string TargetCity { get; set; } = "Faisalabad";
        protected string DisplayedCity { get; set; } = "Faisalabad";
        protected double LiveCloudCover { get; set; } = 0.0;
        protected double LiveWindSpeed { get; set; } = 5.0;
        protected string LiveWeatherDescription { get; set; } = "Clear Sky";
        protected bool IsSearching { get; set; } = false;

        // ADDED BY AI (Sprint 6 - API Task): real-time temperature, now available for free
        // from Open-Meteo. Shown alongside the other live weather stats in GridManager.razor.
        protected double LiveTemperature { get; set; } = 20.0;

        // ADDED BY AI (Sprint 6 - Bug Fix 2): the exact word passed into every asset's
        // SimulateOutput(hour, weather) call below. Kept separate from LiveWeatherDescription
        // because that field can also hold a human diagnostic message on failure (e.g.
        // "Fallback Mode (...)"), and a diagnostic sentence should never accidentally become
        // the "weather" the physics engine simulates against.
        protected string SimulationWeatherCondition { get; set; } = "Sunny";

        // MODIFIED BY AI (Sprint 6 - Bug Fix 2): Replaced the two hardcoded bools
        // (IsSolarActive / IsWindActive) with one flexible list of the real polymorphic
        // EnergyAsset instances. This is the key change that makes the toggle logic correct
        // AND keeps the project flexible for future assets (per the project brief): adding a
        // new asset later (e.g. a Hydro-Electric Dam) means adding ONE line here. It then
        // automatically gets a toggle switch (GridManager.razor renders one per item via
        // @foreach) and is automatically included in / excluded from every calculation via
        // Assets.Where(a => a.IsActive) below. No other code needs to change.
        protected List<EnergyAsset> Assets { get; set; } = new()
        {
            new SolarInverter(65.0),   // matches the capacity used in AeroGrid.Simulator/Program.cs
            new WindTurbine(50.0)      // matches the capacity used in AeroGrid.Simulator/Program.cs
        };

        // Micro-Grid Core Math Logs
        protected List<GridLog> RawDatabaseLogs { get; set; } = new();
        protected List<GridLog> ProcessedWebLogs { get; set; } = new();

        // Dashboard Financial Aggregators
        protected double TotalGeneration { get; set; }
        protected double TotalDemand { get; set; }
        protected double TotalSavings { get; set; }
        protected double TotalExpenses { get; set; }

        // Cached Telemetry Properties for optimized UI rendering
        protected double PeakGeneration { get; private set; }
        protected double PeakDemand { get; private set; }
        protected double AvgGeneration { get; private set; }
        protected int DeficitHours { get; private set; }
        protected int CoveragePercent { get; private set; }
        protected int SavingsRatioPercent { get; private set; }
        protected int ExpenseRatioPercent { get; private set; }

        protected override async Task OnInitializedAsync()
        {
            await FetchDataFromDatabaseAsync();
            await FetchLiveWeatherAndRecalculateAsync();
        }

        private async Task FetchDataFromDatabaseAsync()
        {
            // MODIFIED BY AI: added Database.EnsureCreated() plus a try/catch so that a fresh
            // checkout (aerogrid.db not created yet -- e.g. AeroGrid.Simulator hasn't been run
            // even once) degrades gracefully to an empty dashboard (the existing "No data
            // available. Initialize simulator engine." UI state already handles this) instead
            // of throwing an unhandled exception out of OnInitializedAsync.
            await Task.Run(() =>
            {
                try
                {
                    using var db = new GridDbContext();
                    db.Database.EnsureCreated();
                    RawDatabaseLogs = db.GridLogs.OrderBy(log => log.Hour).ToList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AeroGrid: failed to load grid logs: {ex.Message}");
                    RawDatabaseLogs = new List<GridLog>();
                }
            });
        }

        // ADDED BY AI (post-delivery bug fix): monotonically increasing counter used to detect
        // and discard stale/superseded fetches -- see the race-condition explanation on
        // FetchLiveWeatherAndRecalculateAsync below.
        private int _fetchSequence = 0;

        protected async Task FetchLiveWeatherAndRecalculateAsync()
        {
            if (string.IsNullOrWhiteSpace(TargetCity)) return;

            // ADDED BY AI (bug fix): OnInitializedAsync automatically fires one of these calls
            // for the default city the instant the page loads. If the user then types a
            // different city and clicks "Sync Live Grid" before that automatic call has
            // finished, TWO fetches are now in flight at once. Network latency is not
            // guaranteed to match start order, so the first (automatic, default-city) call can
            // easily finish AFTER the second (user-triggered) one -- and since both write to
            // the same DisplayedCity/LiveWeather* fields, whichever finishes LAST used to win,
            // even though it wasn't the one the user actually asked for. This is exactly what
            // produced "typed London, still shows Faisalabad": the automatic Faisalabad fetch
            // (started first) completed after the manual London fetch (started second).
            //
            // The fix: every call gets its own sequence number, and the requested city is
            // captured up front (not re-read from TargetCity after the fact, since that could
            // have changed again by the time this call finishes). When a fetch's network call
            // returns, it only applies its result if it is still the MOST RECENTLY STARTED
            // request -- an older, now-superseded fetch simply discards its result instead of
            // overwriting a newer one.
            int requestId = ++_fetchSequence;
            string requestedCity = TargetCity;

            IsSearching = true;
            StateHasChanged(); 

            try
            {
                // Dispatches real-time weather inquiries
                var liveData = await WeatherService.GetLiveWeatherAsync(requestedCity);

                // A newer request has since been kicked off -- this result is stale, drop it.
                if (requestId != _fetchSequence) return;

                if (liveData != null)
                {
                    DisplayedCity = liveData.Name ?? requestedCity;
                    LiveCloudCover = liveData.Clouds.All;
                    LiveWindSpeed = liveData.Wind.Speed;
                    LiveTemperature = liveData.Temperature; // ADDED BY AI

                    if (liveData.Weather != null && liveData.Weather.Any())
                    {
                        string condition = liveData.Weather.First().Description;
                        LiveWeatherDescription = condition;
                        // ADDED BY AI: same value drives both what's displayed and what's
                        // simulated -- see the SimulationWeatherCondition declaration above.
                        SimulationWeatherCondition = condition;
                    }
                }
                else
                {
                    SetOfflineSafetyFallbacks(requestedCity, "Empty API Node");
                }
            }
            catch (Exception ex)
            {
                if (requestId != _fetchSequence) return; // stale -- a newer request superseded this one
                SetOfflineSafetyFallbacks(requestedCity, $"Fallback Mode ({ex.Message})");
            }
            finally
            {
                // Only the most recent request gets to clear the spinner and recalculate --
                // an older, superseded request's finally block becomes a no-op instead of
                // clobbering state a newer request is still in the middle of setting.
                if (requestId == _fetchSequence)
                {
                    IsSearching = false;
                    RecalculateGridMetrics();
                    StateHasChanged();
                }
            }
        }

        // MODIFIED BY AI (Sprint 6 - Bug Fix 2):
        // The previous version approximated each asset's contribution by splitting the
        // ALREADY pre-baked, historical rawLog.TotalGeneration value with a fixed 60/40
        // (solar/wind) ratio, and applied hand-invented modifier formulas
        // (1 - cloudCover/100, windSpeed/15 etc). That worked in the narrow sense that
        // switching IsSolarActive/IsWindActive to false zeroed a multiplier, but it (a) wasn't
        // real per-asset physics, (b) hardcoded a 60/40 split that only makes sense for
        // exactly 2 assets -- breaking the "must not be hardcoded for just 2 assets"
        // requirement -- and (c) ignored the polymorphic SimulateOutput() methods the rest of
        // this project (and the README's own OOP pitch) is built around.
        //
        // This version calls the REAL SimulateOutput(hour, weather) on each ACTIVE asset
        // directly, for every hour, using the live weather condition just fetched. Turning an
        // asset off (IsActive = false) now genuinely excludes it from the sum below instead of
        // approximating it away, and it works identically no matter how many assets are in the
        // Assets list.
        protected void RecalculateGridMetrics()
        {
            ProcessedWebLogs.Clear();
            TotalGeneration = 0;
            TotalDemand = 0;
            TotalSavings = 0;
            TotalExpenses = 0;

            const double gridTariff = 0.15;
            var activeAssets = Assets.Where(a => a.IsActive).ToList();

            foreach (var rawLog in RawDatabaseLogs)
            {
                // Real per-asset physics, summed only across whichever assets are active.
                double combinedGeneration = activeAssets.Sum(a => a.SimulateOutput(rawLog.Hour, SimulationWeatherCondition));

                double hourlySavings = Math.Min(combinedGeneration, rawLog.Demand) * gridTariff;
                double hourlyExpense = Math.Max(0.0, rawLog.Demand - combinedGeneration) * gridTariff;

                ProcessedWebLogs.Add(new GridLog(
                    rawLog.Hour, 
                    rawLog.Demand, 
                    combinedGeneration, 
                    hourlySavings, 
                    hourlyExpense, 
                    SimulationWeatherCondition
                ));

                TotalDemand += rawLog.Demand;
                TotalGeneration += combinedGeneration;
                TotalSavings += hourlySavings;
                TotalExpenses += hourlyExpense;
            }

            // Cache UI telemetry layouts
            PeakGeneration = ProcessedWebLogs.Count == 0 ? 0 : ProcessedWebLogs.Max(l => l.TotalGeneration);
            PeakDemand = ProcessedWebLogs.Count == 0 ? 0 : ProcessedWebLogs.Max(l => l.Demand);
            AvgGeneration = ProcessedWebLogs.Count == 0 ? 0 : ProcessedWebLogs.Average(l => l.TotalGeneration);
            DeficitHours = ProcessedWebLogs.Count(l => l.HourlyExpense > 0);

            CoveragePercent = TotalDemand <= 0 ? 0 : (int)Math.Round(Math.Min(100.0, (TotalGeneration / TotalDemand) * 100.0));

            double totalFinancials = TotalSavings + TotalExpenses;
            SavingsRatioPercent = totalFinancials <= 0 ? 0 : (int)Math.Round((TotalSavings / totalFinancials) * 100.0);
            ExpenseRatioPercent = totalFinancials <= 0 ? 0 : (int)Math.Round((TotalExpenses / totalFinancials) * 100.0);
        }

        // PM BUTTON ACTION LINKAGE FIX
        // This fires anytime a UI checkbox or button triggers an asset state modification
        protected void ToggleAsset()
        {
            RecalculateGridMetrics();
            StateHasChanged(); // Instantly pushes recalculated arrays straight back to the frontend canvas
        }

        // MODIFIED BY AI: now takes the requested city explicitly (instead of reading
        // TargetCity directly) so it always reports the city that specific fetch was actually
        // for, consistent with the sequence-guard fix above. Also still resets
        // SimulationWeatherCondition to a safe, real simulation token ("Cloudy") -- kept
        // separate from the human-readable diagnostic text in DisplayedCity/
        // LiveWeatherDescription, so a network failure degrades to a reasonable generation
        // estimate instead of feeding an error message into the physics engine.
        private void SetOfflineSafetyFallbacks(string city, string statusReason)
        {
            DisplayedCity = $"{city} ({statusReason})";
            LiveCloudCover = 75.0;
            LiveWindSpeed = 12.4;
            LiveTemperature = 20.0;
            LiveWeatherDescription = "Overcast Clouds (Fallback Active)";
            SimulationWeatherCondition = "Cloudy";
        }
    }
}
