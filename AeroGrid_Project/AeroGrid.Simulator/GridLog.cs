using System;

namespace AeroGrid.Simulator
{
    public class GridLog
    {
        // Data structure Parameters tracking our timeline, metrics and Financials
        public int Hour { get; set; }
        public double Demand { get; set; }
        public double TotalGeneration { get; set; }
        public double HourlySavings { get; set; }
        public double HourlyExpense { get; set; }
        public string WeatherCondition { get; set; }


        // Constructor to build our clean data rows
        public GridLog(int hour, double demand, double totalGeneration, double HourlySavings, double HourlyExpense, string weatherCondition)
        {
            Hour = hour;
            this.Demand = demand;
            this.TotalGeneration = totalGeneration;
            this.HourlySavings = HourlySavings;
            this.HourlyExpense = HourlyExpense;
            this.WeatherCondition = weatherCondition;
        }         
    }
}