using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Sitecore.Configuration;

namespace Enterspeed.Source.SitecoreCms.V8.Data
{
    public class EnterspeedEnterspeedMigrationService : IEnterspeedMigrationService
    {
        private string _connectionstring;

        public EnterspeedEnterspeedMigrationService()
        {
            var connectionstringName = ConfigurationManager.AppSettings["Enterspeed.QueueSQLConnectionstringName"] ?? "Master";
            _connectionstring = Settings.GetConnectionString(connectionstringName);
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
            using (var connection = new SqlConnection(_connectionstring))
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
            using (var connection = new SqlConnection(_connectionstring))
            {
                connection.Open();
                const string sql = "CREATE TABLE EnterspeedJobs ( Id int, EntityId varchar(255), JobType int, JobState int, Exception varchar(Max), " +
                                   "CreatedAt dateTime, UpdatedAt dateTime, EntityType int, ContentState int); ";

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