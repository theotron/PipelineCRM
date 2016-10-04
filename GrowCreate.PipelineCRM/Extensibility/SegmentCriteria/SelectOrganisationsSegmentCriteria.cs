using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.Services;

namespace GrowCreate.PipelineCRM.SegmentCriteria
{
    public class SelectOrganisationsSegmentCriteria : ISegmentCriteria
    {
        public string Name => "Select organisations";

        public string Description => "List of manually picked organisations";

        public string ConfigDocType => "";

        public IEnumerable<Contact> GetContacts(int Id)
        {
            var segment = new SegmentService().GetById(Id);
            return new OrganisationService()
                .GetByIds(segment.OrganisationIds)
                .Select(x => new ContactService().GetByOrganisationId(x.Id))
                .SelectMany(x => x)
                .Distinct();
        }
    }
}