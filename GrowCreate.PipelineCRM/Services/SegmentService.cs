using Newtonsoft.Json.Linq;
using GrowCreate.PipelineCRM.Config;
using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.DataServices;
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
    public class SegmentService
    {        
        public Segment GetById(int Id, bool getLinks = true)
        {
            var query = new Sql().Select("*").From("pipelineSegment").Where<Segment>(x => x.Id == Id);
            var Segment = DbService.db().Fetch<Segment>(query).FirstOrDefault();
            return Segment;
        }

        public IEnumerable<Segment> GetAll(bool getLinks = false)
        {
            var query = new Sql().Select("*").From("pipelineSegment").Where<Segment>(x => !x.Archived);
            var Segments = DbService.db().Fetch<Segment>(query);
            return Segments;
        }

        public PagedSegments GetPagedSegments(int pageNumber, string sortColumn, string sortOrder, string searchTerm, int typeId)
        {
            int itemsPerPage = PipelineConfig.GetConfig().AppSettings.PageSize;
            var items = new List<Segment>();
            var SegmentType = typeof(Segment);

            var query = new Sql().Select("*").From("pipelineSegment");

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
                query.Append(" and (Name like @0) ", "%" + searchTerm + "%");                
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortOrder))
            {
                query.OrderBy(sortColumn + " " + sortOrder);
            }
            else
            {
                query.OrderBy("Name asc");
            }

            var p = DbService.db().Page<Segment>(pageNumber, itemsPerPage, query);
            return new PagedSegments
            {
                TotalPages = p.TotalPages,
                TotalItems = p.TotalItems,
                ItemsPerPage = p.ItemsPerPage,
                CurrentPage = p.CurrentPage,
                Segments = p.Items.ToList()
            };            
        }  
        
        public IEnumerable<Contact> GetSegmentContacts(Segment segment)
        {
            if (segment.Criteria.ToLower() == "Select contacts")
            {
                return new ContactService().GetByIds(segment.ContactIds);
            }
            if (segment.Criteria.ToLower() == "Select organisations")
            {
                var organisations = new OrganisationService().GetByIds(segment.OrganisationIds);
                return organisations.Select(x => new ContactService().GetByOrganisationId(x.Id)).SelectMany(x => x).Distinct();
            }
            return Enumerable.Empty<Contact>();
        }            

        public Segment Save(Segment Segment)
        {
            return SegmentDbService.Instance.SaveSegment(Segment);
        }

        public int Delete(int SegmentId)
        {
            return DbService.db().Delete<Segment>(SegmentId);
        }
    }

    public static class SegmentExtensions
    {
        public static IEnumerable<dynamic> GetProperties(this Segment Segment)
        {
            if (!String.IsNullOrEmpty(Segment.CustomProps))
                return Newtonsoft.Json.Linq.JArray.Parse(Segment.CustomProps) as IEnumerable<dynamic>;
            else
                return new List<dynamic>();
        }

        public static dynamic GetProperty(this Segment Segment, string alias)
        {
            var props = Segment.GetProperties();
            return props.Where(x => x.alias.ToString().ToLower() == alias.ToLower()).FirstOrDefault();
        }

        public static dynamic GetPropertyValue(this Segment Segment, string alias)
        {
            var prop = Segment.GetProperty(alias);
            if (prop != null && prop.value != null)
            {
                //todo: how about strongly-typed results?
                return prop.value;
            }
            return null;
        }

        public static Segment UpdateProperties(this Segment Segment, Dictionary<string, dynamic> updates)
        {
            var props = Segment.GetProperties().ToList();
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
            Segment.CustomProps = Newtonsoft.Json.JsonConvert.SerializeObject(props);
            return Segment;
        }

        public static Segment UpdateArrayProperties(this Segment Segment, Dictionary<string, string[]> updates)
        {
            var props = Segment.GetProperties().ToList();
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
            Segment.CustomProps = Newtonsoft.Json.JsonConvert.SerializeObject(props);
            return Segment;
        }

        public static Segment UpdateProperty(this Segment Segment, string alias, dynamic value)
        {
            var props = Segment.GetProperties().ToList();
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

            Segment.CustomProps = Newtonsoft.Json.JsonConvert.SerializeObject(props);
            return Segment;
        }

        public static Segment Save(this Segment Segment)
        {
            return new SegmentService().Save(Segment);
        }

        public static void Delete(this Segment Segment)
        {
            new SegmentService().Delete(Segment.Id);
        }

    }

}