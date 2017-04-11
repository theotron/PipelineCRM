using Umbraco.Core;
using Umbraco.Core.Persistence;
using GrowCreate.PipelineCRM.Models;
using Umbraco.Core.Logging;
using System.Reflection;
using Umbraco.Core.Configuration;
using GrowCreate.PipelineCRM.Installer;
using System.Net;
using System.Collections.Specialized;
using System.Web;
using GrowCreate.PipelineCRM.Controllers;
using System;
using System.IO;
using System.Web.Configuration;

namespace GrowCreate.PipelineCRM
{
    public class RegisterEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            var db = applicationContext.DatabaseContext.Database;

            // install tables for fresh installations

            if (!db.TableExist("pipelineContact"))
            {
                LogHelper.Info(MethodBase.GetCurrentMethod().DeclaringType, "pipelineContact not found.");
                db.CreateTable(false, new Contact().GetType());

                // set max lengths as petapoco decoration was deprecated: https://our.umbraco.org/forum/developers/extending-umbraco/66609-umbracocorepersistence-ntext-deprecated-nvarchar-max
                db.Execute("ALTER table pipelineContact ALTER COLUMN CustomProps nvarchar(max)");

            }

            if (!db.TableExist("pipelineContactType"))
            {
                LogHelper.Info(MethodBase.GetCurrentMethod().DeclaringType, "pipelineContactType not found.");
                db.CreateTable(false, new ContactType().GetType());

                db.Save(new ContactType() { Name = "Prospects" });
                db.Save(new ContactType() { Name = "Clients" });

            }
            if (!db.TableExist("pipelineOrganisation"))
            {
                LogHelper.Info(MethodBase.GetCurrentMethod().DeclaringType, "pipelineOrganisation not found.");
                db.CreateTable(false, new Organisation().GetType());
                db.Execute("ALTER table pipelineOrganisation ALTER COLUMN CustomProps nvarchar(max)");
            }
            if (!db.TableExist("pipelineOrganisationType"))
            {
                LogHelper.Info(MethodBase.GetCurrentMethod().DeclaringType, "pipelineOrganisationType not found.");
                db.CreateTable(false, new OrgType().GetType());

                db.Save(new OrgType() { Name = "Prospects" });
                db.Save(new OrgType() { Name = "Clients" });
            }
            if (!db.TableExist("pipelinePipeline"))
            {
                LogHelper.Info(MethodBase.GetCurrentMethod().DeclaringType, "pipelinePipeline not found.");
                db.CreateTable(false, new Pipeline().GetType());
                db.Execute("ALTER table pipelinePipeline ALTER COLUMN CustomProps nvarchar(max)");
            }
            if (!db.TableExist("pipelineLabel"))
            {
                LogHelper.Info(MethodBase.GetCurrentMethod().DeclaringType, "pipelineLabel not found.");
                db.CreateTable(false, new Label().GetType());

                db.Save(new Label() { Name = "Danger", Value = "#d9534f" });
                db.Save(new Label() { Name = "Warning", Value = "#f0ad4e" });
                db.Save(new Label() { Name = "Info", Value = "#5bc0de" });
                db.Save(new Label() { Name = "Success", Value = "#5cb85c" });
                db.Save(new Label() { Name = "Primary", Value = "#337ab7" });
                db.Save(new Label() { Name = "Default", Value = "#cccccc" });
            }
            if (!db.TableExist("pipelineStatus"))
            {
                LogHelper.Info(MethodBase.GetCurrentMethod().DeclaringType, "pipelineStatus not found.");
                db.CreateTable(false, new Status().GetType());

                db.Save(new Status() { Name = "Prospect" });
                db.Save(new Status() { Name = "Won" });
                db.Save(new Status() { Name = "Lost" });

            }
            if (!db.TableExist("pipelineTask"))
            {
                LogHelper.Info(MethodBase.GetCurrentMethod().DeclaringType, "pipelineTask not found.");
                db.CreateTable(false, new Task().GetType());
            }            
            

            if (!db.TableExist("pipelinePreferences"))
            {
                LogHelper.Info(MethodBase.GetCurrentMethod().DeclaringType, "pipelinePreferences not found.");
                db.CreateTable(false, new Preferences().GetType());
            }

            // install tables for fresh installations
            if (!db.TableExist("pipelineSegments"))
            {
                LogHelper.Info(MethodBase.GetCurrentMethod().DeclaringType, "pipelineSegment not found.");
                db.CreateTable(false, new Segment().GetType());
                db.Execute("ALTER table pipelineSegment ALTER COLUMN CustomProps nvarchar(max)");
                db.Execute("ALTER table pipelineSegment ALTER COLUMN CriteriaProps nvarchar(max)");
            }

            if (!db.TableExist("pipelineSegmentType"))
            {
                LogHelper.Info(MethodBase.GetCurrentMethod().DeclaringType, "pipelineSegmentType not found.");
                db.CreateTable(false, new SegmentType().GetType());

                db.Save(new SegmentType() { Name = "Sectors" });
                db.Save(new SegmentType() { Name = "Source" });
            }
            
            // translate files
            TranslationHelper.AddTranslations();
            
            // add admin user to pipeline if not already added
            AppPermissions.Grant("pipelineCrm");

            // Copy icon
            if (!File.Exists(HttpContext.Current.Server.MapPath("/umbraco/images/tray/PipelineCRM-icon.png")))
                File.Copy(HttpContext.Current.Server.MapPath("/App_plugins/PipelineCRM/PipelineCRM-icon.png"),
                    HttpContext.Current.Server.MapPath("/umbraco/images/tray/PipelineCRM-icon.png"));

            // update tables for upgrades                        
            // note: SQL CE doesn't support IF :(
            /*db.Execute(@"                
                IF COL_LENGTH('pipelineTask','SegmentId') IS NULL
                BEGIN
                    ALTER TABLE pipelineTask ADD SegmentId INT
                END
                ");

            db.Execute(@"
                if col_length('pipelinePreferences','SendDigest') is null 
                begin
                    alter table pipelinePreferences add SendDigest int
                end
            ");*/

        }
    }
}