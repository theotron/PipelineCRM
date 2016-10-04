using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GrowCreate.PipelineCRM.Services;
using GrowCreate.PipelineCRM.Models;

namespace GrowCreate.PipelineCRM.DataServices
{
    public class TaskDbService
    {
        public event EventHandler OnBeforeSave;
        public event EventHandler OnAfterSave;

        private static readonly TaskDbService instance = new TaskDbService();

        private TaskDbService() { }

        public static TaskDbService Instance
        {
            get 
            {
                return instance; 
            }
        }
        
        public Task SaveTask(Task task)
        {
            EventHandler presave = OnBeforeSave;
            if (null != presave) presave(task, EventArgs.Empty);

            task.DateUpdated = DateTime.Now;

            if (task.Id > 0)
            {
                DbService.db().Update(task);
            }
            else
            {
                task.DateCreated = task.DateUpdated;
                DbService.db().Save(task);
            }

            EventHandler postsave = OnAfterSave;
            if (null != postsave) postsave(task, EventArgs.Empty);

            return task;
        }
    }
}