using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace GrowCreate.PipelineCRM.Models
{
    [TableName("pipelinePreferences")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class Preferences
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public int UserId { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public bool ReceiveDigest { get; set; }
    }
}