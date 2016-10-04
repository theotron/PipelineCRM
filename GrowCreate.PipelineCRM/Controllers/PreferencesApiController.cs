using System.Linq;
using Umbraco.Core.Persistence;
using Umbraco.Web.Mvc;
using GrowCreate.PipelineCRM.Models;
using Umbraco.Web.WebApi;
using GrowCreate.PipelineCRM.Services;
using System.Collections.Generic;
using GrowCreate.PipelineCRM.Config;

namespace GrowCreate.PipelineCRM.Controllers
{
    [PluginController("PipelineCRM")]
    public class PreferencesApiController : UmbracoAuthorizedApiController
    {
        public Preferences GetUserPreferences()
        {
            var userId = Security.CurrentUser.Id;
            var query = new Sql().Select("*").From("pipelinePreferences").Where<Preferences>(x => x.UserId == userId);
            var pref = DbService.db().Fetch<Preferences>(query).FirstOrDefault();

            if (pref == null)
            {
                pref = PostSave(new Preferences()
                {
                    UserId = userId
                });
            }

            return pref;
        }

        // method to get config values needed by the UI
        public Dictionary<string, string> GetConfig()
        {
            var settings = PipelineConfig.GetConfig().AppSettings;
            var config = new Dictionary<string, string>();

            config["UseBoard"] = settings.UseBoard ? "true" : "false";

            return config;
        }

        public Preferences PostSave(Preferences preferences)
        {
            if (preferences.Id > 0)
                DbService.db().Update(preferences);
            else
                DbService.db().Save(preferences);

            return preferences;
        }
    }
}