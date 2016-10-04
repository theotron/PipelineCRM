using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace GrowCreate.PipelineCRM.Models
{
    [TableName("pipelineOrganisation")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class Organisation
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        public string Name { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int UserId { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int TypeId { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string MemberIds { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Files { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Address { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Telephone { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Website { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Email { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime DateCreated { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime DateUpdated { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public bool Archived { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string CustomProps { get; set; }

        [Ignore]
        public List<MediaFile> Media { get; set; }
        [Ignore]
        public OrgType OrgType { get; set; }
        [Ignore]
        public IEnumerable<Task> Tasks { get; set; }
        [Ignore]
        public IEnumerable<Contact> Contacts { get; set; }
        [Ignore]
        public IEnumerable<Pipeline> Pipelines { get; set; }
        [Ignore]
        public IEnumerable<dynamic> Properties { get; set; }
        [Ignore]
        public bool Obscured { get; set; }
    }

    public class PagedOrganisations
    {
        public IEnumerable<Organisation> Organisations { get; set; }
        public long CurrentPage { get; set; }
        public long ItemsPerPage { get; set; }
        public long TotalPages { get; set; }
        public long TotalItems { get; set; }
    }
}