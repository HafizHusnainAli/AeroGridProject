using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AeroGrid.WebDashboard.Models;

namespace AeroGrid.WebDashboard.Services
{
    public class WeatherApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WeatherApiService> _logger;

        // MODIFIED BY AI (Sprint 6 - Bug Fix 1 / API Task):
        // The previous implementation called OpenWeatherMap (api.openweathermap.org) with a
        // hardcoded placeholder API key ("TODO: Replace this placeholder with your exact
        // 32-character active key string"). That key was never a real, active credential, so
        // every request came back 401 Unauthorized, every call fell into the catch block, and
        // the dashboard always showed the same fallback weather text no matter what city was
        // entered -- which is what "stuck on Faisalabad" actually was.
        //
        // This is now rebuilt on Open-Meteo (open-meteo.com), which is free and requires NO
        // API key at all -- so there's no credential to expire, misconfigure, or forget to
        // set. This also matches the provider this project's own README already names for
        // Sprint 6 ("...pull real-world weather metrics from the global Open-Meteo internet
        // data network").
        //
        // Two public, keyless endpoints are used:
        //   1. Geocoding API  - turns free-text city input (any city: London, Lahore, Paris...)
        //      into latitude/longitude.
        //   2. Forecast API   - turns latitude/longitude into real-time current conditions.
        private const string GeocodingBaseUrl = "https://geocoding-api.open-meteo.com/v1/search";
        private const string ForecastBaseUrl = "https://api.open-meteo.com/v1/forecast";

        // A real, sustained wind is what a wind turbine cares about most, so it's allowed to
        // promote the simulated condition to "Windy" regardless of cloud cover. ~36 km/h.
        private const double WindyThresholdMetersPerSecond = 10.0;

        public WeatherApiService(HttpClient httpClient, ILogger<WeatherApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Critical Fix (kept from the original implementation): Add User-Agent to prevent
            // API firewalls from dropping the socket.
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "AeroGrid-WebDashboard-Client");
            }
        }

        // Returns live weather for any user-entered city, or null if the city can't be
        // resolved. Still returns the existing OpenWeatherResponse shape (see
        // Models/WeatherModels.cs) on purpose, so GridManagerBase.cs -- which already knows
        // how to read liveData.Name / .Clouds.All / .Wind.Speed / .Weather.First().Description
        // -- needed no changes to keep consuming it.
        public async Task<OpenWeatherResponse?> GetLiveWeatherAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city)) return null;

            try
            {
                // ---- Step 1: Geocode the free-text city name into coordinates. ----
                string geoUrl = $"{GeocodingBaseUrl}?name={Uri.EscapeDataString(city)}&count=1&language=en&format=json";
                _logger.LogInformation("AeroGrid Network Outbound: Geocoding city {City}", city);

                var geoResult = await _httpClient.GetFromJsonAsync<OpenMeteoGeocodingResponse>(geoUrl);
                var place = geoResult?.Results?.FirstOrDefault();

                if (place == null)
                {
                    _logger.LogWarning("AeroGrid Geocoding: no match found for city {City}", city);
                    return null;
                }

                // ---- Step 2: Pull real-time current weather for those coordinates. ----
                string forecastUrl =
                    $"{ForecastBaseUrl}?latitude={place.Latitude}&longitude={place.Longitude}" +
                    "&current=temperature_2m,cloud_cover,wind_speed_10m,weather_code" +
                    "&wind_speed_unit=ms&timezone=auto";

                _logger.LogInformation("AeroGrid Network Outbound: Fetching live forecast for {City} ({Lat},{Lon})", city, place.Latitude, place.Longitude);

                var forecast = await _httpClient.GetFromJsonAsync<OpenMeteoForecastResponse>(forecastUrl);

                if (forecast?.Current == null)
                {
                    _logger.LogWarning("AeroGrid Forecast: empty response for {City}", city);
                    return null;
                }

                var current = forecast.Current;
                string displayName = string.IsNullOrWhiteSpace(place.Admin1)
                    ? $"{place.Name}, {place.Country}"
                    : $"{place.Name}, {place.Admin1}, {place.Country}";

                string simulationCondition = MapWeatherCodeToCondition(current.WeatherCode, current.WindSpeed10m);

                return new OpenWeatherResponse
                {
                    Name = displayName,
                    Temperature = current.Temperature2m,
                    Clouds = new CloudData { All = (int)Math.Round(current.CloudCover) },
                    Wind = new WindData { Speed = current.WindSpeed10m },
                    Weather = new List<WeatherDescription>
                    {
                        new WeatherDescription { Description = simulationCondition }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AeroGrid Network Failure for city {City}", city);
                throw;
            }
        }

        // ADDED BY AI: Translates Open-Meteo's numeric WMO weather code into one of the exact
        // condition words that EnergyAsset.SimulateOutput (SolarInverter / WindTurbine) already
        // knows how to simulate against ("Sunny", "Cloudy", "Rainy", "Stormy", "Snowy", "Windy",
        // "Foggy"). This is deliberately the SAME string used for both what the dashboard
        // displays and what drives the physics, so what the user sees is always exactly what
        // the numbers below it are actually based on.
        // WMO code reference: https://open-meteo.com/en/docs (see "WMO Weather interpretation codes")
        private static string MapWeatherCodeToCondition(int code, double windSpeedMs)
        {
            string baseCondition = code switch
            {
                0 => "Sunny",                                  // Clear sky
                1 or 2 or 3 => "Cloudy",                        // Mainly clear / partly cloudy / overcast
                45 or 48 => "Foggy",                            // Fog / depositing rime fog
                >= 51 and <= 67 => "Rainy",                     // Drizzle, freezing drizzle, rain, freezing rain
                80 or 81 or 82 => "Rainy",                      // Rain showers
                71 or 73 or 75 or 77 or 85 or 86 => "Snowy",    // Snow fall / snow grains / snow showers
                >= 95 and <= 99 => "Stormy",                    // Thunderstorm (with or without hail)
                _ => "Cloudy"                                   // Safe default for any undocumented code
            };

            // A sufficiently strong wind matters most to a wind turbine, so let real wind speed
            // promote the condition to "Windy" -- but don't downgrade a storm or snow, which
            // already imply their own strong-wind handling.
            if (windSpeedMs >= WindyThresholdMetersPerSecond && baseCondition is "Sunny" or "Cloudy" or "Foggy")
            {
                return "Windy";
            }

            return baseCondition;
        }
    }
}
