using System;

namespace AeroGrid.Simulator
{
    public class WindTurbine : EnergyAsset
    {
        // Constructor passes the name and maxCapacity to the parent base class constructor
        public WindTurbine(double maxCapacity) : base("Industrial Wind Turbine", maxCapacity)
        {
            // The parent class handles setting the maxCapacity and name properties, so we don't need to set them again here.
        }

        public override double SimulateOutput(int hour, string weather)
        {
            double windEfficiencyModifier = 0.0;

            // MODIFIED BY AI (Sprint 6 - Bug Fix 2 dependency): same case-insensitive fix as
            // SolarInverter.cs — see the comment there for why this matters.
            if (string.Equals(weather, "Sunny", StringComparison.OrdinalIgnoreCase)) { windEfficiencyModifier = 0.3; } // Sunny weather yields 30% output
            else if (string.Equals(weather, "Cloudy", StringComparison.OrdinalIgnoreCase)) { windEfficiencyModifier = 0.5; } // Cloudy weather yields 50% output
            else if (string.Equals(weather, "Rainy", StringComparison.OrdinalIgnoreCase)) { windEfficiencyModifier = 0.7; } // Rainy weather yields 70% output
            else if (string.Equals(weather, "Snowy", StringComparison.OrdinalIgnoreCase)) { windEfficiencyModifier = 0.4; } // Snowy weather yields 40% output
            else if (string.Equals(weather, "Windy", StringComparison.OrdinalIgnoreCase)) { windEfficiencyModifier = 1.2; } // Windy weather yields 120% output
            else if (string.Equals(weather, "Foggy", StringComparison.OrdinalIgnoreCase)) { windEfficiencyModifier = 0.2; } // Foggy weather yields 20% output
            else { windEfficiencyModifier = 0.1; } // Default to 10% output for unknown weather conditions

            // Ensure hour is within valid range
            if (hour < 0 || hour > 24)
            {
                throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be between 0 and 24.");
            }

            // Unlike SolarPanel, wind turbines can operate 24 hours a day!
            // However, wind typically picks up slightly during late-night and early morning hours, so we can simulate a slight increase in output during those times.

            double timeBonus = (hour < 6 || hour > 18) ? 1.15 : 0.85; // 10% bonus for night hours

            double calculatedOutput = MaxCapacity* windEfficiencyModifier * timeBonus;

            if(calculatedOutput > MaxCapacity)
            {
                calculatedOutput = MaxCapacity; // Cap the output at max capacity
            }
            return calculatedOutput;
        }
    }
}