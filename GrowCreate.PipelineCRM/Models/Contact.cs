using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace GrowCreate.PipelineCRM.Models
{
    [TableName("pipelineContact")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class Contact
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int MemberId { get; set; }
        public int TypeId { get; set; }
        
        public string Name { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Title { get; set; }

        public string Email { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Telephone { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Mobile { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Files { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string OrganisationIds { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string CustomProps { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime DateCreated { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime DateUpdated { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public bool Archived { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public bool DisablePortal { get; set; }

        [Ignore]
        public List<MediaFile> Media { get; set; }
        [Ignore]
        public IEnumerable<Task> Tasks { get; set; }
        [Ignore]
        public IEnumerable<Organisation> Organisations { get; set; }
        [Ignore]
        public IEnumerable<string> OrganisationNames { get; set; }
        [Ignore]
        public IMember Member { get; set; }
        [Ignore]
        public IEnumerable<dynamic> Properties { get; set; }
        [Ignore]
        public ContactType Type { get; set; }

        [Ignore]
        public bool Obscured { get; set; }
    }

    public class PagedContacts
    {
        public IEnumerable<Contact> Contacts { get; set; }
        public long CurrentPage { get; set; }
        public long ItemsPerPage { get; set; }
        public long TotalPages { get; set; }
        public long TotalItems { get; set; }
    }

}