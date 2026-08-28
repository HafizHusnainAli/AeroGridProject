using System;
using System.Linq;
using System.Collections.Generic;

namespace AeroGrid.Simulator
{
    public class AnalyticsEngine
    {
        // Property to reference our history dataset
        private List<GridLog> _history;

        // Constructor injects the (simulation Log)history dataset into the AnalyticsEngine(processing engine)
        public AnalyticsEngine(List<GridLog> history)
        {
            _history = history;
        }

        // BI Algorithm 1: Identify the absolute peak hour of green energy production
        public GridLog GetPeakProductionHour()
        {
            if (_history == null || _history.Count == 0)
            {
                throw new InvalidOperationException("No simulation history available.");
            }

            GridLog peakRecord = _history[0];
            foreach (GridLog log in _history)
            {
                if (log.TotalGeneration > peakRecord.TotalGeneration)
                {
                    peakRecord = log; // Found a new peak performance row!
                }
            }
            return peakRecord;
        }

        // BI Algorithm 2: Identify the absolute peak hour of energy demand
        public GridLog GetPeakDemandHour()
        {
            // Step 1: Validate history
            if (_history == null || _history.Count == 0)
            {
                throw new InvalidOperationException("No simulation history available.");
            }

            // Step 2: Find peak hour using LINQ
            // OrderByDescending sorts by Demand high -> low
            // First() returns the first item as GridLog (not GridLog?) so no CS8603
            return _history.OrderByDescending(log => log.Demand).First();
        }

        // BI Algorithm 3: Count hours where grid had generation deficit
        public int GetGridDependencyHoursCount()
        {
            // Step 1: Validate - return 0 if no data
            if (_history == null || _history.Count == 0)
            {
                return 0;
            }

            // Step 2: Count with condition using LINQ
            // Count() returns int, never null, so no warning
            return _history.Count(log => log.HourlyExpense > 0);
        }
    }
}