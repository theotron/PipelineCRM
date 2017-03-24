using GrowCreate.PipelineCRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowCreate.PipelineCRM.SegmentCriteria
{
    public interface ISegmentCriteria
    {
        string Name { get; }
        string Description { get; }
        string ConfigDocType { get; }
        IEnumerable<Contact> GetContacts(Segment segment);
        bool VisitorInSegment(Segment segment);
    }
}
