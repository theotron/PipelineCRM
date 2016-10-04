using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GrowCreate.PipelineCRM.Services;
using GrowCreate.PipelineCRM.Models;

namespace GrowCreate.PipelineCRM.DataServices
{    
    public class ContactDbService
    {
        public event EventHandler OnBeforeSave;
        public event EventHandler OnAfterSave;

        private static readonly ContactDbService instance = new ContactDbService();

        private ContactDbService() { }

        public static ContactDbService Instance
        {
            get 
            {
                return instance; 
            }
        }
        
        public Contact SaveContact(Contact contact)
        {
            EventHandler presave = OnBeforeSave;
            if (null != presave) presave(contact, EventArgs.Empty);

            contact.DateUpdated = DateTime.Now;

            if (contact.Id > 0)
            {
                DbService.db().Update(contact);
            }
            else
            {
                contact.DateCreated = contact.DateUpdated;
                DbService.db().Save(contact);
            }

            EventHandler postsave = OnAfterSave;
            if (null != postsave) postsave(contact, EventArgs.Empty);

            return contact;
        }
    }
}