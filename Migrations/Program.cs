using DbUp;
using System;
using System.Configuration;
using System.Reflection;

namespace Migrations
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

            EnsureDatabase.For.SqlDatabase(connectionString);

            var builder =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetEntryAssembly())
                    .JournalToSqlTable("dbo", "_Migrations")
                    .WithTransactionPerScript()
                    .LogToConsole();

            var upgrader = builder.Build();

            // run scripts
            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(result.Error);
                System.Console.ResetColor();
            }

            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Success!");
            System.Console.ResetColor();
        }
    }
}
