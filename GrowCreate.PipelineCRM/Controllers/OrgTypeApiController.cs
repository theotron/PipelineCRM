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
    public class OrgTypeApiController : UmbracoAuthorizedApiController
    {
        public IEnumerable<OrgType> GetAll()
        {
            return DbService.db().Query<OrgType>("select * from pipelineOrganisationType");
        }

        public OrgType GetById(int id)
        {
            var query = new Sql().Select("*").From("pipelineOrganisationType").Where<OrgType>(x => x.Id == id);
            return DbService.db().Fetch<OrgType>(query).FirstOrDefault();

        }

        public OrgType PostSave(OrgType orgtype)
        {
            if (orgtype.Id > 0)
                DbService.db().Update(orgtype);
            else
                DbService.db().Save(orgtype);

            return orgtype;
        }

        public int DeleteById(int id)
        {
            return DbService.db().Delete<OrgType>(id);
        }

        [System.Web.Http.HttpGet]
        public bool EnabledOrganisations()
        {
            return false; // GrowCreate.PipelineCRM.Config.PipelineConfigurationOptions.GetConfig().PipelineConfiguration.PipelineEnableOrganisations;
        }

        [System.Web.Http.HttpGet]
        public bool CreateMembers()
        {
            return false; // GrowCreate.PipelineCRM.Config.PipelineConfigurationOptions.GetConfig().PipelineConfiguration.PipelineCreateMembers;
        }
    }
}