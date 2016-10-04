using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Models.Membership;

namespace GrowCreate.PipelineCRM.Models
{
    [TableName("pipelineTask")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class Task
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Description { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public bool Done { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int OrganisationId { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int PipelineId { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int ContactId { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int SegmentId { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int UserId { get; set; }

        public DateTime DateCreated { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime DateDue { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime DateComplete { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime DateUpdated { get; set; }
        
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime Reminder { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Type { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Files { get; set; }

        
        [Ignore]
        public int Overdue { get; set; }        
        
        [Ignore]
        public Organisation Organisation { get; set; }

        [Ignore]
        public Contact Contact { get; set; }

        [Ignore]
        public Segment Segment { get; set; }

        [Ignore]
        public Pipeline Pipeline { get; set; }

        [Ignore]
        public List<MediaFile> Media { get; set; }

        [Ignore]
        public string UserAvatar { get; set; }

        [Ignore]
        public string UserName { get; set; }

        [Ignore]
        public int ParentId { get; set; }
 
        [Ignore]
        public string ParentName { get; set; }
    }
}