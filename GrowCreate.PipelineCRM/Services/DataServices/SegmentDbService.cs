using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GrowCreate.PipelineCRM.Services;
using GrowCreate.PipelineCRM.Models;

namespace GrowCreate.PipelineCRM.DataServices
{    
    public class SegmentDbService
    {
        public event EventHandler OnBeforeSave;
        public event EventHandler OnAfterSave;

        private static readonly SegmentDbService instance = new SegmentDbService();

        private SegmentDbService() { }

        public static SegmentDbService Instance
        {
            get 
            {
                return instance; 
            }
        }

        public Segment SaveSegment(Segment Segment)
        {
            EventHandler presave = OnBeforeSave;
            if (null != presave) presave(Segment, EventArgs.Empty);

            Segment.DateUpdated = DateTime.Now;

            if (Segment.Id > 0)
            {
                DbService.db().Update(Segment);
            }
            else
            {
                Segment.DateCreated = Segment.DateUpdated;
                DbService.db().Save(Segment);
            }

            EventHandler postsave = OnAfterSave;
            if (null != postsave) postsave(Segment, EventArgs.Empty);

            return Segment;
        }
    }
}