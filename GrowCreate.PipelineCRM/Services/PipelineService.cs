using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.Controllers;
using Umbraco.Web.WebApi;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using GrowCreate.PipelineCRM.Config;

namespace GrowCreate.PipelineCRM.Services
{
    public class PostNote
    {
        public string name{ get; set; }
        public string email{ get; set; }
        public string organisation{ get; set; }
        public string telephone { get; set; }
        public string subject { get; set; }
        public string comment{ get; set; }
    }

    [PluginController("PipelineCRM")]
    public class PipelineService
    {

        public PagedPipelines GetPagedPipelines(int pageNumber, string sortColumn, string sortOrder, string searchTerm, int statusId = 0, int contactId = 0, int organisationId = 0)
        {
            int itemsPerPage = PipelineConfig.GetConfig().AppSettings.PageSize;
            var items = new List<Contact>();
            var contactType = typeof(Contact);

            var query = new Sql().Select("*").From("pipelinePipeline");

            if (statusId == 0)
            {
                query.Append(" where Archived=0 ", statusId);
            }
            else if (statusId == -1)
            {
                query.Append(" where Archived=1 ", statusId);
            }
            else if (statusId == -2)
            {
                query.Append(" where StatusId=0 and Archived=0 ", statusId);
            }
            else
            {
                query.Append(" where StatusId=@0 and Archived=0 ", statusId);
            }

            if (contactId > 0)
            {
                query.Append(" and ContactId = @0 ", contactId);
            }
            if (organisationId > 0)
            {
                query.Append(" and OrganisationId = @0 ", organisationId);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query.Append(" and Name like @0 ", "%" + searchTerm + "%");
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortOrder))
            {
                query.OrderBy(sortColumn + " " + sortOrder);
            }
            else
            {
                query.OrderBy("StatusId, Name, DateUpdated asc");
            }

            var p = DbService.db().Page<Pipeline>(pageNumber, itemsPerPage, query);

            for (int i = 0; i < p.Items.ToList().Count(); i++)
            {
                p.Items[i] = new PipelineApiController().GetLinks(p.Items[i]);
            }

            return new PagedPipelines
            {
                TotalPages = p.TotalPages,
                TotalItems = p.TotalItems,
                ItemsPerPage = p.ItemsPerPage,
                CurrentPage = p.CurrentPage,
                Pipelines = p.Items.ToList()
            };
        }



        // deprecate further down... 

        public Pipeline CreateNew(string name, string organisation, string email, string telephone, string subject, string comment, int nodeId = 0, double value = 0, int probability = 50, int statusId = 0, Dictionary<string, dynamic> customProps = null)
        {
            var newPipeline = new Pipeline();

            // we need at least name, email and organisation
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(email))
            {
                return newPipeline;
            }

            var taskController = new TaskApiController();
            var orgController = new OrganisationApiController();
            var contactController = new ContactApiController();
            var pipelineController = new PipelineApiController();

            int currentOrgId = 0;

            if (!string.IsNullOrEmpty(organisation))
            {
                // check if we have an org with that email, if not create one
                var currentOrg = orgController.GetByName(organisation);
                if (currentOrg == null)  // organisation is optional
                {
                    currentOrg = orgController.PostSave(new Organisation()
                    {
                        Id = 0,
                        UserId = 0,
                        Name = organisation,
                        DateCreated = DateTime.Now,
                        Email = email
                    });

                    currentOrgId = currentOrg.Id;
                }
            }

            // check if we have a member with that email, if not create one
            var contact = contactController.GetByEmail(email);
            if (contact == null)
            {
                var newContact = new Contact()
                {
                    Id = 0,
                    OrganisationIds = currentOrgId.ToString(),
                    Name = name,
                    Telephone = telephone,
                    Email = email
                };
                contact = contactController.PostSave(newContact);
            }

            // install cookie for personalised content
            HttpCookie cookie = new HttpCookie("PipelineContactId");
            cookie.Value = contact.Id.ToString();
            cookie.Expires = DateTime.MaxValue;
            HttpContext.Current.Response.SetCookie(cookie);

            //finally, create a new pipeline and note
            newPipeline = pipelineController.PostSave(new Pipeline()
            {
                Name = subject,
                DateCreated = DateTime.Now,
                StatusId = statusId,
                OrganisationId = currentOrgId,
                ContactId = contact.Id,
                Value = value,
                DateComplete = DateTime.Now,
                Probability = probability, // todo: read from node
            });

            if (customProps != null)
            {
                newPipeline.UpdateProperties(customProps);
            }


            if (!String.IsNullOrEmpty(comment))
            {
                var newNote = taskController.PostSave(new GrowCreate.PipelineCRM.Models.Task()
                {
                    Description = comment,
                    PipelineId = newPipeline.Id,
                    UserId = -1
                });
            }
            return newPipeline;
        }
        
        public GrowCreate.PipelineCRM.Models.Task CreateTouchPoint(string description, int contactId, int pipelineId = 0, string subject = "")
        {
            var taskApi = new TaskApiController();
            var pipelineApi = new PipelineApiController();

            if (pipelineId == 0)
            {
                var newPipe = new Pipeline();
                newPipe.Name = subject;
                newPipe.ContactId = contactId;
                newPipe = pipelineApi.PostSave(newPipe);
                pipelineId = newPipe.Id;
            }
            
            var newTask = new GrowCreate.PipelineCRM.Models.Task()
            {
                Description = description,
                ContactId = contactId,
                UserId = -1,
                PipelineId = pipelineId
            };
            newTask = taskApi.PostSave(newTask);
            return newTask;
        }

        public Contact GetContactByEmail(string email)
        {            
            var contactApi = new ContactApiController();
            var contact  = contactApi.GetByEmail(email);

            // Track this contact for personalised content   
            HttpCookie cookie = new HttpCookie("PipelineContactId");
            cookie.Value = contact.Id.ToString();
            cookie.Expires = DateTime.MaxValue;
            HttpContext.Current.Response.SetCookie(cookie);

            return contact;
        }

        public Contact GetContactById(int id)
        {
            var contactApi = new ContactApiController();
            return contactApi.GetById(id);
        }

        public IEnumerable<Pipeline> GetOpportunitiesByContactId(int contactId)
        {
            var pipelineApi = new PipelineApiController();
            var pipelines = pipelineApi.GetByContactId(contactId).Reverse();

            foreach (var pipeline in pipelines)
            {
                pipeline.Tasks = new TaskApiController().GetByPipeline(pipeline.Id).OrderBy(x => x.DateCreated).Reverse();
            }

            return pipelines;
        }

        public IEnumerable<GrowCreate.PipelineCRM.Models.Task> GetGroupNotes(Contact contact)
        {            
            var tasksApi = new TaskApiController();

            if (!string.IsNullOrEmpty(contact.OrganisationIds))
            {
                return tasksApi.GetAll().Where(x => contact.OrganisationIds.Split(',').Contains(x.OrganisationId.ToString()));
            }
            return new List<GrowCreate.PipelineCRM.Models.Task>();
        }

        public Pipeline CreateOpportunity(string name, string email, string description, double amount = 0, int statusId = 0)
        {
            var contact = GetContactByEmail(email);
            if (contact == null)
            {
                contact.Name = name;
                contact.Email = email;
            }
            contact = new ContactApiController().PostSave(contact);
            
            var pipeline = new Pipeline()
            {
                Name = description,
                Value = amount,
                StatusId = statusId,
                ContactId = contact.Id
            };
            return new PipelineApiController().PostSave(pipeline);
        }

        public Pipeline UpdateOpportunity(int id, string description, double amount = 0, int statusId = 0)
        {
            var pipelineApi = new PipelineApiController();
            var pipeline = pipelineApi.GetById(id);

            pipeline.Value = amount;
            pipeline.Name = description;
            pipeline.StatusId = statusId;
            pipeline.DateComplete = DateTime.Now;

            return new PipelineApiController().PostSave(pipeline);
        }

        

    }
}