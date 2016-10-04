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
    public class ContactTypeApiController : UmbracoAuthorizedApiController
    {
        public IEnumerable<ContactType> GetAll()
        {
            return DbService.db().Query<ContactType>("select * from pipelineContactType");
        }

        public ContactType GetById(int id)
        {
            var query = new Sql().Select("*").From("pipelineContactType").Where<ContactType>(x => x.Id == id);
            return DbService.db().Fetch<ContactType>(query).FirstOrDefault();

        }

        public ContactType PostSave(ContactType contactType)
        {
            if (contactType.Id > 0)
                DbService.db().Update(contactType);
            else
                DbService.db().Save(contactType);

            return contactType;
        }

        public int DeleteById(int id)
        {
            return DbService.db().Delete<ContactType>(id);
        }
    }
}