using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace GrowCreate.PipelineCRM.Models
{
    public class CustomPropertyTab
    {
        public List<CustomProperty> items { get; set; }
        public string name { get; set; }
    }

    public class CustomProperty
    {
        public int id { get; set; }
        public string alias { get; set; }
        public string label { get; set; }
        public string description { get; set; }
        public string view { get; set; }
        public string value { get; set; }
        public dynamic config { get; set; }
    }

    public class CustomPropertyConfig
    {
        public List<CustomPropertyPreValue> items { get; set; }
    }

    public class CustomPropertyPreValue
    {
        public int id { get; set; }
        public string value { get; set; }
        public string alias { get; set; }
    }
}