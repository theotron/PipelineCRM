using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.Services;

namespace GrowCreate.PipelineCRM.SegmentCriteria
{
    public class SelectContactsSegmentCriteria : ISegmentCriteria
    {
        public string Name => "Select contacts";

        public string Description => "List of manually picked contacts";

        public string ConfigDocType => "";

        public IEnumerable<Contact> GetContacts(int Id)
        {
            var segment = new SegmentService().GetById(Id);
            return new ContactService().GetByIds(segment.ContactIds);
        }
    }
}