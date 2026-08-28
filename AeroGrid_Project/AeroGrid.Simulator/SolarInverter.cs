using System;

namespace AeroGrid.Simulator
{
    // The color (:) indicates that the SolarInverter class inherits from the EnergyAsset class, meaning it will have access to the properties and methods defined in EnergyAsset.
    public class SolarInverter : EnergyAsset
    {
        // 2. Constructor (Special method to initialize our blueprint object)
        public SolarInverter(double maxCapacity) : base("Solar Panel Array", maxCapacity)
        {
            // The parent class handles setting the maxCapacity and name properties, so we don't need to set them again here.
        }

        // 3. Core Action Method (Simulate Solar Output Based on Time of Day and Weather Conditions)
        public override double SimulateOutput(int hour, string weather)
        {
            //Evaluate Base Output based on weather Criteria
            double baseSolarOutput=0.0;

            // MODIFIED BY AI (Sprint 6 - Bug Fix 2 dependency): switched from "==" (case-sensitive)
            // to StringComparison.OrdinalIgnoreCase. Previously "sunny" (e.g. from
            // AeroGrid.Simulator/Program.cs, which lowercases console input) could never match
            // "Sunny" here, so every run silently fell through to the 10% default branch
            // regardless of the weather actually entered/fetched. Confirmed empirically: the
            // shipped aerogrid.db had identical output every hour even though it was logged
            // against "sunny". This also makes the method safe for the new live weather feed
            // in AeroGrid.WebDashboard, which now calls this same method directly per asset.
            if(string.Equals(weather, "Sunny", StringComparison.OrdinalIgnoreCase)){baseSolarOutput = MaxCapacity;}//Sunny weather yields maximum output
            else if(string.Equals(weather, "Cloudy", StringComparison.OrdinalIgnoreCase)){baseSolarOutput = MaxCapacity * 0.5;}//Cloudy weather yields 50% output
            else if(string.Equals(weather, "Rainy", StringComparison.OrdinalIgnoreCase)){baseSolarOutput = MaxCapacity * 0.2;}//Rainy weather yields 20% output
            else if(string.Equals(weather, "Stormy", StringComparison.OrdinalIgnoreCase)){baseSolarOutput = MaxCapacity * 0.1;}//Stormy weather yields 10% output
            else if(string.Equals(weather, "Snowy", StringComparison.OrdinalIgnoreCase)){baseSolarOutput = MaxCapacity * 0.15;}//Snowy weather yields 15% output
            else if(string.Equals(weather, "Windy", StringComparison.OrdinalIgnoreCase)){baseSolarOutput = MaxCapacity * 0.6;}//Windy weather yields 60% output
            else if(string.Equals(weather, "Foggy", StringComparison.OrdinalIgnoreCase)){baseSolarOutput = MaxCapacity * 0.3;}//Foggy weather yields 30% output
            else{baseSolarOutput = MaxCapacity * 0.1;}//Default to 10% output for unknown weather conditions
            

            // Ensure hour is within valid range
            if (hour < 0 || hour > 24)
            {
                throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be between 0 and 24.");
            }

            // Calculate the day factor based on the hour of the day
            double dayFactor = (hour >= 6 && hour <= 18) ? Math.Sin((hour - 6) * Math.PI / 12) : 0.0;

            // Calculate dynamic solar output based on time of day and weather conditions
            double dynamicSolarOutput = baseSolarOutput * dayFactor;

            return dynamicSolarOutput;
        }

        internal double SimulateSolarOutput(int hour, string currentWeather)
        {
            throw new NotImplementedException();
        }
    }
}