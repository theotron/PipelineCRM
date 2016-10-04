using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GrowCreate.PipelineCRM.Services;
using GrowCreate.PipelineCRM.Models;

namespace GrowCreate.PipelineCRM.DataServices
{    
    public class OrganisationDbService
    {
        public event EventHandler OnBeforeSave;
        public event EventHandler OnAfterSave;

        private static readonly OrganisationDbService instance = new OrganisationDbService();

        private OrganisationDbService() { }

        public static OrganisationDbService Instance
        {
            get 
            {
                return instance; 
            }
        }

        public Organisation SaveOrganisation(Organisation organisation)
        {
            EventHandler presave = OnBeforeSave;
            if (null != presave) presave(organisation, EventArgs.Empty);

            organisation.DateUpdated = DateTime.Now;

            if (organisation.Id > 0)
            {
                DbService.db().Update(organisation);
            }
            else
            {
                organisation.DateCreated = organisation.DateUpdated;
                DbService.db().Save(organisation);
            }

            EventHandler postsave = OnAfterSave;
            if (null != postsave) postsave(organisation, EventArgs.Empty);

            return organisation;
        }
    }
}