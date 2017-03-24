using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web;
using Umbraco.Core.Persistence;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using GrowCreate.PipelineCRM.Models;
using Umbraco.Web.WebApi;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using Umbraco.Core;
using GrowCreate.PipelineCRM.Services;
using System.Configuration;
using GrowCreate.PipelineCRM.DataServices;
using GrowCreate.PipelineCRM.Config;

namespace GrowCreate.PipelineCRM.Controllers
{
    [PluginController("PipelineCRM")]
    public class ContactApiController : UmbracoAuthorizedApiController
    {
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
        
        public IEnumerable<Contact> GetAll(bool getLinks = false)
        {
            return new ContactService().GetAll(getLinks);
        }

        public PagedContacts GetPaged(int pageNumber = 0, string sortColumn = "", string sortOrder = "", string searchTerm = "", int typeId = 0)
        {
            return new ContactService().GetPagedContacts(pageNumber, sortColumn, sortOrder, searchTerm, typeId);
        }

        public IEnumerable<Contact> GetUnassigned(bool getLinks = false)
        {
            var query = new Sql().Select("*").From("pipelineContact").Where<Contact>(x => !x.Archived).Where<Contact>(x => x.OrganisationIds == "" || x.OrganisationIds == "0");
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

        public Contact GetById(int id, bool getLinks = true)
        {
            return new ContactService().GetById(id, getLinks);
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

        public IEnumerable<Contact> GetByOrganisationId(int id)
        {
            return new ContactService().GetByOrganisationId(id, false);
        }

        public IEnumerable<Contact> GetByContactTypeId(int id)
        {
            return new ContactService().GetByTypeId(id, false);
        }

        public Contact GetByEmail(string email)
        {
            var query = new Sql().Select("*").From("pipelineContact").Where<Contact>(x => x.Email == email);
            return DbService.db().Fetch<Contact>(query).FirstOrDefault();
        }

        public IEnumerable<Contact> GetContactsByName(string name)
        {
            var query = new Sql().Select("*").From("pipelineContact").Where<Contact>(x => x.Name.ToLower().Contains(name.ToLower()));
            return DbService.db().Fetch<Contact>(query);
        }

        public IEnumerable<Contact> PostSaveContacts(IEnumerable<Contact> contacts)
        {
            foreach (var contact in contacts)
            {
                PostSave(contact);
            }            
            return contacts;
        }

        public Contact PostSave(Contact contact)
        {
            contact = new ContactService().Save(contact);

            CreateMember(contact, contact.DisablePortal);
            ToggleMember(contact, contact.DisablePortal);

            return contact;
        }

        public int DeleteById(int id, bool deleteLinks = false)
        {
            var contact = GetById(id, true);
            if (contact != null)
            {
                var pipelines = new PipelineApiController().GetByContactId(contact.Id);

                if (deleteLinks)
                {
                    if (pipelines.Any())
                    {
                        new PipelineApiController().DeletePipelines(pipelines);
                    }
                }

                DeleteMember(GetById(id));
                new TaskApiController().DeleteTasks(contact.Tasks);

                return new ContactService().Delete(id);
            }
            return 0;
        }

        public void DeleteContacts(IEnumerable<Contact> contacts, bool deleteLinks = false)
        {
            foreach (var contact in contacts)
            {
                DeleteById(contact.Id);
            }
        }

        public void DeleteContactsById(string Ids, bool deleteLinks = false)
        {
            var idList = Ids.Split(',').Select(s => int.Parse(s));
            foreach (var id in idList)
            {
                DeleteById(id, deleteLinks);
            }
        }

        public IEnumerable<Contact> GetArchived()
        {
            var query = new Sql().Select("*").From("pipelineContact").Where<Contact>(x => x.Archived);
            var contacts = DbService.db().Fetch<Contact>(query);
            return contacts;
        }

        public void Archive(Contact contact)
        {
            contact.Archived = true;
            ToggleMember(contact, false);
            DbService.db().Save(contact);
        }

        public void Restore(Contact contact)
        {
            contact.Archived = false;
            ToggleMember(contact, true);
            DbService.db().Save(contact);
        }

        public void CreateMember(Contact contact, bool approved = true)
        {
            bool createMembers = PipelineConfig.GetConfig().AppSettings.CreateMembers;
            string memberType = PipelineConfig.GetConfig().AppSettings.MemberType;
            if (createMembers)
            {
                var memberService = UmbracoContext.Application.Services.MemberService;
                if (memberService.GetByEmail(contact.Email) == null)
                {
                    var newMember = memberService.CreateMemberWithIdentity(contact.Email, contact.Email, contact.Name, memberType);
                    newMember.RawPasswordValue = RandomPassword();
                    newMember.IsApproved = !approved;
                    memberService.Save(newMember);
                }
            }            
        }

        public void ToggleMember(Contact contact, bool approved)
        {
            var memberService = UmbracoContext.Application.Services.MemberService;
            var member = memberService.GetByEmail(contact.Email);
            if (member != null)
            {
                member.IsApproved = !approved;
                memberService.Save(member);
            }            
        }

        public void DeleteMember(Contact contact)
        {
            var memberService = UmbracoContext.Application.Services.MemberService;
            var member = memberService.GetByEmail(contact.Email);
            if (member != null)
            {
                memberService.Delete(member);
            }
        }

        public string RandomPassword()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(
                Enumerable.Repeat(chars, 8)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
        }
    }
   
}