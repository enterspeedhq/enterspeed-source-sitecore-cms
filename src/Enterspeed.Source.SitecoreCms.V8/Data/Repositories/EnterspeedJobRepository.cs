using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Enterspeed.Source.SitecoreCms.V8.Data.Models;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Configuration;

namespace Enterspeed.Source.SitecoreCms.V8.Data.Repositories
{
    public class EnterspeedJobRepository : IEnterspeedJobRepository
    {
        private readonly string _connectionString;
        private readonly string _schemaName = "EnterspeedJobs";
        private readonly IEnterspeedSitecoreLoggingService _loggingService;

        public EnterspeedJobRepository(IEnterspeedSitecoreLoggingService loggingService)
        {
            var connectionstringName = ConfigurationManager.AppSettings["Enterspeed.QueueSQLConnectionstringName"] ?? "Master";
            _loggingService = loggingService;
            _connectionString = Settings.GetConnectionString(connectionstringName);
        }

        public IList<EnterspeedJob> GetFailedJobs()
        {
            var result = new List<EnterspeedJob>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = $@"SELECT * FROM {_schemaName} WHERE State = {EnterspeedJobState.Failed.GetHashCode()} ORDER BY CreatedAt DESC";
                using (var command = new SqlCommand(sql, connection))
                {
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        foreach (var record in GetFromReader(reader))
                        {
                            result.Add(EnterspeedJob.Map(record));
                        }
                    }

                    reader.Dispose();
                }
            }

            return result;
        }

        public IList<EnterspeedJob> GetFailedJobs(List<string> entityIds)
        {
            var result = new List<EnterspeedJob>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = $@"SELECT * FROM {_schemaName} WHERE State = {EnterspeedJobState.Failed.GetHashCode()} ORDER BY CreatedAt DESC";
                using (var command = new SqlCommand(sql, connection))
                {
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        foreach (var record in GetFromReader(reader))
                        {
                            result.Add(EnterspeedJob.Map(record));
                        }
                    }

                    reader.Dispose();
                }
            }

            return result;
        }

        public IList<EnterspeedJob> GetPendingJobs(int count)
        {
            var result = new List<EnterspeedJob>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = $@"SELECT TOP {count} * FROM {_schemaName} WHERE State = {EnterspeedJobState.Pending.GetHashCode()} ORDER BY CreatedAt DESC";
                using (var command = new SqlCommand(sql, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        foreach (var record in GetFromReader(reader))
                        {
                            result.Add(EnterspeedJob.Map(record));
                        }
                    }

                    reader.Dispose();
                }
            }

            return result;
        }

        public IList<EnterspeedJob> GetOldProcessingTasks(int olderThanMinutes = 60)
        {
            var dateThreshhold = DateTime.UtcNow.AddMinutes(olderThanMinutes * -1);

            var result = new List<EnterspeedJob>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = $@"SELECT * FROM {_schemaName} WHERE State = {EnterspeedJobState.Processing.GetHashCode()} && UpdatedAt <= {dateThreshhold}";
                using (var command = new SqlCommand(sql, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        foreach (var record in GetFromReader(reader))
                        {
                            result.Add(EnterspeedJob.Map(record));
                        }
                    }

                    reader.Dispose();
                }
            }

            return result;
        }

        public void Save(IList<EnterspeedJob> jobs)
        {
            if (jobs == null || !jobs.Any())
            {
                return;
            }

            try
            {
                foreach (var job in jobs)
                {
                    Enum jobType = job.JobType;
                    var jobTypeInt = Convert.ToInt32(jobType);

                    Enum jobState = job.State;
                    var jobStateInt = Convert.ToInt32(jobState);

                    Enum contentState = job.ContentState;
                    var contentStateInt = Convert.ToInt32(contentState);

                    Enum entityType = job.EntityType;
                    var entityTypeInt = Convert.ToInt32(entityType);

                    var createdAt = job.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                    var updatedAt = job.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss");

                    var sql = $@"INSERT INTO EnterspeedJobs ( EntityId, Culture, JobType, State, Exception, CreatedAt, UpdatedAt, EntityType, ContentState, BuildHookUrls) 
                    VALUES('{job.EntityId}', '{job.Culture}', '{jobTypeInt}', '{jobStateInt}', '{job.Exception}', '{createdAt}','{updatedAt}','{entityTypeInt}','{contentStateInt}', '{job.BuildHookUrls}'); ";

                    using (var connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();

                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.ExecuteNonQuery();
                        }

                        connection.Close();
                    }
                }
            }
            catch (Exception e)
            {
                _loggingService.Error("Something went wrong storing jobs + ", e);
                throw;
            }
        }

        public void Delete(IList<int> ids)
        {
            var arrayOfIds = ids.Select(i => i.ToString()).ToArray();
            var stringOfIds = string.Join(",", arrayOfIds);

            var sql = $@"DELETE from `{_schemaName}` WHERE `Id` IN ({stringOfIds}));";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
            throw new NotImplementedException();
        }

        IEnumerable<IDataRecord> GetFromReader(IDataReader reader)
        {
            while (reader.Read()) yield return reader;
        }

    }
}