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
using System.Linq.Expressions;

namespace GrowCreate.PipelineCRM.Services
{
    public class ContactService
    {        
        public Contact GetById(int Id, bool getLinks = true)
        {
            var query = new Sql().Select("*").From("pipelineContact").Where<Contact>(x => x.Id == Id);
            var contact = DbService.db().Fetch<Contact>(query).FirstOrDefault();            

            if (getLinks && contact != null && !contact.Obscured)
            {
                return GetLinks(contact);
            }
            return contact;
        }

        public IEnumerable<Contact> GetByIds(string Ids = "")
        {
            if (!String.IsNullOrEmpty(Ids) && Ids.Split(',').Count() > 0)
            {
                int[] idList = Ids.Split(',').Select(int.Parse).ToArray();
                var query = new Sql("select * from pipelineContact where Id in (@idList)", new { idList });
                return DbService.db().Fetch<Contact>(query);
            }
            return new List<Contact>();
        }

        public Contact GetByEmail(string Email, bool getLinks = true)
        {
            var query = new Sql().Select("*").From("pipelineContact").Where<Contact>(x => x.Email == Email);
            var contact = DbService.db().Fetch<Contact>(query).FirstOrDefault();            

            if (getLinks && contact != null && !contact.Obscured)
            {
                return GetLinks(contact);
            }
            return contact;
        }

        public IEnumerable<Contact> GetAll(bool getLinks = false)
        {
            var query = new Sql().Select("*").From("pipelineContact").Where<Contact>(x => !x.Archived);
            var contacts = DbService.db().Fetch<Contact>(query);
            if (getLinks)
            {
                for (int i = 0; i < contacts.Count(); i++)
                {
                    contacts[i] = GetLinks(contacts[i]);
                }
            }

            return contacts;
        }

        public PagedContacts GetPagedContacts(int pageNumber, string sortColumn, string sortOrder, string searchTerm, int typeId)
        {
            int itemsPerPage = PipelineConfig.GetConfig().AppSettings.PageSize;
            var items = new List<Contact>();
            var contactType = typeof(Contact);

            var query = new Sql().Select("*").From("pipelineContact");

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
                query.Append(" and (Name like @0 or Email like @0) ", "%" + searchTerm + "%");                
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortOrder) && sortColumn != "OrganisationNames")
            {
                query.OrderBy(sortColumn + " " + sortOrder);
            }
            else
            {
                query.OrderBy("Name asc");
            }

            var p = DbService.db().Page<Contact>(pageNumber, itemsPerPage, query);

            for (int i = 0; i < p.Items.ToList().Count(); i++)
            {
                p.Items[i].Organisations = new OrganisationApiController().GetByIds(p.Items[i].OrganisationIds);
                p.Items[i].OrganisationNames = p.Items[i].Organisations.Select(x => x.Name).OrderBy(x => x);
            }

            // special sorting for organisations
            if (sortColumn == "OrganisationNames")
            {
                p.Items = sortOrder.ToLower() != "desc" ?
                    p.Items.OrderBy(x => x.OrganisationNames.FirstOrDefault()).ToList() : p.Items.OrderByDescending(x => x.OrganisationNames.FirstOrDefault()).ToList();
            }

            return new PagedContacts
            {
                TotalPages = p.TotalPages,
                TotalItems = p.TotalItems,
                ItemsPerPage = p.ItemsPerPage,
                CurrentPage = p.CurrentPage,
                Contacts = p.Items.ToList()
            };            
        }

        public IEnumerable<Contact> GetByOrganisationId(int OrganisationId, bool getLinks = true)
        {
            var contacts = DbService.db().Fetch<Contact>(string.Format("SELECT * FROM [dbo].[pipelineContact] WHERE OrganisationIds IS NOT NULL AND OrganisationIds = '{0}' OR OrganisationIds LIKE '%{0},%' OR OrganisationIds LIKE '%,{0},%'", OrganisationId.ToString()));
            if (getLinks)
            {
                for (int i = 0; i < contacts.Count(); i++)
                {
                    contacts[i] = GetLinks(contacts[i]);
                }
            }
            return contacts;
        }

        public IEnumerable<Contact> GetByTypeId(int TypeId, bool getLinks = true)
        {
            var query = new Sql().Select("*").From("pipelineContact").Where<Contact>(x => x.TypeId == TypeId);
            var contacts = DbService.db().Fetch<Contact>(query);
            if (getLinks)
            {
                for (int i = 0; i < contacts.Count(); i++)
                {
                    contacts[i] = GetLinks(contacts[i]);
                }
            }
            return contacts;
        }

        public Contact Save(Contact contact)
        {
            return ContactDbService.Instance.SaveContact(contact);
        }

        public int Delete(int ContactId)
        {
            return DbService.db().Delete<Contact>(ContactId);
        }

        public Contact GetLinks(Contact contact)
        {
            if (!String.IsNullOrEmpty(contact.OrganisationIds))
            {
                contact.Organisations = new OrganisationApiController().GetByIds(contact.OrganisationIds);
            }
            contact.Tasks = new TaskApiController().GetByContactId(contact.Id);
            contact.Type = new ContactTypeApiController().GetById(contact.TypeId);

            if (!String.IsNullOrEmpty(contact.Files) && contact.Files.Split(',').Count() > 0)
            {
                var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                var media = umbracoHelper.TypedMedia(contact.Files.Split(','));
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

                contact.Media = mediaList;
            }

            return contact;
        }

    }

    public static class ContactExtensions
    {
        public static IEnumerable<dynamic> GetProperties(this Contact contact)
        {
            if (!String.IsNullOrEmpty(contact.CustomProps))
                return Newtonsoft.Json.Linq.JArray.Parse(contact.CustomProps) as IEnumerable<dynamic>;
            else
                return new List<dynamic>();
        }

        public static dynamic GetProperty(this Contact contact, string alias)
        {
            var props = contact.GetProperties();
            return props.Where(x => x.alias.ToString().ToLower() == alias.ToLower()).FirstOrDefault();
        }

        public static dynamic GetPropertyValue(this Contact contact, string alias)
        {
            var prop = contact.GetProperty(alias);
            if (prop != null && prop.value != null)
            {
                //todo: how about strongly-typed results?
                return prop.value;
            }
            return null;
        }

        public static Contact UpdateProperties(this Contact contact, Dictionary<string, dynamic> updates)
        {
            var props = contact.GetProperties().ToList();
            foreach (var update in updates)
            {
                var prop = props.Where(x => x.alias.ToString().ToLower() == update.Key.ToLower()).FirstOrDefault();

                if (prop == null)
                {
                    // get doc type from pipeline config
                    string alias = update.Key;
                    var pipelineConfig = PipelineConfig.GetConfig().AppSettings;
                    var docType = ApplicationContext.Current.Services.ContentTypeService.GetContentType(pipelineConfig.ContactDocTypes);

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
            contact.CustomProps = Newtonsoft.Json.JsonConvert.SerializeObject(props);
            return contact;
        }

        public static Contact UpdateArrayProperties(this Contact contact, Dictionary<string, string[]> updates)
        {
            var props = contact.GetProperties().ToList();
            foreach (var update in updates)
            {
                var prop = props.Where(x => x.alias.ToString().ToLower() == update.Key.ToLower()).FirstOrDefault();
                JArray jValue = JArray.FromObject(update.Value);

                if (prop == null)
                {
                    // get doc type from pipeline config
                    string alias = update.Key;
                    var pipelineConfig = PipelineConfig.GetConfig().AppSettings;
                    var docType = ApplicationContext.Current.Services.ContentTypeService.GetContentType(pipelineConfig.ContactDocTypes);

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
            contact.CustomProps = Newtonsoft.Json.JsonConvert.SerializeObject(props);
            return contact;
        }

        public static Contact UpdateProperty(this Contact contact, string alias, dynamic value)
        {
            var props = contact.GetProperties().ToList();
            var prop = props.Where(x => x.alias.ToString().ToLower() == alias.ToLower()).FirstOrDefault();

            if (prop == null)
            {
                var pipelineConfig = PipelineConfig.GetConfig().AppSettings;
                var docType = ApplicationContext.Current.Services.ContentTypeService.GetContentType(pipelineConfig.ContactDocTypes);
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

            contact.CustomProps = Newtonsoft.Json.JsonConvert.SerializeObject(props);
            return contact;
        }

        public static Contact Save(this Contact contact)
        {
            return new ContactService().Save(contact);
        }

        public static void Delete(this Contact contact)
        {
            new ContactService().Delete(contact.Id);
        }

    }

}