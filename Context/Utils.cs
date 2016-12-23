using Respawn;
using System.Configuration;

namespace Context
{
    class Utils
    {
        public static void CleanDB()
        {
            var connection = ConfigurationManager.ConnectionStrings["DatabaseConnection"];

            Checkpoint checkpoint = new Checkpoint()
            {
                TablesToIgnore = new[] { "_Migrations" }
            };

            checkpoint.Reset(connection.ConnectionString);
        }
    }
}
