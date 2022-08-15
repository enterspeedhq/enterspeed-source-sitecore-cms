using System;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Enterspeed.Source.SitecoreCms.V8.Data.Models
{
    public class EnterspeedJob
    {
        public int Id { get; set; }

        public string EntityId { get; set; }

        public string Culture { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EnterspeedJobType JobType { get; set; }

        public DateTime CreatedAt { get; set; }

        public EnterspeedJobState State { get; set; }

        public string Exception { get; set; }

        public DateTime UpdatedAt { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EnterspeedJobEntityType EntityType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EnterspeedContentState ContentState { get; set; }

        public string BuildHookUrls { get; set; }

        public static EnterspeedJob Map(IDataRecord record)
        {
            var contentStateParsed = Enum.TryParse(record["ContentState"].ToString(), out EnterspeedContentState contentState);
            var jobTypeParsed = Enum.TryParse(record["JobType"].ToString(), out EnterspeedJobType jobType);
            var jobStateParsed = Enum.TryParse(record["State"].ToString(), out EnterspeedJobState state);
            var entityTypeParsed = Enum.TryParse(record["EntityType"].ToString(), out EnterspeedJobEntityType entityType);

            var enterspeedJob = new EnterspeedJob()
            {
                Id = int.Parse(record["Id"].ToString()),
                EntityId = record["EntityId"].ToString(),
                Culture = record["Culture"].ToString(),
                CreatedAt = DateTime.Parse(record["CreatedAt"].ToString()),
                Exception = record["Exception"].ToString(),
                UpdatedAt = DateTime.Parse(record["UpdatedAt"].ToString()),
                BuildHookUrls = record["BuildHookUrls"].ToString()
            };

            if (contentStateParsed)
            {
                enterspeedJob.ContentState = contentState;
            }

            if (jobTypeParsed)
            {
                enterspeedJob.JobType = jobType;
            }

            if (jobStateParsed)
            {
                enterspeedJob.State = state;
            }

            if (entityTypeParsed)
            {
                enterspeedJob.EntityType = entityType;
            }

            return enterspeedJob;
        }
    }
}