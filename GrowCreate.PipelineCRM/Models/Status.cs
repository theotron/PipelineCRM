using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace GrowCreate.PipelineCRM.Models
{
    [TableName("pipelineStatus")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class Status
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        public string Name { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public bool Complete { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int Probability { get; set; }
    }
}