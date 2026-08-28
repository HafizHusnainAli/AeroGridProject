
using System;
using System.Collections.Generic; // Required for using List<T> and other generic collections (List Collection).
// Instructs the compiler to use the AeroGrid.Simulator namespace room, which contains the SolarInverter class.
using AeroGrid.Simulator;

// Create an instance of the SolarInverter class
double factoryDemand = 80.0; // Increased factory demand in kWh to challenge our hybrid grid system and test the financial balancing engine.
string currentWeather; //current weather condition

// new financial configuration variables
double gridTriff = 0.15; //grid tariff in $/kWh
double totalMoneySaved = 0.0; //total money saved in $
double totalMoneySpent = 0.0; //total money spent in $



Console.WriteLine("\n Enter Current weather condition:");
currentWeather = Console.ReadLine()?.ToLower() ?? " ";
if (string.IsNullOrEmpty(currentWeather) || (currentWeather != "sunny" && currentWeather != "cloudy" && currentWeather != "rainy" && currentWeather != "stormy" && currentWeather != "snowy" && currentWeather != "windy" && currentWeather != "foggy"))
{
    Console.WriteLine("Invalid weather condition. Please enter a valid weather condition.");
    return;
}

//=============================================
// POLYMORPHIC DATA STRUCTURE IMPLEMENTATION
// CREATE a collection that stores ANY object inheriting from EnergyAsset (SolarInverter, WindTurbine, etc.)
//=============================================
List<EnergyAsset> energyAssets = new List<EnergyAsset>()
{
    new SolarInverter(65.0),
    new WindTurbine(50.0) // Our brand new asset working alongside Solar!
};

//=============================================
// Creating an empty data structure to store our simulation history for each hour of the simulation
//=============================================
List<GridLog> simulationHistory = new List<GridLog>(); // Create a list to store GridLog objects for each hour of the simulation

Console.WriteLine($"\n\tStarting our Hybrid Grid 24-hour simulation ({energyAssets.Count}) Assets Active...  \n ");

for (int hour = 1; hour <= 24; hour++)
{
    // Call the object's method to calculate the dynamic power stream!

    double combinedGridGeneration = 0.0;
    // Polymorphic behavior: Loop through each EnergyAsset in the collection will use its own implementation of SimulateOutput, allowing for diverse energy generation calculations based on the specific asset type (SolarInverter, WindTurbine, etc.).
    foreach (EnergyAsset asset in energyAssets)
    {
        // C# dynamically calls the correct math formula behind the scenes (Solar wave or Wind modifier)!
        double assetOutput = asset.SimulateOutput(hour, currentWeather);
        combinedGridGeneration += assetOutput;

    }

    //New financial Balancing Engine
    double HourlyExpense = 0.0; //initialize hourly expense
    double HourlySavings = 0.0; //initialize hourly savings

    if (combinedGridGeneration >= factoryDemand)
    {            // Solar covers everything! We save what the factory would have cost to run on the grid.
                 //Savings equal the entire grid cost you successfully avoided paying
        HourlySavings = factoryDemand * gridTriff; //calculate hourly savings
        totalMoneySaved += HourlySavings; //add hourly savings to total money saved
    }
    else
    {             // Solar falls short. We calculate the gap and buy it from the city.
        HourlyExpense = (factoryDemand - combinedGridGeneration) * gridTriff; //calculate hourly expense
        totalMoneySpent += HourlyExpense; //add hourly expense to total money spent

        // We still save some money on the portion of solar output that covers the factory demand.
        HourlySavings = combinedGridGeneration * gridTriff; //calculate hourly savings
        totalMoneySaved += HourlySavings; //add hourly savings to total money saved
    }

    // ============================================================
    // DATA STRUCTURE STATE EXTRACTION
    // Build a new snapshot object for this hour and add it into our memory list
    // ============================================================
    GridLog hourRecord = new GridLog(hour, factoryDemand, combinedGridGeneration, HourlySavings, HourlyExpense, currentWeather);
    simulationHistory.Add(hourRecord); // Add the hour record to the simulation history list

    // Keeping my single operational terminal print line here intact
    Console.WriteLine($"Hour {hour:00} Demand: {factoryDemand} kWh | Grid Generation: {combinedGridGeneration} kWh | Hourly Savings: ${HourlySavings:F2} | Hourly Expense: ${HourlyExpense:F2}");
}
// -----------  Final Financial Report  -----------
Console.WriteLine("\n\t ==============x==============");
Console.WriteLine("\t---- Final Financial Report ---- ");
Console.WriteLine("\t ==============x==============");
Console.WriteLine($"\nTotal Operational Expenses (Paid to Grid): ${totalMoneySpent:F2}");
Console.WriteLine($"Total Operational Savings (Retained Cash): ${totalMoneySaved:F2}");

if (totalMoneySaved > totalMoneySpent)
{
    Console.WriteLine($"Overall Savings: ${totalMoneySaved - totalMoneySpent:F2}");
}
else if (totalMoneySpent > totalMoneySaved)
{
    Console.WriteLine($"Overall Losses: ${totalMoneySpent - totalMoneySaved:F2}");
}
else
{
    Console.WriteLine("No net savings or losses.");
}

double NetFinancialImpact = totalMoneySaved - totalMoneySpent;
if (NetFinancialImpact >= 0)
{
    Console.WriteLine($"Net Financial Impact (Retained Cash): +${NetFinancialImpact:F2}");
}
else
{
    Console.WriteLine($"Net Financial Impact (Additional Expenses): -${Math.Abs(NetFinancialImpact):F2}");
}

//==============================================
// ADVANCE BUSINESS INTELLIGENCE ANALYTICS ENGINE IMPLEMENTATION
// Instantiate our custom analytic class, passing in our stored history list
//==============================================
AnalyticsEngine analytics = new AnalyticsEngine(simulationHistory);

GridLog peakHourData = analytics.GetPeakProductionHour();
int deficitHours = analytics.GetGridDependencyHoursCount();
GridLog demandedPeakHour = analytics.GetPeakDemandHour();

Console.WriteLine("\n\t --- Advance Grid Analysis --- ");
if (peakHourData != null)
{
    Console.WriteLine($"\tPeak Green Energy Production Hour: {peakHourData.Hour:00} | Total Generation: {peakHourData.TotalGeneration} kWh | Weather: {peakHourData.WeatherCondition}");
}
if (demandedPeakHour != null)
{
    Console.WriteLine($"\tPeak Energy Demand Hour: {demandedPeakHour.Hour:00} | Total Demand: {demandedPeakHour.Demand} kWh | Weather: {demandedPeakHour.WeatherCondition}");
}
Console.WriteLine($"\tGrid Dependency Duration: {deficitHours} Hours / 24 Hours | Percentage: {(deficitHours / 24.0) * 100:F2}%");

//=================================================
// SQLite PERSISTENT ENGINE IMPLEMENTATION
// Save your memory logging arrays straight into our local hard drive storage
//==================================================
using (GridDbContext database = new GridDbContext())
{
    // Step 1: Ensuring the physical aerogrid.db file exists on your HDD.
    // If it doesn't exist, this line automatically creates it from scratch!
    database.Database.EnsureCreated();

    // Step 2: Clear out any old previous simulation runs from the table 
    // so we don't crash from duplicate key errors when re-running the app.
    database.GridLogs.RemoveRange(database.GridLogs);

    // Step 3: Dump our entire memory array ledger straight into the context rows
    database.GridLogs.AddRange(simulationHistory);

    // Step 4: Commit changes and write the physical bytes down onto your HDD
    database.SaveChanges();

    Console.WriteLine("\n\t[DATABASE SUCCESS] Stored 24 historical ledger records to aerogrid.db!");
}
//=============================================================

Console.WriteLine("\n\t==============x==============");
Console.WriteLine("\nSimulation complete. Thank you for Trusting AeroGrid Simulator.\n");
