using System.Text.Json.Serialization;

namespace AeroGrid.WebDashboard.Models
{
    // The master container object matching the root JSON response from the OpenWeather API
    public class WeatherResponse
    {
        [JsonPropertyName("coord")]
        public Coordinates Coordinates { get; set; } = new();

        [JsonPropertyName("weather")]
        public Weather[] Weather { get; set; } = Array.Empty<Weather>();

        [JsonPropertyName("base")]
        public string Base { get; set; } = string.Empty;

        [JsonPropertyName("main")]
        public Main Main { get; set; } = new();

        [JsonPropertyName("visibility")]
        public int Visibility { get; set; }

        [JsonPropertyName("wind")]
        public Wind Wind { get; set; } = new();

        [JsonPropertyName("clouds")]
        public Clouds Clouds { get; set; } = new();

        [JsonPropertyName("dt")]
        public int Dt { get; set; }

        [JsonPropertyName("sys")]
        public Sys Sys { get; set; } = new();

        [JsonPropertyName("timezone")]
        public int Timezone { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("cod")]
        public int Cod { get; set; }

        [JsonPropertyName("current")]
        public CurrentWeatherDetails Current { get; set; } = new();
    }

    // =====================================================================
    // PM FIX: ADDED SUB-CLASSES TO SOLVE CONTEXT COMPILER BREAKS
    // =====================================================================

    public class Coordinates
    {
        [JsonPropertyName("lon")]
        public double Lon { get; set; }

        [JsonPropertyName("lat")]
        public double Lat { get; set; }
    }

    public class Weather
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("main")]
        public string MainCondition { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;
    }

    public class Main
    {
        [JsonPropertyName("temp")]
        public double Temp { get; set; }

        [JsonPropertyName("feels_like")]
        public double FeelsLike { get; set; }

        [JsonPropertyName("temp_min")]
        public double TempMin { get; set; }

        [JsonPropertyName("temp_max")]
        public double TempMax { get; set; }

        [JsonPropertyName("pressure")]
        public int Pressure { get; set; }

        [JsonPropertyName("humidity")]
        public int Humidity { get; set; }

        [JsonPropertyName("sea_level")]
        public int SeaLevel { get; set; }

        [JsonPropertyName("grnd_level")]
        public int GroundLevel { get; set; }
    }

    public class Wind
    {
        [JsonPropertyName("speed")]
        public double Speed { get; set; }

        [JsonPropertyName("deg")]
        public int Deg { get; set; }

        [JsonPropertyName("gust")]
        public double Gust { get; set; }
    }

    public class Clouds
    {
        [JsonPropertyName("all")]
        public int All { get; set; }
    }

    public class Sys
    {
        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;

        [JsonPropertyName("sunrise")]
        public int Sunrise { get; set; }

        [JsonPropertyName("sunset")]
        public int Sunset { get; set; }
    }

    public class CurrentWeatherDetails
    {
        [JsonPropertyName("temp")]
        public double Temp { get; set; }

        [JsonPropertyName("feels_like")]
        public double FeelsLike { get; set; }

        [JsonPropertyName("temp_min")]
        public double TempMin { get; set; }

        [JsonPropertyName("temp_max")]
        public double TempMax { get; set; }

        [JsonPropertyName("pressure")]
        public int Pressure { get; set; }

        [JsonPropertyName("humidity")]
        public int Humidity { get; set; }
    }
}
