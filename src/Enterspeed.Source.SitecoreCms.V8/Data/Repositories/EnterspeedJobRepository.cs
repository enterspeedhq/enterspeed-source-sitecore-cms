using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Enterspeed.Source.SitecoreCms.V8.Data.Models;
using Enterspeed.Source.SitecoreCms.V8.Exceptions;
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

                var sql = $"SELECT * FROM {_schemaName} WHERE State = {EnterspeedJobState.Failed.GetHashCode()} ORDER BY CreatedAt DESC";
                using (var command = new SqlCommand(sql, connection))
                {
                    var reader = command.ExecuteReader();

                    foreach (var record in GetFromReader(reader))
                    {
                        result.Add(EnterspeedJob.Map(record));
                    }

                    reader.Dispose();
                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            return result;
        }

        public IList<EnterspeedJob> GetFailedJobs(List<string> entityIds)
        {
            var result = new List<EnterspeedJob>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = $"SELECT * FROM {_schemaName} WHERE State = {EnterspeedJobState.Failed.GetHashCode()} ORDER BY CreatedAt DESC";
                using (var command = new SqlCommand(sql, connection))
                {
                    var reader = command.ExecuteReader();
                    foreach (var record in GetFromReader(reader))
                    {
                        result.Add(EnterspeedJob.Map(record));
                    }

                    reader.Dispose();
                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            return result;
        }

        public IList<EnterspeedJob> GetPendingJobs(int count = 2000)
        {
            var result = new List<EnterspeedJob>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = $"SELECT TOP {count} * FROM {_schemaName} WHERE State = {EnterspeedJobState.Pending.GetHashCode()} ORDER BY CreatedAt DESC";
                using (var command = new SqlCommand(sql, connection))
                {
                    var reader = command.ExecuteReader();
                    foreach (var record in GetFromReader(reader))
                    {
                        result.Add(EnterspeedJob.Map(record));
                    }

                    reader.Dispose();
                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            return result;
        }

        public IList<EnterspeedJob> GetOldProcessingTasks(int olderThanMinutes = 60)
        {
            var dateThreshhold = DateTime.UtcNow.AddMinutes(olderThanMinutes * -1).ToSqlDateTime();

            var result = new List<EnterspeedJob>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var sql = $"SELECT * FROM {_schemaName} WHERE (State = {EnterspeedJobState.Processing.GetHashCode()} OR State = {EnterspeedJobState.Pending.GetHashCode()}) " +
                         $"AND UpdatedAt <= '{dateThreshhold}'";
                using (var command = new SqlCommand(sql, connection))
                {
                    var reader = command.ExecuteReader();
                    {
                        foreach (var record in GetFromReader(reader))
                        {
                            result.Add(EnterspeedJob.Map(record));
                        }

                        reader.Dispose();
                        command.Dispose();
                    }

                    connection.Close();
                    connection.Dispose();
                }

                return result;
            }
        }

        public void Create(EnterspeedJob job)
        {
            try
            {
                var sql = $@"INSERT INTO {_schemaName} (EntityId, Culture, JobType, State, Exception, CreatedAt, UpdatedAt, EntityType, ContentState, BuildHookUrls) " +
                        $"VALUES('{job.EntityId}', '{job.Culture}', '{job.JobType.GetHashCode()}', '{job.State.GetHashCode()}', '{job.Exception}', " +
                        $"'{job.CreatedAt.ToSqlDateTime()}','{job.UpdatedAt.ToSqlDateTime()}','{job.EntityType.GetHashCode()}','{job.ContentState.GetHashCode()}', '{job.BuildHookUrls}'); ";

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                _loggingService.Error("Something went wrong storing jobs + ", e);
                throw;
            }
        }

        public void Create(IList<EnterspeedJob> jobs)
        {
            foreach (var job in jobs)
            {
                Create(job);
            }
        }

        public void Update(EnterspeedJob job)
        {
            try
            {
                var sql = $"UPDATE {_schemaName} SET EntityId = '{job.EntityId}', Culture = '{job.Culture}', JobType = {job.JobType.GetHashCode()}, " +
                          $"State = {job.State.GetHashCode()}, Exception = '{job.Exception}', CreatedAt = '{job.CreatedAt.ToSqlDateTime()}', UpdatedAt = '{job.UpdatedAt.ToSqlDateTime()}', " +
                          $"EntityType = {job.EntityType.GetHashCode()}, ContentState = {job.ContentState.GetHashCode()}, BuildHookUrls = '{job.BuildHookUrls}' WHERE Id = {job.Id}";

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                    connection.Dispose();
                }
            }
            catch (Exception e)
            {
                _loggingService.Error("Something went wrong storing jobs + ", e);
                throw;
            }
        }

        public void Update(IList<EnterspeedJob> jobs)
        {
            foreach (var job in jobs)
            {
                Update(job);
            }
        }

        public void Delete(IList<int> ids)
        {
            var arrayOfIds = ids.Select(i => i.ToString()).ToArray();
            var stringOfIds = string.Join(",", arrayOfIds);

            var sql = $@"DELETE from {_schemaName} WHERE Id IN ({stringOfIds})";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }

                connection.Close();
                connection.Dispose();
            }
        }

        IEnumerable<IDataRecord> GetFromReader(IDataReader reader)
        {
            while (reader.Read()) yield return reader;
        }
    }
}