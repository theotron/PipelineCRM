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
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using System.Configuration;
using GrowCreate.PipelineCRM.Services;
using GrowCreate.PipelineCRM.Config;

namespace GrowCreate.PipelineCRM.SurfaceControllers
{
    public class NotificationController : SurfaceController
    {
        public class NotificationEmail
        {
            public string userName { get; set; }
            public string userEmail { get; set; }
            public IEnumerable<Task> todaysTasks { get; set; }
            public IEnumerable<Task> reminders { get; set; }
            public IEnumerable<Task> overdueTasks { get; set; }
            public IEnumerable<Task> upcomingTasks { get; set; }
            public IEnumerable<Pipeline> newPipelines { get; set; }
        }

        public class PipelineUser
        {
            public string userName { get; set; }
            public string userEmail { get; set; }
            public int Id { get; set; }
        }

        [HttpGet]
        public ActionResult SendNotifications(string force = "")
        {
            if (force == "true" || (DateTime.Now.Hour == PipelineConfig.GetConfig().AppSettings.DigestTime && DateTime.Now.DayOfWeek != DayOfWeek.Saturday && DateTime.Now.DayOfWeek != DayOfWeek.Sunday))
            {
                var notifactions = CollectNotifications();
                string notificationEmailBody = PipelineConfig.GetConfig().DigestBody.InnerHtml;                
                string sender = PipelineConfig.GetConfig().AppSettings.DigestSender;
                string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority) + "/umbraco/#/pipelineCrm/pipelineCrmTree/edit/";                

                foreach (var notification in notifactions)
                {
                    string subject = PipelineConfig.GetConfig().AppSettings.DigestSubject;
                    string todaysTasks = "";
                    string reminderTasks = "";
                    string overdueTasks = "";
                    string upcomingTasks = "";
                    string pipelineRows = "";

                    subject = subject.Replace("{0}", notification.userName).Replace("{1}", DateTime.Now.ToString("dd MMM yyyy"));

                    if (notification.todaysTasks.Any())
                    {
                        foreach (var task in notification.todaysTasks)
                        {
                            todaysTasks += TaskRow(task);
                        }
                    }
                    else
                    {
                        todaysTasks = "No tasks due today.";
                    }

                    if (notification.reminders.Any())
                    {
                        foreach (var task in notification.reminders)
                        {
                            reminderTasks += TaskRow(task);
                        }
                    }
                    else
                    {
                        reminderTasks = "No task reminders today.";
                    }
                    
                    if (notification.overdueTasks.Any())
                    {
                        foreach (var task in notification.overdueTasks)
                        {
                            overdueTasks += TaskRow(task);
                        }
                    }
                    else
                    {
                        overdueTasks = "No overdue tasks.";
                    }
                    
                    if (notification.newPipelines.Any())
                    {
                        foreach (var pipeline in notification.newPipelines)
                        {
                            pipelineRows += PipelineRow(pipeline);
                        }
                    }
                    else
                    {
                        pipelineRows = "No new pipelines today.";
                    }

                    string emailBody = notificationEmailBody
                            .Replace("{0}", subject)
                            .Replace("{1}", todaysTasks)
                            .Replace("{2}", reminderTasks)
                            .Replace("{3}", overdueTasks)
                            .Replace("{4}", upcomingTasks)
                            .Replace("{5}", pipelineRows);

                    umbraco.library.SendMail(
                        sender,
                        notification.userEmail,
                        subject,
                        emailBody,
                        true
                        );
                    //return Content(emailBody);
                }                

                return Json(notifactions.Count() + " emails sent.", JsonRequestBehavior.AllowGet);
            }
            return Json("Please wait until the time set in the config.", JsonRequestBehavior.AllowGet);
        }
        
        private string TaskRow(Task task)
        {
            string notificationEmailRow = PipelineConfig.GetConfig().DigestRow.InnerHtml;
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority) + "/umbraco#/pipelineCrm/";

            return notificationEmailRow
                .Replace("{0}", baseUrl + task.ParentId.ToString())
                .Replace("{1}", !string.IsNullOrEmpty(task.Type) ? task.Type + ": " : "")
                .Replace("{2}", task.Description)
                .Replace("{3}", "In " + task.ParentName + ", due" + task.DateDue.ToString("dd MMM yyyy"));
        }

        private string PipelineRow(Pipeline pipeline)
        {
            string notificationEmailRow = PipelineConfig.GetConfig().DigestRow.InnerHtml;
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority) + "/umbraco#/pipelineCrm/";

            if (pipeline.Status == null)
                return "";

            return notificationEmailRow
                .Replace("{0}", baseUrl + pipeline.Id.ToString())
                .Replace("{1}", pipeline.Status.Name + ": ")
                .Replace("{2}", pipeline.Name)
                .Replace("{3}", "Created at " + pipeline.DateCreated.ToString("dd MMM yyyy")); ;
        }

        [HttpGet]
        public ActionResult GetNotifications()
        {
            return Json(CollectNotifications(), JsonRequestBehavior.AllowGet);
        }

        public IEnumerable<NotificationEmail> CollectNotifications()
        {
            // get users which have opted to receive the digest
            var users = DbService.db().Query<PipelineUser>("select Id, userName, userEmail from umbracoUser where id in (select userId from pipelinePreferences where ReceiveDigest = 1)").ToList();
            var notificationService = new GrowCreate.PipelineCRM.Services.NotificationService();
            var notifications = new List<NotificationEmail>();

            foreach (var user in users)
            {
                var userTasks = notificationService.GetUserTasks(user.Id);
                var notification = new NotificationEmail()
                {
                    userName = user.userName,
                    userEmail = user.userEmail,
                    todaysTasks = userTasks.Where<Task>(x => x.DateDue != null && x.DateDue.Date == DateTime.Now.Date),
                    reminders = userTasks.Where<Task>(x => x.Reminder != null && x.Reminder.Date == DateTime.Now.Date),
                    overdueTasks = userTasks.Where(x => x.DateDue.Date < DateTime.Now.Date),                    
                    newPipelines = notificationService.GetNewPipelines()
                };

                if (notification.todaysTasks.Any() || notification.overdueTasks.Any() || notification.upcomingTasks.Any() || notification.newPipelines.Any())
                {
                    notifications.Add(notification);
                }
            }

            return notifications;
        }
    }
}