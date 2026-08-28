using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace AeroGrid.Simulator
{
    // Inheriting from DbContext turns this class into a database access controller
    public class GridDbContext : DbContext
    {
        // This property represents our database table. It maps the GridLog objects into rows!
        public DbSet<GridLog> GridLogs { get; set; }

        // Configure the connection path to create the database file safely on your hard drive
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // MODIFIED BY AI (critical portability fix): the previous connection string was a
            // hardcoded, machine-specific absolute Windows path
            // (@"D:\AeroGrid_Project\AeroGrid.Simulator\aerogrid.db"). That only ever worked on
            // the one machine that has a D: drive with the project at that exact path — on any
            // other machine, another OS, or a CI/test runner, EF Core would either fail outright
            // or (worse) silently create a fresh, empty database in an unexpected place. It also
            // meant AeroGrid.Simulator (console) and AeroGrid.WebDashboard (web) could easily end
            // up pointing at two different files instead of sharing one.
            // ResolveSharedDatabasePath() below finds the AeroGrid.Simulator folder at runtime
            // relative to wherever the app is actually running, so both projects always agree on
            // the same aerogrid.db, on any machine, OS, or drive letter, in Debug or Release.
            string dbPath = ResolveSharedDatabasePath();
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = dbPath };
            optionsBuilder.UseSqlite(connectionStringBuilder.ConnectionString);
        }

        // ADDED BY AI: Walks up from the running app's own folder until it finds the
        // AeroGrid.Simulator project folder alongside it, so AeroGrid.Simulator.exe and
        // AeroGrid.WebDashboard.exe (which live in two different bin/ output folders) both
        // resolve to the exact same physical aerogrid.db file.
        private static string ResolveSharedDatabasePath()
        {
            const string ProjectFolderName = "AeroGrid.Simulator";
            const string DbFileName = "aerogrid.db";

            DirectoryInfo? current = new DirectoryInfo(AppContext.BaseDirectory);

            for (int i = 0; i < 10 && current != null; i++)
            {
                string simulatorFolder = Path.Combine(current.FullName, ProjectFolderName);
                if (Directory.Exists(simulatorFolder))
                {
                    return Path.Combine(simulatorFolder, DbFileName);
                }
                current = current.Parent;
            }

            // Fallback for an unusual deployment layout where the sibling project folder can't
            // be found (e.g. a standalone publish output with no source tree around it) — keep
            // the app usable by creating the database next to wherever it's running instead of
            // throwing.
            return Path.Combine(AppContext.BaseDirectory, DbFileName);
        }

        // Apply specialized structural definitions to map data properties correctly
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tells the database engine that the 'Hour' property serves as the primary unique key ID
            modelBuilder.Entity<GridLog>().HasKey(log => log.Hour);
        }
    }
}