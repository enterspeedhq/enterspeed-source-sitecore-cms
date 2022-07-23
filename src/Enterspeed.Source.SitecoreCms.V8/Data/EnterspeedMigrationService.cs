using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Sitecore.Configuration;

namespace Enterspeed.Source.SitecoreCms.V8.Data
{
    public class EnterspeedMigrationService : IEnterspeedMigrationService
    {
        private readonly string _connectionString;

        public EnterspeedMigrationService()
        {
            var connectionstringName = ConfigurationManager.AppSettings["Enterspeed.QueueSQLConnectionstringName"] ?? "Master";
            _connectionString = Settings.GetConnectionString(connectionstringName);
        }

        public void Init()
        {
            if (!TableCreated())
            {
                CreateTable();
            }
        }

        private bool TableCreated()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                const string sql = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EnterspeedJobs' ";
                using (var command = new SqlCommand(sql, connection))
                {
                    var reader = command.ExecuteReader();
                    var tableName = string.Empty;

                    // Call Read before accessing data.
                    while (reader.Read())
                    {
                        tableName = ReadSingleRow(reader);
                    }

                    reader.Dispose();

                    return !string.IsNullOrEmpty(tableName);
                }
            }
        }

        private void CreateTable()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                const string sql = "CREATE TABLE EnterspeedJobs ( Id int IDENTITY(1,1), Culture varchar(8), EntityId varchar(40), JobType int, State int, Exception varchar(Max), " +
                                   "CreatedAt dateTime, UpdatedAt dateTime, EntityType int, ContentState int, BuildHookUrls varchar(Max)); ";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        private string ReadSingleRow(IDataRecord dataRecord)
        {
            return $"{dataRecord[0]}, {dataRecord[1]}";
        }
    }
}