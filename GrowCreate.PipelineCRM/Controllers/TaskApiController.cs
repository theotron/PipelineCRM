using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using GrowCreate.PipelineCRM.Models;
using Umbraco.Web.WebApi;
using Umbraco.Core.Services;
using Umbraco.Core;
using GrowCreate.PipelineCRM.Services;
using GrowCreate.PipelineCRM.DataServices;

namespace GrowCreate.PipelineCRM.Controllers
{
    [PluginController("PipelineCRM")]
    public class TaskApiController : UmbracoAuthorizedApiController
    {
        private Task GetTaskUser(Task task)
        {
            if (task.UserId >= 0)
            {
                var userService = ApplicationContext.Current.Services.UserService; 
                var user = userService.GetByProviderKey(task.UserId);
                if (user != null)
                {
                    task.UserName = user.Name;
                    task.UserAvatar = umbraco.library.md5(user.Email);
                }                
            }
            else if (task.ContactId > 0)
            {
                var contact = new ContactService().GetById(task.ContactId, false);
                if (contact != null)
                {
                    task.UserName = contact.Name;
                    task.UserAvatar = umbraco.library.md5(contact.Email);
                }
            }

            if (!String.IsNullOrEmpty(task.Files) && task.Files.Split(',').Count() > 0)
            {
                var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                var media = umbracoHelper.TypedMedia(task.Files.Split(','));
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

                task.Media = mediaList;
            }

            task.Overdue = !task.Done && task.DateDue < DateTime.Now ? (DateTime.Now - task.DateDue).Days : 0;

            return task;
        }
        
        public IEnumerable<Task> GetAll()
        {
            return DbService.db().Query<Task>("select * from pipelineTask");
        }

        public IEnumerable<Task> GetMyTasks()
        {
            var userService = ApplicationContext.Current.Services.UserService;
            var userId = userService.GetByUsername(HttpContext.Current.User.Identity.Name).Id;
            var query = new Sql().Select("*").From("pipelineTask").Where<Task>(x => x.UserId == userId).Where<Task>(x => !x.Done).Where<Task>(x => x.DateDue != null);
            var tasks = DbService.db().Fetch<Task>(query).ToList();

            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i] = GetTaskUser(tasks[i]);
                if (tasks[i].PipelineId > 0)
                {
                    tasks[i].Pipeline = new PipelineApiController().GetById(tasks[i].PipelineId, false);                
                }
                if (tasks[i].ContactId > 0)
                {
                    tasks[i].Contact = new ContactApiController().GetById(tasks[i].ContactId, false);
                }
                if (tasks[i].OrganisationId > 0)
                {
                    tasks[i].Organisation = new OrganisationApiController().GetById(tasks[i].OrganisationId, false);
                }
            }

            //tasks.RemoveAll(x => (x.PipelineId > 0 && x.Pipeline.Archived) || (x.ContactId > 0 && x.Contact.Archived) || (x.OrganisationId > 0 && x.Organisation.Archived));

            return tasks.ToList().OrderBy(x => x.Overdue).OrderBy(x => x.DateDue);
        }

        public IEnumerable<Task> GetByPipeline(int id)
        {
            var query = new Sql().Select("*").From("pipelineTask").Where<Task>(x => x.PipelineId == id).OrderByDescending("DateDue").OrderByDescending("DateCreated");
            var tasks = DbService.db().Fetch<Task>(query).ToList();

            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i] = GetTaskUser(tasks[i]);
            }

            return tasks;
        }

        public IEnumerable<Task> GetLatest()
        {
            string currentUserName = HttpContext.Current.User.Identity.Name;
            var userService = ApplicationContext.Current.Services.UserService;
            int UserId = userService.GetByUsername(currentUserName).Id;
            var tasks = DbService.db().Query<Task>("select top 5 * from pipelineTask where Done = 0 and UserId = @0 order by DateDue desc, DateCreated desc", UserId).ToList();

            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i] = GetTaskUser(tasks[i]);
                tasks[i].Pipeline = new PipelineApiController().GetById(tasks[i].PipelineId, false);
            }

            return tasks;
        }

        public IEnumerable<Task> GetByOrganisation(int id)
        {
            var query = new Sql("select * from pipelineTask where PipelineId in (select Id from pipelinePipeline where OrganisationId = @0 and Archived = 0)", id);
            var tasks = DbService.db().Fetch<Task>(query);

            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i] = GetTaskUser(tasks[i]);
                tasks[i].Pipeline = new PipelineApiController().GetById(tasks[i].PipelineId, false);
            }

            return tasks;
        }

        public IEnumerable<Task> GetByOrganisationId(int id)
        {
            var query = new Sql().Select("*").From("pipelineTask").Where<Task>(x => x.OrganisationId == id).OrderByDescending("DateDue").OrderByDescending("DateCreated");
            var tasks = DbService.db().Fetch<Task>(query);

            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i] = GetTaskUser(tasks[i]);
                tasks[i].Pipeline = new PipelineApiController().GetById(tasks[i].PipelineId, false);
            }

            return tasks;
        }
        
        public IEnumerable<Task> GetByContactId(int id)
        {
            var query = new Sql().Select("*").From("pipelineTask").Where<Task>(x => x.ContactId == id).OrderByDescending("DateDue").OrderByDescending("DateCreated");
            var tasks = DbService.db().Fetch<Task>(query);

            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i] = GetTaskUser(tasks[i]);
                tasks[i].Pipeline = new PipelineApiController().GetById(tasks[i].PipelineId, false);
            }

            return tasks;
        }

        public IEnumerable<Task> GetBySegmentId(int id)
        {
            var query = new Sql().Select("*").From("pipelineTask").Where<Task>(x => x.SegmentId == id).OrderByDescending("DateDue").OrderByDescending("DateCreated");
            var tasks = DbService.db().Fetch<Task>(query);

            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i] = GetTaskUser(tasks[i]);
                tasks[i].Pipeline = new PipelineApiController().GetById(tasks[i].PipelineId, false);
            }

            return tasks;
        }

        public Task GetById(int id)
        {
            var query = new Sql().Select("*").From("pipelineTask").Where<Task>(x => x.Id == id);
            var task = DbService.db().Fetch<Task>(query).FirstOrDefault();

            task = GetTaskUser(task);

            return task;
        }

        public Task CreateTask(string description, int contactId = 0, int pipelineId = 0, int organisationId = 0, string type = "")
        {
            var task = new Task()
            {
                Description = description,
                PipelineId = pipelineId,
                ContactId = contactId,
                OrganisationId = organisationId,
                Type = type
            };
            return PostSave(task);
        }

        public Task PostSave(Task task, bool sendEmail = false)
        {            
            if (task.UserId == 0 && HttpContext.Current.User != null)
            {
                var userService = ApplicationContext.Current.Services.UserService; 
                var userId = userService.GetByUsername(HttpContext.Current.User.Identity.Name).Id;
                task.UserId = userId;
            }

            // auto-set due date for orphan tasks
            if (task.PipelineId == 0 && task.ContactId == 0 && task.OrganisationId == 0)
            {
                task.DateDue = task.DateDue > DateTime.MinValue ? task.DateDue : DateTime.Now;
            }

            task = TaskDbService.Instance.SaveTask(task); 
            return GetTaskUser(task);
        }

        public Task PostToggle(int id)
        {
            var task = GetById(id);
            if (!task.Done)
            {
                task.Done = true;
                task.DateComplete = DateTime.Now;
            }
            else
            {
                task.Done = false;
            }
            DbService.db().Update(task);                       
            return task;
        }

        public int DeleteById(int id)
        {
            return DbService.db().Delete<Task>(id);
        }

        public void DeleteTasks(IEnumerable<Task> tasks)
        {
            foreach (var task in tasks)
            {
                DeleteById(task.Id);
            }
        }

        public void SendNote(Task task)
        {
            string recipient = task.PipelineId > 0 ?
                new PipelineApiController().GetById(task.PipelineId).Contact.Email :
                task.Contact.Email;
            new GrowCreate.PipelineCRM.Services.NotificationService().SendNote(task, recipient);
        }

    }
}