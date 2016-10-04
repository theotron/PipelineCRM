using Newtonsoft.Json.Linq;
using GrowCreate.PipelineCRM.Config;
using GrowCreate.PipelineCRM.Controllers;
using GrowCreate.PipelineCRM.DataServices;
using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using Umbraco.Core;

namespace GrowCreate.PipelineCRM.Services
{
    public class OrganisationService
    {        
        public Organisation GetById(int Id, bool getLinks = true)
        {
            var query = new Sql().Select("*").From("pipelineOrganisation").Where<Organisation>(x => x.Id == Id);
            var Organisation = DbService.db().Fetch<Organisation>(query).FirstOrDefault();            

            if (getLinks && Organisation != null && !Organisation.Obscured)
            {
                return GetLinks(Organisation);
            }
            return Organisation;
        }

        public IEnumerable<Organisation> GetByIds(string Ids = "")
        {
            if (!String.IsNullOrEmpty(Ids) && Ids.Split(',').Count() > 0)
            {
                int[] idList = Ids.Split(',').Select(int.Parse).ToArray();
                var query = new Sql("select * from pipelineOrganisation where Id in (@idList)", new { idList }); //.Select("*").From("pipelineOrganisation").Where<Organisation>(x => Ids.Split(',').Contains(x.Id.ToString()));
                return DbService.db().Fetch<Organisation>(query);
            }
            return new List<Organisation>();
        }

        public IEnumerable<Organisation> GetAll(bool getLinks = false)
        {
            var query = new Sql().Select("*").From("pipelineOrganisation").Where<Organisation>(x => !x.Archived);
            var Organisations = DbService.db().Fetch<Organisation>(query);
            if (getLinks)
            {
                for (int i = 0; i < Organisations.Count(); i++)
                {
                    Organisations[i] = GetLinks(Organisations[i]);
                }
            }

            return Organisations;
        }

        public PagedOrganisations GetPagedOrganisations(int pageNumber, string sortColumn, string sortOrder, string searchTerm, int typeId)
        {
            int itemsPerPage = PipelineConfig.GetConfig().AppSettings.PageSize;
            var items = new List<Organisation>();
            var OrganisationType = typeof(Organisation);

            var query = new Sql().Select("*").From("pipelineOrganisation");

            if (typeId == 0)
            {
                query.Append(" where Archived=0 ", typeId);
            }
            else if (typeId == -1)
            {
                query.Append(" where Archived=1 ", typeId);
            }
            else if (typeId == -2)
            {
                query.Append(" where TypeId=0 and Archived=0 ", typeId);
            }
            else 
            {
                query.Append(" where TypeId=@0 and Archived=0 ", typeId);                
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query.Append(" and (Name like @0 or Address like @0 or Website like @0 or Email like @0) ", "%" + searchTerm + "%");                
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortOrder))
            {
                query.OrderBy(sortColumn + " " + sortOrder);
            }
            else
            {
                query.OrderBy("Name asc");
            }

            var p = DbService.db().Page<Organisation>(pageNumber, itemsPerPage, query);
            return new PagedOrganisations
            {
                TotalPages = p.TotalPages,
                TotalItems = p.TotalItems,
                ItemsPerPage = p.ItemsPerPage,
                CurrentPage = p.CurrentPage,
                Organisations = p.Items.ToList()
            };            
        }  
              
        public IEnumerable<Organisation> GetByTypeId(int TypeId, bool getLinks = true)
        {
            var query = new Sql().Select("*").From("pipelineOrganisation").Where<Organisation>(x => x.TypeId == TypeId);
            var Organisations = DbService.db().Fetch<Organisation>(query);
            if (getLinks)
            {
                for (int i = 0; i < Organisations.Count(); i++)
                {
                    Organisations[i] = GetLinks(Organisations[i]);
                }
            }
            return Organisations;
        }

        public Organisation Save(Organisation Organisation)
        {
            return OrganisationDbService.Instance.SaveOrganisation(Organisation);
        }

        public int Delete(int OrganisationId)
        {
            return DbService.db().Delete<Organisation>(OrganisationId);
        }

        public Organisation GetLinks(Organisation org)
        {
            org.Tasks = new TaskApiController().GetByOrganisationId(org.Id).OrderBy(x => x.Overdue).OrderBy(x => x.DateDue);
            org.Pipelines = new PipelineApiController().GetByOrganisationId(org.Id);
            org.OrgType = new OrgTypeApiController().GetById(org.TypeId);

            if (!String.IsNullOrEmpty(org.Files) && org.Files.Split(',').Count() > 0)
            {
                var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                var media = umbracoHelper.TypedMedia(org.Files.Split(','));
                var mediaList = new List<MediaFile>();

                foreach (var file in media)
                {
                    var mediaFile = new MediaFile()
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Url = file.Url
                    };
                    mediaList.Add(mediaFile);
                }

                org.Media = mediaList;
            }
            return org;
        }

    }

    public static class OrganisationExtensions
    {
        public static IEnumerable<dynamic> GetProperties(this Organisation Organisation)
        {
            if (!String.IsNullOrEmpty(Organisation.CustomProps))
                return Newtonsoft.Json.Linq.JArray.Parse(Organisation.CustomProps) as IEnumerable<dynamic>;
            else
                return new List<dynamic>();
        }

        public static dynamic GetProperty(this Organisation Organisation, string alias)
        {
            var props = Organisation.GetProperties();
            return props.Where(x => x.alias.ToString().ToLower() == alias.ToLower()).FirstOrDefault();
        }

        public static dynamic GetPropertyValue(this Organisation Organisation, string alias)
        {
            var prop = Organisation.GetProperty(alias);
            if (prop != null && prop.value != null)
            {
                //todo: how about strongly-typed results?
                return prop.value;
            }
            return null;
        }

        public static Organisation UpdateProperties(this Organisation Organisation, Dictionary<string, dynamic> updates)
        {
            var props = Organisation.GetProperties().ToList();
            foreach (var update in updates)
            {
                var prop = props.Where(x => x.alias.ToString().ToLower() == update.Key.ToLower()).FirstOrDefault();

                if (prop == null)
                {
                    // get doc type from pipeline config
                    string alias = update.Key;
                    var pipelineConfig = PipelineConfig.GetConfig().AppSettings;
                    var docType = ApplicationContext.Current.Services.ContentTypeService.GetContentType(pipelineConfig.OrganisationDocTypes);

                    if (docType != null)
                    {
                        var type = docType.PropertyTypes.SingleOrDefault(x => x.Alias.ToLower() == alias.ToLower());
                        if (type != null)
                        {
                            var newProp = new
                            {
                                alias = alias,
                                value = update.Value,
                                id = type.Id
                            };
                            props.Add(newProp);
                        }
                    }
                }
                else
                {
                    prop.value = update.Value;
                }
            }
            Organisation.CustomProps = Newtonsoft.Json.JsonConvert.SerializeObject(props);
            return Organisation;
        }

        public static Organisation UpdateArrayProperties(this Organisation Organisation, Dictionary<string, string[]> updates)
        {
            var props = Organisation.GetProperties().ToList();
            foreach (var update in updates)
            {
                var prop = props.Where(x => x.alias.ToString().ToLower() == update.Key.ToLower()).FirstOrDefault();
                JArray jValue = JArray.FromObject(update.Value);

                if (prop == null)
                {
                    // get doc type from pipeline config
                    string alias = update.Key;
                    var pipelineConfig = PipelineConfig.GetConfig().AppSettings;
                    var docType = ApplicationContext.Current.Services.ContentTypeService.GetContentType(pipelineConfig.OrganisationDocTypes);

                    if (docType != null)
                    {
                        var type = docType.PropertyTypes.SingleOrDefault(x => x.Alias.ToLower() == alias.ToLower());
                        if (type != null)
                        {
                            var newProp = new
                            {
                                alias = alias,
                                value = jValue,
                                id = type.Id
                            };
                            props.Add(newProp);
                        }
                    }
                }
                else
                {
                    prop.value = jValue;
                }
            }
            Organisation.CustomProps = Newtonsoft.Json.JsonConvert.SerializeObject(props);
            return Organisation;
        }

        public static Organisation UpdateProperty(this Organisation Organisation, string alias, dynamic value)
        {
            var props = Organisation.GetProperties().ToList();
            var prop = props.Where(x => x.alias.ToString().ToLower() == alias.ToLower()).FirstOrDefault();

            if (prop == null)
            {
                var pipelineConfig = PipelineConfig.GetConfig().AppSettings;
                var docType = ApplicationContext.Current.Services.ContentTypeService.GetContentType(pipelineConfig.OrganisationDocTypes);
                var typeId = docType.PropertyTypes.FirstOrDefault(x => x.Alias.ToLower() == alias.ToLower()).Id;

                var newProp = new
                {
                    alias = alias,
                    value = value,
                    id = typeId
                };
                props.Add(newProp);
            }
            else
            {
                props.Where(x => x.alias.ToString().ToLower() == alias.ToLower()).FirstOrDefault().value = value;
            }

            Organisation.CustomProps = Newtonsoft.Json.JsonConvert.SerializeObject(props);
            return Organisation;
        }

        public static Organisation Save(this Organisation Organisation)
        {
            return new OrganisationService().Save(Organisation);
        }

        public static void Delete(this Organisation Organisation)
        {
            new OrganisationService().Delete(Organisation.Id);
        }

    }

}