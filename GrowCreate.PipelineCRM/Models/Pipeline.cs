using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Models.Membership;

namespace GrowCreate.PipelineCRM.Models
{
    [TableName("pipelinePipeline")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class Pipeline
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        public string Name { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public double Value { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public double Probability { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int StatusId { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int OrganisationId { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int ContactId { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string LabelIds { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime DateCreated { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime DateComplete { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime DateUpdated { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public bool Archived { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string CustomProps { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int UserId { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int SortOrder { get; set; }

        [Ignore]
        public Organisation Organisation { get; set; }
        [Ignore]
        public Contact Contact { get; set; }
        [Ignore]
        public Status Status { get; set; }
        [Ignore]
        public IEnumerable<Label> Labels { get; set; }
        [Ignore]
        public IEnumerable<Task> Tasks{ get; set; }
        [Ignore]
        public IUser User { get; set; }
        [Ignore]
        public IEnumerable<dynamic> Properties { get; set; }
        [Ignore]
        public string UserAvatar { get; set; }
        [Ignore]
        public string UserName { get; set; }

        [Ignore]
        public bool Obscured { get; set; } 
        
    }

    public class PagedPipelines
    {
        public IEnumerable<Pipeline> Pipelines { get; set; }
        public long CurrentPage { get; set; }
        public long ItemsPerPage { get; set; }
        public long TotalPages { get; set; }
        public long TotalItems { get; set; }
    }
}