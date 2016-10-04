using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace GrowCreate.PipelineCRM.Controllers
{
    [PluginController("PipelineCRM")]
    public class PipelineSettingsController : UmbracoAuthorizedApiController
    {
        //[System.Web.Http.HttpGet]
        //public bool CreateMembers()
        //{
        //    return Config.Pipeline.GetConfig().AppSettingsElement.PipelineCreateMembers;
        //}

        //[System.Web.Http.HttpGet]
        //public string ContactDocTypes()
        //{
        //    return Config.Pipeline.GetConfig().AppSettingsElement.ContactDocTypes;
        //}
    }
}