using System;
using System.Collections.Generic;
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
            return _db ?? (_db = ApplicationContext.Current.DatabaseContext.Database);
        }
    }
}