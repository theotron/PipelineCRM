using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Xml;
using Umbraco.Core.Logging;


namespace GrowCreate.PipelineCRM.Installer
{
    public class TranslationHelper
    {
        // Set the path of the language files directory
        private const string UmbracoLangPath = "~/umbraco/config/lang/";
        private const string PluginLangPath = "~/App_Plugins/PipelineCRM/Lang/";

        public static IEnumerable<FileInfo> GetUmbracoLanguageFiles()
        {
            var umbPath = HostingEnvironment.MapPath(UmbracoLangPath);
            var di = new DirectoryInfo(umbPath);
            return di.GetFiles("*.xml");
        }

        public static IEnumerable<FileInfo> GetPluginLanguageFiles()
        {
            var PluginPath = HostingEnvironment.MapPath(PluginLangPath);
            var di = new DirectoryInfo(PluginPath);
            return di.GetFiles("*.xml");
        }

        public static IEnumerable<FileInfo> GetUmbracoLanguageFilesToInsertLocalizationData()
        {
            return GetUmbracoLanguageFiles().Where(x => GetPluginLanguageFiles().Any(y => y.Name == x.Name));
        }

        public static void AddTranslations()
        {
            var PluginFiles = GetPluginLanguageFiles();
            
            //Convert to an array
            var PluginFileArray = PluginFiles as FileInfo[] ?? PluginFiles.ToArray();

            //Check which language filenames that we have match up
            var existingLangs = GetUmbracoLanguageFilesToInsertLocalizationData();
            
            //For each umbraco language file...
            foreach (var lang in existingLangs)
            {
                var Plugin = new XmlDocument() { PreserveWhitespace = true };
                var umb = new XmlDocument() { PreserveWhitespace = true };

                try
                {
                    //From our Plugin language file/s - try & find a file with the same name
                    var match = PluginFileArray.FirstOrDefault(x => x.Name == lang.Name);

                    //Ensure we have a match & not null
                    if (match != null)
                    {
                        //Load the two XML files
                        Plugin.LoadXml(File.ReadAllText(match.FullName));
                        umb.LoadXml(File.ReadAllText(lang.FullName));

                        //Get all of the <area>'s from Plugin XML file & their child elements
                        var areas = Plugin.DocumentElement.SelectNodes("//area");

                        //For each <area> in our XML...
                        foreach (XmlNode area in areas)
                        {
                            //Get the current area in this loop from our Plugin translation file - alias attribute
                            var aliasToTryFind = area.Attributes["alias"];

                            //Try and find <area> with same alias in the umbraco file
                            var findAreaInUmbracoLang = umb.SelectSingleNode(string.Format("//area [@alias='{0}']", aliasToTryFind.Value));

                            //Can not find <area> to import/merge in Umbraco lang file
                            if (findAreaInUmbracoLang == null)
                            {
                                //So let's just import the area and child keys
                                var import = umb.ImportNode(area, true);
                                umb.DocumentElement.AppendChild(import);
                            }
                            else
                            {
                                //We have found the <area> so don't just overwrite from what we have
                                //Ensure to go through each key and check we have it or not
                                foreach (XmlNode areaKey in area.ChildNodes)
                                {
                                    //Added as area.childNodes contained 3 items for one element - with 2 being WhiteSpace elements
                                    if (areaKey.NodeType == XmlNodeType.Element)
                                    {
                                        //Get the current area in this loop from our Plugin translation file - alias attribute
                                        var keyAliasToTryFind = areaKey.Attributes["alias"];

                                        //Try and find <key> is in the Umbraco XML lang doc
                                        var findKeyInUmbracoLang = findAreaInUmbracoLang.SelectSingleNode(string.Format("./key [@alias='{0}']", keyAliasToTryFind.Value));

                                        //Can not find <key> in Umbraco lang file - let's add it
                                        //And DO NOTHING if we do find it - don't want to overwrite it
                                        if (findKeyInUmbracoLang == null)
                                        {
                                            var keyImport = umb.ImportNode(areaKey, true);
                                            findAreaInUmbracoLang.AppendChild(keyImport);
                                        }
                                    }


                                }
                            }
                        }

                        //Save the umb lang file with the merged contents
                        umb.Save(lang.FullName);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error<TranslationHelper>("Failed to add Pipeline localization values to language file", ex);
                }

            }
        }

        public static void RemoveTranslations()
        {
            var PluginFiles = GetPluginLanguageFiles();

            //Convert to an array
            var PluginFileArray = PluginFiles as FileInfo[] ?? PluginFiles.ToArray();

            //Check which language filenames that we have match up
            var existingLangs = GetUmbracoLanguageFilesToInsertLocalizationData();

            //For each umbraco language file...
            foreach (var lang in existingLangs)
            {
                var Plugin = new XmlDocument() { PreserveWhitespace = true };
                var umb = new XmlDocument() { PreserveWhitespace = true };

                try
                {
                    //From our Plugin language file/s - try & find a file with the same name
                    var match = PluginFileArray.FirstOrDefault(x => x.Name == lang.Name);

                    //Ensure we have a match & not null
                    if (match != null)
                    {
                        //Load the two XML files
                        Plugin.LoadXml(File.ReadAllText(match.FullName));
                        umb.LoadXml(File.ReadAllText(lang.FullName));

                        //Get all of the <area>'s from Plugin XML file & their child elements
                        var areas = Plugin.DocumentElement.SelectNodes("//area");

                        //For each <area> in our XML...
                        foreach (XmlNode area in areas)
                        {
                            //Get the current area in this loop from our Plugin translation file - alias attribute
                            var aliasToTryFind = area.Attributes["alias"];

                            //Try and find <area> with same alias in the umbraco file
                            var findAreaInUmbracoLang =
                                umb.SelectSingleNode(string.Format("//area [@alias='{0}']", aliasToTryFind.Value));

                            //Found <area> with alias to remove from Umbraco lang file
                            if (findAreaInUmbracoLang != null)
                            {
                                //We have found the <area> so don't just REMOVE it entirely from what we have
                                //As may be 'treeHeaders' or 'sections' as the area which is core Umbraco
                                //Ensure to go through each key and check we have it or not
                                foreach (XmlNode areaKey in area.ChildNodes)
                                {
                                    //Get the current area in this loop from our Plugin translation file - alias attribute
                                    var keyAliasToTryFind = areaKey.Attributes["alias"];

                                    //Try and find <key> is in the Umbraco XML lang doc
                                    var findKeyInUmbracoLang = findAreaInUmbracoLang.SelectSingleNode(string.Format("//key [@alias='{0}']", keyAliasToTryFind.Value));

                                    //Can find <key> in Umbraco lang file - let's REMOVE it
                                    if (findKeyInUmbracoLang != null)
                                    {
                                        var keyToRemove = umb.ImportNode(areaKey, true);
                                        findAreaInUmbracoLang.RemoveChild(keyToRemove);
                                    }
                                }

                                //After looping through all - lets check we have no <key> left in them
                                if (!area.HasChildNodes)
                                {
                                    //No child nodes - so is our custom Plugin areas as opposed to Umbraco core ones 'treeHeaders' & 'sections'
                                    //Remove the area itself as it's empty
                                    var areaToRemove = umb.ImportNode(area, true);
                                    Plugin.DocumentElement.RemoveChild(areaToRemove);
                                }
                            }
                        }

                        //Save the umb lang file with the merged contents
                        umb.Save(lang.FullName);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error<TranslationHelper>("Failed to add Pipeline localization values to language file", ex);
                }
            }

        }
    }
}