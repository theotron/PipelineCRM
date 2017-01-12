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
using Umbraco.Core.Services;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Newtonsoft.Json;
using GrowCreate.PipelineCRM.Services;
using GrowCreate.PipelineCRM.Config;
using System.Dynamic;

namespace GrowCreate.PipelineCRM.Controllers
{
    [PluginController("PipelineCRM")]
    public class CustomPropertyApiController : UmbracoAuthorizedApiController
    {
        public IEnumerable<CustomPropertyTab> GetCriteriaProps(string criteriaName)
        {
            var criteria = SegmentCriteriaService.GetSegmentCriteria().SingleOrDefault(x => x.Name == criteriaName);
            if (criteria != null)
            {
                return GetCustomProps(docTypeAlias: criteria.ConfigDocType);
            }
            return null;
        }

        public IEnumerable<CustomPropertyTab> GetCustomProps(string type = "", string docTypeAlias = "")
        {
            var outProps = new List<CustomPropertyTab>();

            // we need either param
            if (string.IsNullOrEmpty(docTypeAlias) && string.IsNullOrEmpty(type))
                return outProps;

            // get doc type from pipeline config            
            if (string.IsNullOrEmpty(docTypeAlias) && !string.IsNullOrEmpty(type))
            {
                var pipelineConfig = PipelineConfig.GetConfig().AppSettings;

                switch (type)
                {
                    case "contact":
                        docTypeAlias = pipelineConfig.ContactDocTypes;
                        break;
                    case "organisation":
                        docTypeAlias = pipelineConfig.OrganisationDocTypes;
                        break;
                    case "segment":
                        docTypeAlias = pipelineConfig.SegmentDocTypes;
                        break;
                    default:
                        docTypeAlias = pipelineConfig.OpportunityDocTypes;
                        break;
                }

                if (string.IsNullOrEmpty(docTypeAlias) || Services.ContentTypeService.GetContentType(docTypeAlias) == null)
                    return outProps;
            }

            // check there is such a doc type
            if (Services.ContentTypeService.GetContentType(docTypeAlias) == null)
                return outProps;

            // construct shadow doc type definition
            var tabs = Services.ContentTypeService.GetContentType(docTypeAlias).PropertyGroups.OrderBy(x => x.SortOrder);

            foreach (var tab in tabs)
            {
                var tabProps = new CustomPropertyTab()
                {
                    name = tab.Name.Contains('.') ? tab.Name.Split('.')[1] : tab.Name,
                    items = new List<CustomProperty>()
                };
                var props = tab.PropertyTypes.OrderBy(x => x.SortOrder);

                if (props.Any())
                {
                    foreach (var prop in props)
                    {
                        dynamic config = new ExpandoObject();
                        var prevalues = Services.DataTypeService.GetPreValuesCollectionByDataTypeId(prop.DataTypeDefinitionId).PreValuesAsDictionary;

                        if (prevalues.Any())
                        {
                            var items = new List<CustomPropertyPreValue>();
                            foreach (var preval in prevalues)
                            {
                                items.Add(new CustomPropertyPreValue()
                                {
                                    id = preval.Value.Id,
                                    alias = preval.Key,
                                    value = preval.Value.Value
                                });
                            }

                            config.items = items;

                            // marks as multiPicker if it has min/max number 
                            if (items.Any(x => x.alias == "minNumber" || x.alias == "maxNumber"))
                            {
                                config.multiPicker = "1";
                            }
                        }

                        var newProp = new CustomProperty()
                        {
                            id = prop.Id,
                            alias = prop.Alias,
                            label = prop.Name,
                            description = prop.Description,
                            view = PropertyEditorResolver.Current.GetByAlias(prop.PropertyEditorAlias).ValueEditor.View,
                            config = config
                        };
                        tabProps.items.Add(newProp);
                    }

                    outProps.Add(tabProps);
                }                
            }

            return outProps;
        }
    }
}