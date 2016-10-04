using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web.Mvc;
using GrowCreate.PipelineCRM.Models;
using Umbraco.Web.WebApi;
using GrowCreate.PipelineCRM.Services;
using GrowCreate.PipelineCRM.DataServices;

namespace GrowCreate.PipelineCRM.Controllers
{
    [PluginController("PipelineCRM")]
    public class OrganisationApiController : UmbracoAuthorizedApiController
    {
        private Organisation GetOrgLinks(Organisation org)
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

        public IEnumerable<Organisation> GetAll(bool getLinks = false)
        {
            var query = new Sql().Select("*").From("pipelineOrganisation").Where<Organisation>(x => !x.Archived);
            var orgs = DbService.db().Fetch<Organisation>(query);
            if (getLinks)
            {
                for (int i = 0; i < orgs.Count(); i++)
                {
                    orgs[i] = GetOrgLinks(orgs[i]);
                }
            }
            return orgs.OrderBy(x => x.Name);
        }

        public PagedOrganisations GetPaged(int pageNumber = 0, string sortColumn = "", string sortOrder = "", string searchTerm = "", int typeId = 0)
        {
            return new OrganisationService().GetPagedOrganisations(pageNumber, sortColumn, sortOrder, searchTerm, typeId);
        }

        public IEnumerable<Organisation> GetUnassigned(bool getLinks = false)
        {
            var query = new Sql().Select("*").From("pipelineOrganisation").Where<Organisation>(x => !x.Archived && x.TypeId == 0);
            var orgs = DbService.db().Fetch<Organisation>(query);
            if (getLinks)
            {
                for (int i = 0; i < orgs.Count(); i++)
                {
                    orgs[i] = GetOrgLinks(orgs[i]);
                }
            }
            return orgs.OrderBy(x => x.Name);
        }
        
        public IEnumerable<Organisation> GetAllUsed(bool getLinks = false)
        {
            var contacts = new ContactApiController().GetAll().Where(x => !string.IsNullOrEmpty(x.OrganisationIds) && x.OrganisationIds != "0");
            var orgs = string.Join(",", contacts.Select(x => x.OrganisationIds));
            var orgIds = string.Join(",", orgs.Split(',').Where(x => !string.IsNullOrEmpty(x)).Distinct());

            return GetByIds(orgIds);
        }

        public Organisation GetById(int id, bool getLinks = true)
        {
            var query = new Sql().Select("*").From("pipelineOrganisation").Where<Organisation>(x => x.Id == id);
            var org = DbService.db().Fetch<Organisation>(query).FirstOrDefault();            
            
            if (getLinks && org!= null && !org.Obscured)
            {
                org.Contacts = new ContactApiController().GetByOrganisationId(org.Id);
                org = GetOrgLinks(org);
            }
            return org;
        }

        public IEnumerable<Organisation> GetByIds(string Ids = "")
        {
            return new OrganisationService().GetByIds(Ids);
        }

        public IEnumerable<Organisation> GetByMemberId(int id)
        {
            var query = new Sql().Select("*").From("pipelineOrganisation").Where<Organisation>(x => x.MemberIds.Contains(id.ToString()));
            return DbService.db().Fetch<Organisation>(query);
        }

        public IEnumerable<Organisation> GetByTypeId(int id, bool getLinks = false)
        {
            var query = new Sql().Select("*").From("pipelineOrganisation").Where<Organisation>(x => x.TypeId == id).Where<Organisation>(x => !x.Archived);
            var orgs = DbService.db().Fetch<Organisation>(query);
            if (getLinks)
            {
                for (int i = 0; i < orgs.Count(); i++)
                {
                    orgs[i] = GetOrgLinks(orgs[i]);
                }
            }
            return orgs;
        }

        public Organisation GetByName(string name)
        {
            var query = new Sql().Select("*").From("pipelineOrganisation").Where<Organisation>(x => x.Name.ToLower() == name.ToLower());
            var org = DbService.db().Fetch<Organisation>(query).FirstOrDefault();
            return org;
        }

        public IEnumerable<Organisation> GetOrganisationsByName(string name)
        {
            var query = new Sql().Select("*").From("pipelineOrganisation").Where<Organisation>(x => x.Name.ToLower().Contains(name.ToLower()));
            return DbService.db().Fetch<Organisation>(query);
        }

        public Organisation PostSave(Organisation org)
        {
            org = OrganisationDbService.Instance.SaveOrganisation(org);
            org.Contacts = new ContactApiController().GetByOrganisationId(org.Id);
            return org;
        }

        public IEnumerable<Organisation> PostSaveOrganisations(IEnumerable<Organisation> orgs)
        {
            foreach (var org in orgs)
            {
                PostSave(org);
            }
            return orgs;
        }

        public int DeleteById(int id, bool deleteLinks = false)
        {
            var org = GetById(id, true);
            if (org != null)
            {
                if (deleteLinks)
                {
                    if (org.Pipelines.Any())
                    {
                        new PipelineApiController().DeletePipelines(org.Pipelines);
                    }
                    /*if (org.Contacts.Any())
                    {
                        new ContactApiController().DeleteContacts(org.Contacts);
                    }*/
                }

                new TaskApiController().DeleteTasks(org.Tasks);
                return DbService.db().Delete<Organisation>(id);
            }
            return 0;
        }


        public void DeleteOrgs(IEnumerable<Organisation> orgs, bool deleteLinks = false)
        {
            foreach (var org in orgs)
            {
                DeleteById(org.Id);
            }
        }

        public void DeleteOrgsById(string Ids, bool deleteLinks = false)
        {
            var idList = Ids.Split(',').Select(s => int.Parse(s));
            foreach (var id in idList)
            {
                DeleteById(id, deleteLinks);
            }
        }

        private IEnumerable<IMember> GetMembers(string MemberIds = "")
        {
            if (!String.IsNullOrEmpty(MemberIds))
            {
                int[] memberIds = Array.ConvertAll(MemberIds.Split(','), int.Parse);
                return ApplicationContext.Current.Services.MemberService.GetAllMembers(memberIds);
            }
            else 
            {
                return new List<IMember>();
            }   
        }

        public IEnumerable<Organisation> GetArchived()
        {
            var query = new Sql().Select("*").From("pipelineOrganisation").Where<Organisation>(x => x.Archived);
            var orgs = DbService.db().Fetch<Organisation>(query);
            return orgs;
        }

        public void Archive(Organisation org)
        {
            org.Archived = true;
            DbService.db().Save(org);
        }

        public void Restore(Organisation org)
        {
            org.Archived = false;
            DbService.db().Save(org);
        }

    }
}