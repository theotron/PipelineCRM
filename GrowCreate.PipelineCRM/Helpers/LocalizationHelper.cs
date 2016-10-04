using System.Globalization;
using System.Threading;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;

namespace GrowCreate.PipelineCRM.Helpers
{
    public class LocalizationHelper
    {
        private static readonly string UmbracoDefaultUiLanguage = GlobalSettings.DefaultUILanguage;

        public static CultureInfo GetCultureFromUser(IUser user)
        {            
            return CultureInfo.GetCultureInfo(GetLanguage(user));
        }
        public static string GetLanguage(IUser u)
        {
            if (u != null)
            {
                return u.Language;
            }

            return GetLanguage(string.Empty);
        }
        private static string GetLanguage(string userLanguage)
        {
            if (userLanguage.IsNullOrWhiteSpace() == false)
            {
                return userLanguage;
            }

            var language = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            if (string.IsNullOrEmpty(language))
                language = UmbracoDefaultUiLanguage;
            return language;
        }
    }
}