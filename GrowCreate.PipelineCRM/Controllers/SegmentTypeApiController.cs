using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Persistence;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using GrowCreate.PipelineCRM.Models;
using Umbraco.Web.WebApi;
using GrowCreate.PipelineCRM.Services;

namespace GrowCreate.PipelineCRM.Controllers
{
    [PluginController("PipelineCRM")]
    public class SegmentTypeApiController : UmbracoAuthorizedApiController
    {
        public IEnumerable<SegmentType> GetAll()
        {
            return DbService.db().Query<SegmentType>("select * from pipelineSegmentType");
        }

        public SegmentType GetById(int id)
        {
            var query = new Sql().Select("*").From("pipelineSegmentType").Where<SegmentType>(x => x.Id == id);
            return DbService.db().Fetch<SegmentType>(query).FirstOrDefault();

        }

        public SegmentType PostSave(SegmentType Segmenttype)
        {
            if (Segmenttype.Id > 0)
                DbService.db().Update(Segmenttype);
            else
                DbService.db().Save(Segmenttype);

            return Segmenttype;
        }

        public int DeleteById(int id)
        {
            return DbService.db().Delete<SegmentType>(id);
        }        
    }
}