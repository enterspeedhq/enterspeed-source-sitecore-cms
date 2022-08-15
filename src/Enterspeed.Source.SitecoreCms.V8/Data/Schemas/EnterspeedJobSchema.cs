using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.DynamicData;

namespace Enterspeed.Source.SitecoreCms.V8.Data.Schemas
{
    [TableName("EnterspeedJobs")]
    public class EnterspeedJobSchema
    {
        [Column("Id")]
        public int Id { get; set; }

        [Column("EntityId")]
        public string EntityId { get; set; }

        [Column("Culture")]
        public string Culture { get; set; }

        [Column("JobType")]
        public int JobType { get; set; }

        [Column("State")]
        public int JobState { get; set; }

        [Column("Exception")]
        public string Exception { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; }

        [Column("EntityType")]
        public int EntityType { get; set; }

        [Column("ContentState")]
        public int ContentState { get; set; }
    }
}
