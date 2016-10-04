using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.Services;
using GrowCreate.PipelineCRM.DataServices;
using GrowCreate.PipelineCRM.Controllers;

namespace GrowCreate.PipelineCRM.Helpers
{
    public static class PipelineHelper
    {
        public static Pipeline CreatePipeline(string Title,
            string contactName,
            string contactEmail,
            string contactTelephone = "",
            string organisationName = "",
            string comments = "",
            int opportunityValue = 0,
            int opportunityProbability = 0,
            int opportunityStatusId = 0
            )
        {

            var contact = CreateContact(contactName, contactEmail, contactTelephone, organisationName);            
            var pipeline = new Pipeline()
            {
                Name = Title,
                ContactId = contact.Id,
                Value = opportunityValue,
                Probability = opportunityProbability,
                StatusId = opportunityStatusId,
                DateComplete = DateTime.Now
            };

            if (!string.IsNullOrEmpty(contact.OrganisationIds))
            {
                pipeline.OrganisationId = int.Parse(contact.OrganisationIds.Split(',')[0]);
            }

            pipeline = PipelineDbService.Instance.SavePipeline(pipeline);

            if (!string.IsNullOrEmpty(comments))
            {
                CreateTask(comments, contactEmail, pipeline.Id);
            }

            return pipeline;
        }

        public static Task CreateTask(string Note, string contactEmail, int PipelineId = 0)
        {

            var task = new Task();
            var contact = new ContactService().GetByEmail(contactEmail);
            
            if (contact != null)
            {
                task.Description = Note;
                task.UserId = -1;
                task.ContactId = contact.Id;
                if (PipelineId > 0)
                {
                    task.PipelineId = PipelineId;
                }
                task = TaskDbService.Instance.SaveTask(task);
            }
            return task;
        }

        public static Task CreateTask(string Note, int ContactId, int PipelineId = 0)
        {
            var task = new Task();
                       
            task.Description = Note;
            task.UserId = -1;
            task.ContactId = ContactId;
            if (PipelineId > 0)
            {
                task.PipelineId = PipelineId;
            }
            task = TaskDbService.Instance.SaveTask(task);

            return task;
        }

        public static Contact CreateContact(string Name, string contactEmail, string contactTelephone = "", string organisationName = "")
        {
            var contact = new ContactService().GetByEmail(contactEmail);

            if (contact == null)
            {
                contact = new Contact();
            }

            contact.Name = Name;
            contact.Telephone = contactTelephone;
            contact.Email = contactEmail;

            if (!string.IsNullOrEmpty(organisationName))
            {
                var org = CreateOrganisation(organisationName);
                contact.OrganisationIds = string.IsNullOrEmpty(contact.OrganisationIds) ?
                    org.Id.ToString() : contact.OrganisationIds + "," + org.Id.ToString();
            }


            contact = ContactDbService.Instance.SaveContact(contact);
            return contact;
        }

        public static Organisation CreateOrganisation(string Name, string Telephone = "", string Email = "", string Website = "")
        {
            var org = new OrganisationApiController().GetByName(Name);

            if (org == null)
            {
                org = new Organisation();
            }

            org.Name = Name;
            org.Email = Email;
            org.Telephone = Telephone;
            org.Website = Website;

            return OrganisationDbService.Instance.SaveOrganisation(org);
        }


    }
}