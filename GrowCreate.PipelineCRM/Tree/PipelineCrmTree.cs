using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using GrowCreate.PipelineCRM.Controllers;
using System.Threading;
using System.Globalization;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using GrowCreate.PipelineCRM.Helpers;
using Umbraco.Core.Configuration;
using GrowCreate.PipelineCRM.Config;
using GrowCreate.PipelineCRM.Services;

namespace GrowCreate.PipelineCRM.Trees
{

    [Tree("pipelineCrm", "pipelineCrmTree", "PipelineCRM")]
    [PluginController("PipelineCRM")]
    public class PipelineCrmTreeController : TreeController
    {
        private IEnumerable<Models.Status> GetStatuses()
        {
            return new StatusApiController().GetAll();
        }
        private IEnumerable<Models.OrgType> GetOrgTypes()
        {
            return new OrgTypeApiController().GetAll();
        }

        private string GetTranslation(string key)
        {
            // using old method - there seems to be a bug in the new one below
            return ui.Text(key.Split('/')[0], key.Split('/')[1]);
            
            //var textService = ApplicationContext.Services.TextService;
            //var culture = LocalizationHelper.GetCultureFromUser(UmbracoContext.Security.CurrentUser);
            //return textService.Localize(key, culture);
        }

        protected override TreeNodeCollection GetTreeNodes(string id, System.Net.Http.Formatting.FormDataCollection queryStrings)
        {
            var ctrl = new PipelineApiController();
            var nodes = new TreeNodeCollection();
            var mainRoute = "/pipelineCrm/pipelineCrmTree";
            bool useBoard = PipelineConfig.GetConfig().AppSettings.UseBoard;

            if (id == Constants.System.Root.ToInvariantString())
            {
                
                nodes.Add(
                        CreateTreeNode(
                        "pipelines",
                        null,
                        queryStrings,
                        GetTranslation("pipeline/opportunities"),
                        "icon-dashboard", 
                        true,
                        useBoard ? "/pipelineCrm" : "/pipelineCrm/pipelineCrmTree/browse/0"
                    )
                );

                nodes.Add(
                    CreateTreeNode(
                        "tasks",
                        null,
                        queryStrings,
                        GetTranslation("pipeline/tasks"),
                        "icon-checkbox",
                        false,
                        mainRoute + "/tasks/0"
                    )
                ); 
                
                nodes.Add(
                    CreateTreeNode(
                        "contacts",
                        null,
                        queryStrings,
                        GetTranslation("pipeline/contacts"),
                        "icon-user",
                        true,
                        mainRoute + "/contacts/0"
                    )
                );

                nodes.Add(
                    CreateTreeNode(
                        "organisations",
                        "-1",
                        queryStrings,
                        GetTranslation("pipeline/organisations"),
                        "icon-company",
                        true,
                        mainRoute + "/organisations/0"
                    )
                );

                if (PipelineProService.Check()){
                    nodes.Add(
                        CreateTreeNode(
                            "segments",
                            "-1",
                            queryStrings,
                            GetTranslation("pipeline/segments"),
                            "icon-users",
                            true,
                            mainRoute + "/segments/0"
                        )
                    );
                } 

                nodes.Add(
                    CreateTreeNode(
                        "recyclebin",
                        null,
                        queryStrings,
                        GetTranslation("general/recycleBin"),
                        "icon-trash",
                        true,
                         "/pipelineCrm/pipelineCrmTree/browse/-1"
                    )
                );

                nodes.Add(
                    CreateTreeNode(
                        "settings",
                        null,
                        queryStrings,
                        GetTranslation("pipeline/settings"),
                        "icon-settings",
                        false,
                        mainRoute + "/settings/0"
                    )
                );

            }
            else if (id == "pipelines")
            {
                // list all opp statuses
                foreach (var status in GetStatuses())
                {
                    var newTreeNode = CreateTreeNode(
                        status.Id.ToString() + "_pipelines",
                        id,
                        queryStrings,
                        status.Name,
                        "icon-dashboard",
                        false,
                        "/pipelineCrm/pipelineCrmTree/browse/" + status.Id.ToString());
                    nodes.Add(newTreeNode);
                }

                // get unassigned pipelines
                var unassigned = ctrl.GetByStatusId(0);
                nodes.Add(
                CreateTreeNode(
                    "0",
                    id,
                    queryStrings,
                    GetTranslation("pipeline/none"),
                    "icon-dashboard",
                    false,
                    "/pipelineCrm/pipelineCrmTree/browse/-2"
                    )
                );

            }
            else if (id == "organisations")
            {
                // list all org types
                foreach (var type in GetOrgTypes())
                {
                    var newTreeNode = CreateTreeNode(
                        type.Id.ToString() + "_organisations",
                        id,
                        queryStrings,
                        type.Name,
                        "icon-company",
                        false,
                        mainRoute + "/organisations/" + type.Id.ToString());
                    nodes.Add(newTreeNode);
                }

                // get unassigned orgs
                var unassigned = ctrl.GetByStatusId(0);
                nodes.Add(
                CreateTreeNode(
                    "0",
                    id,
                    queryStrings,
                    GetTranslation("pipeline/none"),
                    "icon-company",
                    false,
                    mainRoute + "/organisations/-2"
                    )
                );
                
            }
            else if (id == "contacts")
            {
                // list all orgs
                foreach (var type in new ContactTypeApiController().GetAll())
                {
                    var newTreeNode = CreateTreeNode(
                        type.Id.ToString() + "_contacts",
                        id,
                        queryStrings,
                        type.Name,
                        "icon-user",
                        false,
                        mainRoute + "/contacts/" + type.Id.ToString());
                    nodes.Add(newTreeNode);
                }

                // get contacts with no groups
                nodes.Add(
                CreateTreeNode(
                    "0",
                    id,
                    queryStrings,
                    GetTranslation("pipeline/none"),
                    "icon-user",
                    false,
                    mainRoute + "/contacts/-2"
                    )
                );                
                
            }

            // segments
            else if (id == "segments")
            {
                // list all segment types
                foreach (var type in new SegmentTypeApiController().GetAll())
                {
                    var newTreeNode = CreateTreeNode(
                        type.Id.ToString() + "_segments",
                        id,
                        queryStrings,
                        type.Name,
                        "icon-users",
                        false,
                        mainRoute + "/segments/" + type.Id.ToString());
                    nodes.Add(newTreeNode);
                }
            }

            else if (id == "recyclebin")
            {
                
                // pipelines
                nodes.Add(
                    CreateTreeNode(
                            "0",
                            id,
                            queryStrings,
                            GetTranslation("pipeline/opportunities"),
                            "icon-dashboard",
                            false,
                            "/pipelineCrm/pipelineCrmTree/browse/-1"
                        )
                    );
                
                // contacts
                nodes.Add(
                    CreateTreeNode(
                        "0",
                        id,
                        queryStrings,
                        GetTranslation("pipeline/contacts"),
                        "icon-user",
                        false,
                        mainRoute + "/contacts/-1"
                    )
                );

                // groups
                nodes.Add(
                    CreateTreeNode(
                        "0",
                        id,
                        queryStrings,
                        GetTranslation("pipeline/organisations"),
                        "icon-company",
                        false,
                        mainRoute + "/organisations/-1"
                    )
                );

                if (PipelineProService.Check())
                {
                    nodes.Add(
                        CreateTreeNode(
                            "0",
                            id,
                            queryStrings,
                            GetTranslation("pipeline/segments"),
                            "icon-users",
                            false,
                            mainRoute + "/segments/-1"
                        )
                    );
                }
            }

            return nodes;

            //this tree doesn't suport rendering more than 1 level
            throw new NotSupportedException();
        }

        protected override MenuItemCollection GetMenuForNode(string id, System.Net.Http.Formatting.FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            // create and reload on nodes            
            if (id == Constants.System.Root.ToInvariantString() || id == "pipelines")
            {
                menu.Items
                    .Add<ActionNew>(GetTranslation("general/new") + " "  + GetTranslation("pipeline/opportunity"), false)
                    .NavigateToRoute("/pipelineCrm/pipelineCrmTree/edit/-1");
            }
            else if (id.EndsWith("_pipelines"))
            {
                menu.Items
                    .Add<ActionNew>(GetTranslation("general/new") + " " + GetTranslation("pipeline/opportunity"), false)
                    .NavigateToRoute("/pipelineCrm/pipelineCrmTree/edit/-1?statusId=" + id.Split('_')[0]);
            }
            else if (id == "organisations")
            {
                menu.Items
                    .Add<ActionNew>(GetTranslation("general/new") + " " + GetTranslation("pipeline/organisation"), false)
                    .NavigateToRoute("/pipelineCrm/pipelineCrmTree/edit.organisation/-1");
            }
            else if (id.EndsWith("_organisations"))
            {
                menu.Items
                    .Add<ActionNew>(GetTranslation("general/new") + " " + GetTranslation("pipeline/organisation"), false)
                    .NavigateToRoute("/pipelineCrm/pipelineCrmTree/edit.organisation/-1?typeId=" + id.Split('_')[0]);
            }
            else if (id == "contacts")
            {
                menu.Items
                    .Add<ActionNew>(GetTranslation("general/new") + " " + GetTranslation("pipeline/contact"), false)
                    .NavigateToRoute("/pipelineCrm/pipelineCrmTree/edit.contact/-1");
            }
            else if (id.EndsWith("_contacts"))
            {
                menu.Items
                    .Add<ActionNew>(GetTranslation("general/new") + " " + GetTranslation("pipeline/contact"), false)
                    .NavigateToRoute("/pipelineCrm/pipelineCrmTree/edit.contact/-1?typeId=" + id.Split('_')[0]);
            }

            menu.Items.Add<RefreshNode, ActionRefresh>(GetTranslation(string.Format("actions/{0}", ActionRefresh.Instance.Alias)));

            return menu;
        }
    }
}