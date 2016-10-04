using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using GrowCreate.PipelineCRM.Models;
using Umbraco.Web.WebApi;
using GrowCreate.PipelineCRM.Services;

namespace GrowCreate.PipelineCRM.Controllers
{
    [PluginController("PipelineCRM")]
    public class StatusApiController : UmbracoAuthorizedApiController
    {
        public IEnumerable<Status> GetAll()
        {
            return DbService.db().Query<Status>("select * from pipelineStatus");
        }

        public Status GetById(int id)
        {
            var query = new Sql().Select("*").From("pipelineStatus").Where<Status>(x => x.Id == id);
            return DbService.db().Fetch<Status>(query).FirstOrDefault();
        }

        public IEnumerable<Status> GetOpen()
        {
            var query = new Sql().Select("*").From("pipelineStatus").Where<Status>(x => !x.Complete);
            return DbService.db().Fetch<Status>(query);
        }

        public Status PostSave(Status status)
        {
            LogHelper.Info(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, "Saving object:" + status);

            if (status.Id > 0)
                DbService.db().Update(status);
            else
                DbService.db().Save(status);

            return status;
        }

        public int DeleteById(int id)
        {
            return DbService.db().Delete<Status>(id);
        }

    }
}