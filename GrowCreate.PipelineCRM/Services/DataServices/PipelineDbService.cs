using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GrowCreate.PipelineCRM.Services;
using GrowCreate.PipelineCRM.Models;

namespace GrowCreate.PipelineCRM.DataServices
{    
    public class PipelineDbService
    {
        public event EventHandler OnBeforeSave;
        public event EventHandler OnAfterSave;

        private static readonly PipelineDbService instance = new PipelineDbService();

        private PipelineDbService() { }

        public static PipelineDbService Instance
        {
            get 
            {
                return instance; 
            }
        }

        public Pipeline SavePipeline(Pipeline pipeline)
        {
            EventHandler presave = OnBeforeSave;
            if (null != presave) presave(pipeline, EventArgs.Empty);

            pipeline.DateUpdated = pipeline.DateUpdated > DateTime.MinValue ? pipeline.DateUpdated : DateTime.Now;

            if (pipeline.Id > 0)
            {
                DbService.db().Update(pipeline);
            }
            else
            {
                pipeline.DateCreated = pipeline.DateCreated > DateTime.MinValue ? pipeline.DateCreated : pipeline.DateUpdated;
                DbService.db().Save(pipeline);
            }

            EventHandler postsave = OnAfterSave;
            if (null != postsave) postsave(pipeline, EventArgs.Empty);

            return pipeline;
        }
    }
}