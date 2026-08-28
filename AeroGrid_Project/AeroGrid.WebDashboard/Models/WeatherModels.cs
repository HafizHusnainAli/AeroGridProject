using System.Text.Json.Serialization;

namespace AeroGrid.WebDashboard.Models
{
    public class OpenWeatherResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("weather")]
        public List<WeatherDescription> Weather { get; set; } = new();

        [JsonPropertyName("wind")]
        public WindData Wind { get; set; } = new();

        [JsonPropertyName("clouds")]
        public CloudData Clouds { get; set; } = new();

        // ADDED BY AI (Sprint 6 - API Task): real-time temperature, now populated from
        // Open-Meteo's "temperature_2m" reading (see WeatherApiService.GetLiveWeatherAsync).
        // Kept as a flat field here rather than mimicking OpenWeatherMap's nested
        // main.temp shape, since this class is only ever built by our own service now.
        public double Temperature { get; set; }
    }

    public class WeatherDescription
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class WindData
    {
        [JsonPropertyName("speed")]
        public double Speed { get; set; }
    }

    public class CloudData
    {
        [JsonPropertyName("all")]
        public int All { get; set; }
    }

    // =====================================================================
    // ADDED BY AI (Sprint 6 - API Task): DTOs for the new Open-Meteo integration.
    // Open-Meteo is used instead of OpenWeatherMap because it needs no API key (the
    // previous OpenWeatherMap integration shipped with a placeholder key that could
    // never actually authenticate — see WeatherApiService.cs) and it's the provider
    // this project's own README already names for Sprint 6. Two calls are needed:
    // 1) Geocoding: free-text city name -> latitude/longitude
    // 2) Forecast: latitude/longitude -> real-time current conditions
    // =====================================================================

    public class OpenMeteoGeocodingResponse
    {
        [JsonPropertyName("results")]
        public List<OpenMeteoPlace>? Results { get; set; }
    }

    public class OpenMeteoPlace
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;

        [JsonPropertyName("admin1")]
        public string? Admin1 { get; set; }
    }

    public class OpenMeteoForecastResponse
    {
        [JsonPropertyName("current")]
        public OpenMeteoCurrentWeather? Current { get; set; }
    }

    public class OpenMeteoCurrentWeather
    {
        [JsonPropertyName("temperature_2m")]
        public double Temperature2m { get; set; }

        [JsonPropertyName("cloud_cover")]
        public double CloudCover { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public double WindSpeed10m { get; set; }

        [JsonPropertyName("weather_code")]
        public int WeatherCode { get; set; }
    }
}
