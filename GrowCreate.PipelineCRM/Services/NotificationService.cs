using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using GrowCreate.PipelineCRM.Models;
using Umbraco.Web.WebApi;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using System.Configuration;

namespace GrowCreate.PipelineCRM.Services
{
    public class NotificationService
    {
        public IEnumerable<Task> GetUserTasks(int userId)
        {
            var taskQuery = new Sql("select * from pipelineTask where UserId = @0 and (Done = 0 or Done is null)", userId);
            var tasks = DbService.db().Query<Task>(taskQuery).ToList();

            foreach (var task in tasks)
            {
                if (task.PipelineId > 0)
                {
                    var pipelineQuery = new Sql().Select("*").From("pipelinePipeline").Where<Pipeline>(x => x.Id == task.PipelineId);
                    task.Pipeline = DbService.db().Query<Pipeline>(pipelineQuery).FirstOrDefault();

                    task.ParentId = task.PipelineId;
                    task.ParentName = task.Pipeline.Name;
                }
                else if (task.OrganisationId > 0)
                {
                    var orgQuery = new Sql().Select("*").From("pipelineOrganisation").Where<Organisation>(x => x.Id == task.OrganisationId);
                    task.Organisation = DbService.db().Query<Organisation>(orgQuery).FirstOrDefault();

                    task.ParentId = task.OrganisationId;
                    task.ParentName = task.Organisation.Name;
                }
                else if (task.ContactId > 0)
                {
                    var contactQuery = new Sql().Select("*").From("pipelineContact").Where<Contact>(x => x.Id == task.ContactId);
                    task.Contact = DbService.db().Query<Contact>(contactQuery).FirstOrDefault();

                    task.ParentId = task.ContactId;
                    task.ParentName = task.Contact.Name;
                }
            }

            // remove arhived pipelines
            tasks = tasks.Where(x => x.Pipeline == null || !x.Pipeline.Archived).ToList();

            return tasks;

        }
        public IEnumerable<Pipeline> GetNewPipelines()
        {
            var pipelineQuery = new Sql().Select("*").From("pipelinePipeline").Where<Pipeline>(x => x.DateCreated > DateTime.Now.AddDays(-1).Date);
            var pipelines = DbService.db().Query<Pipeline>(pipelineQuery).ToList();

            foreach (var pipeline in pipelines)
            {
                var statusQuery = new Sql().Select("*").From("pipelineStatus").Where<Status>(x => x.Id == pipeline.StatusId);
                pipeline.Status = DbService.db().Query<Status>(statusQuery).FirstOrDefault();
            }

            return pipelines;
        }

        private string notificationEmailBody =
            @"<html>
                <body>
                    <h2>{0}</h2>
                    <p>{1}</p>
                    <p>To see the full note and reply, please head on to <a href='{2}'>our website</a></p>
                </body>
            </html>";

        public void SendNote(Task task, string recipient)
        {
            string subject = ConfigurationManager.AppSettings["TaskNoteSubject"];
            string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

            umbraco.library.SendMail(
                ConfigurationManager.AppSettings["NotificationSender"],
                "theo@growcreate.co.uk", //recipient,
                subject,
                notificationEmailBody
                    .Replace("{0}", subject)
                    .Replace("{1}", task.Description)
                    .Replace("{2}", baseUrl + "/view-note?id=" + task.Id + "&user=" + umbraco.library.md5(recipient)),
                true
                );
        }

    }
}