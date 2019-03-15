using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LCU.State.API.IdeSettings.Models;
using LCU.Graphs;
using LCU.Graphs.Registry.Enterprises.IDE;
using System.Collections.Generic;
using System.Linq;

namespace LCU.State.API.IDESettings
{
    public static class Refresh
    {
        [FunctionName("Refresh")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.WithState<dynamic, IdeSettingsState>(log, async (details, reqData, state, stateMgr) =>
            {
                var regGraphConfig = new LCUGraphConfig()
                {
                    APIKey = Environment.GetEnvironmentVariable("LCU_GRAPH_API_KEY"),
                    Host = Environment.GetEnvironmentVariable("LCU_GRAPH_HOST"),
                    Database = Environment.GetEnvironmentVariable("LCU_GRAPH_DATABASE"),
                    Graph = Environment.GetEnvironmentVariable("LCU_GRAPH")
                };

                var ideGraph = new IDEGraph(regGraphConfig);

                var settings = new IDEContainerSettings()
                {
                    Container = "Default",
                    EnterprisePrimaryAPIKey = details.EnterpriseAPIKey
                };

                var ideSettings = await ideGraph.EnsureIDESettings(settings);

                state.Activities = await ideGraph.ListActivities(details.EnterpriseAPIKey, settings.Container);

                if (state.AddNew == null)
                    state.AddNew = new IdeSettingsAddNew();

                if (!state.SideBarEditActivity.IsNullOrEmpty())
                {
                    state.SideBarSections = await ideGraph.ListSideBarSections(state.SideBarEditActivity, details.EnterpriseAPIKey, settings.Container);

                    if (!state.EditSection.IsNullOrEmpty())
                        state.SectionActions = await ideGraph.ListSectionActions(state.SideBarEditActivity, state.EditSection, details.EnterpriseAPIKey, "Default");
                }

               var lcus = await ideGraph.ListLCUs(details.EnterpriseAPIKey, settings.Container);

                state.LCUSolutionOptions = lcus?.ToDictionary(lcu => lcu.Lookup, lcu =>
                {
                    var solutions = ideGraph.ListLCUSolutions(lcu.Lookup, details.EnterpriseAPIKey, "Default").Result;

                    return solutions?.Select(sln => sln.Name)?.ToList();
                });

                if (state.Arch == null)
                    state.Arch = new IdeSettingsArchitechture() { LCUs = new List<LowCodeUnitConfig>() };

                state.Arch.LCUs = await ideGraph.ListLCUs(details.EnterpriseAPIKey, settings.Container);

                if (state.Config == null)
                    state.Config = new IdeSettingsConfig();

                state.Config.ConfigLCU = null;

                state.Config.LCUFiles = new List<string>();

                state.Config.LCUSolutions = new List<IdeSettingsConfigSolution>();

                state.Config.Files = new List<string>();

                state.Config.Solutions = new List<IdeSettingsConfigSolution>();

                return state;
            });
        }
    }
}
