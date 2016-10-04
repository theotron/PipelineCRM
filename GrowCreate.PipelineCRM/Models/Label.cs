using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace GrowCreate.PipelineCRM.Models
{
    [TableName("pipelineLabel")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class Label
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}