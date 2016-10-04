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
    public class LabelApiController : UmbracoAuthorizedApiController
    {
        public IEnumerable<Label> GetAll()
        {
            return DbService.db().Query<Label>("select * from pipelineLabel");
        }

        public Label GetById(int id)
        {
            var query = new Sql().Select("*").From("pipelineLabel").Where<Label>(x => x.Id == id);
            return DbService.db().Fetch<Label>(query).FirstOrDefault();
        }

        public IEnumerable<Label> GetById(string ids)
        {
            var labels = new List<Label>();
            foreach (var id in ids.Split(','))
            {
                if (!labels.Where(x => x.Id == int.Parse(id)).Any())
                    labels.Add(GetById(int.Parse(id)));
            }
            return labels;
        }

    }
}