using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Models.Membership;

namespace GrowCreate.PipelineCRM.Models
{
    public class QuickCreate
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public int Probability { get; set; }
        public int StatusId { get; set; }
        public string OrganisationName { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string Message { get; set; }
    }
}