using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using GrowCreate.PipelineCRM.Models;

namespace GrowCreate.PipelineCRM.Models
{
    [TableName("pipelineSegment")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class Segment
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Criteria { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int TypeId { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string ContactIds { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string OrganisationIds { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime DateCreated { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime DateUpdated { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public bool Archived { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string CustomProps { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string CriteriaProps { get; set; }

        [Ignore]
        public SegmentType SegmentType { get; set; }
        [Ignore]
        public IEnumerable<Contact> Contacts { get; set; }
        [Ignore]
        public IEnumerable<Organisation> Organisations { get; set; }
        [Ignore]
        public IEnumerable<Task> Tasks { get; set; }
        [Ignore]
        public IEnumerable<dynamic> Properties { get; set; }
        [Ignore]
        public IEnumerable<dynamic> CriteriaProperties { get; set; }
        [Ignore]
        public bool Obscured { get; set; }
    }

    public class PagedSegments
    {
        public IEnumerable<Segment> Segments { get; set; }
        public long CurrentPage { get; set; }
        public long ItemsPerPage { get; set; }
        public long TotalPages { get; set; }
        public long TotalItems { get; set; }
    }
}