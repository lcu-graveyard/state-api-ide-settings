using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fathym;
using Fathym.Design.Singleton;
using LCU.Graphs.Registry.Enterprises;
using LCU.Graphs.Registry.Enterprises.IDE;
using LCU.Manager;
using LCU.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace LCU.Manager
{
    public class IDESettingsStateHarness : LCUStateHarness<IdeSettingsState>
    {
        #region Fields
        protected readonly string container;

        protected readonly IDEGraph ideGraph;
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public IDESettingsStateHarness(HttpRequest req, ILogger log, IdeSettingsState state)
            : base(req, log, state)
        {
            this.container = "Default";

            ideGraph = req.LoadGraph<IDEGraph>(log);
        }
        #endregion

        #region API Methods
        public virtual async Task<IdeSettingsState> AddSideBarSection(string section)
        {
            await ideGraph.AddSideBarSection(state.SideBarEditActivity, section, details.EnterpriseAPIKey, container);

            return await LoadSideBarSections();
        }

        public virtual async Task<IdeSettingsState> ClearConfig()
        {
            state.Config.ConfigLCU = null;

            state.Config.LCUFiles = new List<string>();

            state.Config.LCUSolutions = new List<IdeSettingsConfigSolution>();

            state.Config.Files = new List<string>();

            state.Config.Solutions = new List<IdeSettingsConfigSolution>();

            return state;
        }

        public virtual async Task<IdeSettingsState> DeleteActivity(string activityLookup)
        {
            await ideGraph.DeleteActivity(activityLookup, details.EnterpriseAPIKey, container);

            return await LoadActivities();
        }

        public virtual async Task<IdeSettingsState> DeleteLCU(string lcuLookup)
        {
            await ideGraph.DeleteLCU(lcuLookup, details.EnterpriseAPIKey, container);

            //  TODO:  Need to delete other assets related to the LCU...  created apps, delete from filesystem, cleanup state??  Or what do we want to do with that stuff?

            return await LoadLCUs();
        }

        public virtual async Task<IdeSettingsState> DeleteSectionAction(string action, string group)
        {
            await ideGraph.DeleteSectionAction(state.SideBarEditActivity, state.EditSection, action, group, details.EnterpriseAPIKey, container);

            return await LoadSecionActions();
        }

        public virtual async Task<IdeSettingsState> DeleteSideBarSection(string section)
        {
            await ideGraph.DeleteSideBarSection(state.SideBarEditActivity, section, details.EnterpriseAPIKey, container);

            //  TODO:  Also need to delete all related side bar actions for sections

            return await LoadSideBarSections();
        }

        public virtual async Task<IdeSettingsState> ConfigureSideBarEditActivity()
        {
            if (!state.SideBarEditActivity.IsNullOrEmpty())
            {
                state.SideBarSections = await ideGraph.ListSideBarSections(state.SideBarEditActivity, details.EnterpriseAPIKey, container);

                if (!state.EditSection.IsNullOrEmpty())
                    state.SectionActions = await ideGraph.ListSectionActions(state.SideBarEditActivity, state.EditSection, details.EnterpriseAPIKey, container);
            }

            return state;
        }

        public virtual async Task<IdeSettingsState> DeconstructLCUConfig(string lcuLookup)
        {
            var client = new HttpClient();

            //  TODO:  This should hard code at https once that is enforced on the platform
            var lcuJsonPath = $"https://{details.Host}/_lcu/{lcuLookup}/lcu.json";

            log.LogInformation($"Loading lcu.json from: {lcuJsonPath}");

            var lcuConfigResp = await client.GetAsync(lcuJsonPath);

            var lcuConfigStr = await lcuConfigResp.Content.ReadAsStringAsync();

            log.LogInformation($"lcu.json Loaded: {lcuConfigStr}");

            if (lcuConfigResp.IsSuccessStatusCode && !lcuConfigStr.IsNullOrEmpty() && !lcuConfigStr.StartsWith("<"))
            {
                var lcuConfig = lcuConfigStr.FromJSON<dynamic>();

                var slnsDict = ((JToken)lcuConfig.config.solutions).ToObject<Dictionary<string, dynamic>>();

                state.Config.Solutions = slnsDict.Select(sd => new IdeSettingsConfigSolution()
                {
                    Element = sd.Value.element,
                    Name = sd.Key
                }).ToList();

                //  TODO:  Elements and Modules
            }

            return state;
        }

        public virtual async Task<IdeSettingsState> Ensure()
        {
            var settings = new IDEContainerSettings()
            {
                Container = container,
                EnterprisePrimaryAPIKey = details.EnterpriseAPIKey
            };

            await ideGraph.EnsureIDESettings(settings);

            if (state.AddNew == null)
                state.AddNew = new IdeSettingsAddNew();

            if (state.Arch == null)
                state.Arch = new IdeSettingsArchitechture() { LCUs = new List<LowCodeUnitConfig>() };

            if (state.Config == null)
                state.Config = new IdeSettingsConfig();

            return state;
        }

        public virtual async Task<IdeSettingsState> LoadActivities()
        {
            state.Activities = await ideGraph.ListActivities(details.EnterpriseAPIKey, container);

            return state;
        }

        public virtual async Task<IdeSettingsState> LoadLCUs()
        {
            state.Arch.LCUs = await ideGraph.ListLCUs(details.EnterpriseAPIKey, container);

            state.LCUSolutionOptions = state.Arch.LCUs?.ToDictionary(lcu => lcu.Lookup, lcu =>
            {
                var solutions = ideGraph.ListLCUSolutions(lcu.Lookup, details.EnterpriseAPIKey, container).Result;

                return solutions?.Select(sln => sln.Name)?.ToList();
            });

            return state;
        }

        public virtual async Task<IdeSettingsState> LoadLCUConfig(string lcuLookup)
        {
            return await WhenAll(
                LoadLCUFiles(lcuLookup),
                LoadLCUSolutions(lcuLookup)
            );
        }

        public virtual async Task<IdeSettingsState> LoadLCUFiles(string lcuLookup)
        {
            state.Config.LCUFiles = await ideGraph.ListLCUFiles(lcuLookup, details.Host);

            return state;
        }

        public virtual async Task<IdeSettingsState> LoadLCUSolutions(string lcuLookup)
        {
            state.Config.LCUSolutions = await ideGraph.ListLCUSolutions(lcuLookup, details.EnterpriseAPIKey, container);

            return state;
        }

        public virtual async Task<IdeSettingsState> LoadSecionActions()
        {
            if (!state.SideBarEditActivity.IsNullOrEmpty() && !state.EditSection.IsNullOrEmpty())
                state.SectionActions = await ideGraph.ListSectionActions(state.SideBarEditActivity, state.EditSection, details.EnterpriseAPIKey, container);
            else
                state.SectionActions = new List<IdeSettingsSectionAction>();
            return state;
        }

        public virtual async Task<IdeSettingsState> LoadSideBarSections()
        {
            state.SideBarSections = await ideGraph.ListSideBarSections(state.SideBarEditActivity, details.EnterpriseAPIKey, container);

            return state;
        }

        public virtual async Task<IdeSettingsState> SaveActivity(IDEActivity activity)
        {
            if (!activity.Title.IsNullOrEmpty() && !activity.Lookup.IsNullOrEmpty() && !activity.Icon.IsNullOrEmpty())
            {
                activity = await ideGraph.SaveActivity(activity, details.EnterpriseAPIKey, container);

                await WhenAll(
                    LoadActivities(),
                    ToggleAddNew(AddNewTypes.None)
                );

                state.EditActivity = activity.Lookup;
            }

            return state;
        }

        public virtual async Task<IdeSettingsState> SaveLCU(LowCodeUnitConfig lcu)
        {
            if (!lcu.Lookup.IsNullOrEmpty() && !lcu.NPMPackage.IsNullOrEmpty() && !lcu.PackageVersion.IsNullOrEmpty())
            {
                var status = await ensureApplication(lcu);

                lcu = await ideGraph.SaveLCU(lcu, details.EnterpriseAPIKey, container);

                return await WhenAll(
                    LoadLCUs(),
                    ToggleAddNew(AddNewTypes.None)
                );
            }

            return state;
        }

        public virtual async Task<IdeSettingsState> SaveLCUCapabilities(string lcuLookup, List<string> files, List<IdeSettingsConfigSolution> solutions)
        {
            if (!lcuLookup.IsNullOrEmpty())
            {
                var status = await ideGraph.SaveLCUCapabilities(lcuLookup, files, solutions, details.EnterpriseAPIKey, container);

                return await WhenAll(
                    LoadLCUs(),
                    LoadLCUConfig(lcuLookup)
                );
            }

            return state;
        }

        public virtual async Task<IdeSettingsState> SaveSectionAction(IdeSettingsSectionAction action)
        {
            if (!action.Action.IsNullOrEmpty() && !action.Name.IsNullOrEmpty())
            {
                var secAct = await ideGraph.SaveSectionAction(state.SideBarEditActivity, state.EditSection, action, details.EnterpriseAPIKey, container);

                return await WhenAll(
                    LoadSecionActions(),
                    ToggleAddNew(AddNewTypes.None)
                );
            }

            return state;
        }

        public virtual async Task<IdeSettingsState> SetConfigLCU(string lcuLookup)
        {
            await ClearConfig();

            state.Config.ConfigLCU = lcuLookup;

            log.LogInformation("Starting to set config LCU");

            if (!state.Config.ConfigLCU.IsNullOrEmpty())
                return await WhenAll(
                    DeconstructLCUConfig(state.Config.ConfigLCU),
                    LoadLCUConfig(state.Config.ConfigLCU)
                );
            else
                return state;
        }

        public virtual async Task<IdeSettingsState> SetEditActivity(string activityLookup)
        {
            await ToggleAddNew(AddNewTypes.None);

            state.EditActivity = state.Activities?.FirstOrDefault(a => a.Lookup == activityLookup)?.Lookup;

            return state;
        }

        public virtual async Task<IdeSettingsState> SetEditLCU(string lcuLookup)
        {
            await ToggleAddNew(AddNewTypes.None);

            state.Arch.EditLCU = state.Arch.LCUs?.FirstOrDefault(a => a.Lookup == lcuLookup)?.Lookup;

            return state;
        }

        public virtual async Task<IdeSettingsState> SetEditSection(string section)
        {
            await ToggleAddNew(AddNewTypes.None);

            state.EditSection = state.SideBarSections?.FirstOrDefault(sec => sec == section);

            return await LoadSecionActions();
        }

        public virtual async Task<IdeSettingsState> SetEditSectionAction(string action)
        {
            state.EditSectionAction = state.SectionActions?.FirstOrDefault(sa => sa.Action == action)?.Action;

            return await LoadSecionActions();
        }

        public virtual async Task<IdeSettingsState> SetSideBarEditActivity(string activityLookup)
        {
            state.SideBarEditActivity = activityLookup;

            return await ConfigureSideBarEditActivity();
        }

        public virtual async Task<IdeSettingsState> ToggleAddNew(AddNewTypes type)
        {
            state.EditActivity = null;

            state.Arch.EditLCU = null;

            state.EditSectionAction = null;

            switch (type)
            {
                case AddNewTypes.Activity:
                    state.AddNew.Activity = !state.AddNew.Activity;

                    state.AddNew.LCU = false;

                    state.AddNew.SectionAction = false;

                    state.EditSection = null;
                    break;

                case AddNewTypes.LCU:
                    state.AddNew.Activity = false;

                    state.AddNew.LCU = !state.AddNew.LCU;

                    state.AddNew.SectionAction = false;

                    state.EditSection = null;
                    break;

                case AddNewTypes.SectionAction:
                    state.AddNew.Activity = false;

                    state.AddNew.LCU = false;

                    state.AddNew.SectionAction = !state.AddNew.SectionAction;
                    break;

                case AddNewTypes.None:
                    state.AddNew.Activity = false;

                    state.AddNew.LCU = false;

                    state.AddNew.SectionAction = false;

                    state.EditSection = null;
                    break;
            }

            return state;
        }
        #endregion

        #region Helpers
        #endregion
    }

    public enum AddNewTypes
    {
        None,
        Activity,
        LCU,
        SectionAction
    }
}