using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Logging;
using GrowCreate.PipelineCRM.Services;
using Umbraco.Core;

namespace GrowCreate.PipelineCRM.Installer
{
    public class AppPermissions
    {
        public static void Grant(string appName)
        {
            var us = ApplicationContext.Current.Services.UserService;
            var admin = us.GetByProviderKey(0);
            if (admin.AllowedSections.FindIndex(x => x == appName) <= 0)
            {
                admin.AddAllowedSection(appName);
                us.Save(admin);
            }
        }

        public static void Revoke()
        {            
        }
    }
}