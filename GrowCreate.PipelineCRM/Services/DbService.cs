using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace GrowCreate.PipelineCRM.Services
{
    public class DbService
    {
        [ThreadStatic]
        private static volatile Database _db;

        public static Database db()
        {
            var dbConn = new Database("umbracoDbDSN");
            if (ConfigurationManager.ConnectionStrings["PipelineDb"] != null)
            {
                var _dbConn = new Database("PipelineDb");
                if (_dbConn != null)
                {
                    dbConn = _dbConn;
                }
            }

            return _db ?? (_db = dbConn);
        }
    }
}