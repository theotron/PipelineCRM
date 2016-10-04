using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models;

namespace GrowCreate.PipelineCRM.Models
{
    public class MediaFile
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
    }
}