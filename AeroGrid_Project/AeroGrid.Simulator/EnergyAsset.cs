using System;

namespace AeroGrid.Simulator
{
    public abstract class EnergyAsset
    {
        
        //shared properties for all industrial energy assets
        public string Name { get; set; }
        public double MaxCapacity { get; set; } // in kW

        // ADDED BY AI (Sprint 6 - Bug Fix 2): Generic on/off state shared by every asset type.
        // Living here on the base class (instead of as separate bools like "IsSolarActive" /
        // "IsWindActive" on the dashboard) means any NEW asset that inherits EnergyAsset gets
        // toggle support automatically — nothing else needs to change when a 3rd, 4th, etc.
        // asset is added later. Defaults to true so existing behavior is unchanged unless
        // something explicitly switches it off.
        public bool IsActive { get; set; } = true;

        // shared constructor for all energy assets
        public EnergyAsset( string name, double maxCapacity)
        {
            Name = name;
            MaxCapacity = maxCapacity;
        }

        //Abstract method: Every child class must implement this method to simulate the output of the energy asset based on the hour and weather conditions.
        public abstract double SimulateOutput(int hour, string weather);
    }

}