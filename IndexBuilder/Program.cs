using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchEngineLib;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace IndexBuilder
{
    /// <summary>
    /// ToDo:
    /// Implement multiple query/index functionality
    /// Implement index search functionality
    /// Create database for search
    /// Create config file for console
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string debugOrRelease = "Release";
            string filePath = String.Empty;
            string contents = String.Empty;
            string query = String.Empty;
            string connectionString = ConfigurationManager.ConnectionStrings["db"].ConnectionString;
            DataTable results = null;

#if DEBUG
            debugOrRelease = "Debug";
#endif

            filePath = $"/bin/{debugOrRelease}/Index.config";

            if (!File.Exists(filePath))
                return;

            query = GetQuery(filePath);

            if (String.IsNullOrWhiteSpace(query))
                return;

            results = GetResults(connectionString, query);

            var searchIndex = new Index(results);

            Console.Read();

            // Implement search functionality here
        }

        /// <summary>
        /// Retrieve query from config file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static string GetQuery(string filePath)
        {
            string contents = String.Empty;

            using (var reader = new StreamReader(filePath))
            {
                contents = reader.ReadToEnd();
            }

            return contents.Substring(contents.IndexOf('{'), contents.LastIndexOf('}'));
        }

        /// <summary>
        /// Run query obtained from the config file
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static DataTable GetResults(string connectionString, string query)
        {
            DataTable results = null;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(query, conn))
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    conn.Open();
                    adapter.Fill(results);
                }
            }
            catch (Exception)
            {
                return null;
            }

            return results;
        }
    }
}
