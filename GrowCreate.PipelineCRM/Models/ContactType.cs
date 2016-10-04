using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace GrowCreate.PipelineCRM.Models
{
    [TableName("pipelineContactType")]
    public class ContactType
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Name { get; set; }
    }
}