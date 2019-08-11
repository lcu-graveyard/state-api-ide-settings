using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fathym;
using Fathym.Design.Singleton;
using LCU.Graphs.Registry.Enterprises;
using LCU.Graphs.Registry.Enterprises.IDE;
using LCU.Presentation.Personas.Applications;
using LCU.Runtime;
using LCU.State.API.IDESettings.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace LCU.State.API.IDESettings.Harness
{
    public class IDESettingsStateHarness : LCUStateHarness<IdeSettingsState>
    {
        #region Fields
        protected readonly ApplicationManagerClient appMgr;
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public IDESettingsStateHarness(HttpRequest req, ILogger logger, IdeSettingsState state)
            : base(req, logger, state)
        {
            appMgr = req.ResolveClient<ApplicationManagerClient>(logger);
        }
        #endregion

        #region API Methods
        public virtual async Task<IdeSettingsState> AddSideBarSection(string section)
        {
            await appDev.AddSideBarSection(section, details.EnterpriseAPIKey, state.SideBarEditActivity);

            return await LoadSideBarSections();
        }

        public virtual async Task<IdeSettingsState> ClearConfig()
        {
            state.Config.CurrentLCUConfig = null;

            state.Config.LCUConfig = new LowCodeUnitConfiguration();

            state.Config.ActiveFiles = new List<string>();

            state.Config.ActiveSolutions = new List<IdeSettingsConfigSolution>();

            return state;
        }


        public virtual async Task<IdeSettingsState> ConfigureSideBarEditActivity()
        {
            if (!state.SideBarEditActivity.IsNullOrEmpty())
            {
                var sidBarSections = await appMgr.LoadIDESideBarSections(details.EnterpriseAPIKey, state.SideBarEditActivity);

                state.SideBarSections = sidBarSections.Model;

                if (!state.EditSection.IsNullOrEmpty())
                {
                    var sidBarActions = await appMgr.LoadIDESideBarActions(details.EnterpriseAPIKey, state.SideBarEditActivity, state.EditSection);

                    state.SectionActions = sidBarActions.Model;
                }
            }

            return state;
        }

        public virtual async Task<IdeSettingsState> DeleteActivity(string activityLookup)
        {
            await appDev.DeleteActivity(activityLookup, details.EnterpriseAPIKey);

            return await LoadActivities();
        }


        public virtual async Task<IdeSettingsState> DeleteLCU(string lcuLookup)
        {
            await appDev.DeleteLCU(lcuLookup, details.EnterpriseAPIKey);

            //  TODO:  Need to delete other assets related to the LCU...  created apps, delete from filesystem, cleanup state??  Or what do we want to do with that stuff?

            return await LoadLCUs();
        }

        public virtual async Task<IdeSettingsState> DeleteSectionAction(string action, string group)
        {
            await appDev.DeleteSectionAction(details.EnterpriseAPIKey, state.EditSection, state.SideBarEditActivity, action, group);

            return await LoadSecionActions();
        }

        public virtual async Task<IdeSettingsState> DeleteSideBarSection(string section)
        {
            await appDev.DeleteSideBarSection(section, details.EnterpriseAPIKey, state.SideBarEditActivity);

            //  TODO:  Also need to delete all related side bar actions for sections

            return await LoadSideBarSections();
        }

        public virtual async Task<IdeSettingsState> DeconstructLCUConfig(string lcuLookup)
        {
            var lcuConfig = await appMgr.LoadLCUConfig(lcuLookup, details.Host);

            state.Config.LCUConfig = lcuConfig.Model;

            state.Config.CurrentLCUConfig = lcuLookup;

            return state;
        }

        public virtual async Task<IdeSettingsState> Ensure()
        {
            await appDev.EnsureIDESettings(details.EnterpriseAPIKey);

            if (state.AddNew == null)
                state.AddNew = new IdeSettingsAddNew();

            if (state.Arch == null)
                state.Arch = new IdeSettingsArchitechtureState() { LCUs = new List<LowCodeUnitSetupConfig>() };

            if (state.Config == null)
                state.Config = new IdeSettingsConfigState()
                {
                    LCUConfig = new LowCodeUnitConfiguration()
                };

            return state;
        }

        public virtual async Task<IdeSettingsState> LoadActivities()
        {
            var acts = await appMgr.LoadIDEActivities(details.EnterpriseAPIKey);

            state.Activities = acts.Model;

            return state;
        }

        public virtual async Task<IdeSettingsState> LoadLCUs()
        {
            var lcus = await appMgr.ListLCUs(details.EnterpriseAPIKey);
            
            state.Arch.LCUs = lcus.Model;

            state.LCUSolutionOptions = state.Arch.LCUs?.ToDictionary(lcu => lcu.Lookup, lcu =>
            {
                var solutions = appMgr.ListLCUSolutions(details.EnterpriseAPIKey, lcu.Lookup).Result;

                return solutions?.Model.Select(sln => sln.Name)?.ToList();
            });

            return state;
        }

        public virtual async Task<IdeSettingsState> LoadLCUConfig(string lcuLookup)
        {
            var lcuConfig = await appMgr.LoadLCUConfig(lcuLookup, details.Host);

            state.Config.LCUConfig = lcuConfig.Model;

            return state;
        }

        public virtual async Task<IdeSettingsState> LoadSecionActions()
        {
            if (!state.SideBarEditActivity.IsNullOrEmpty() && !state.EditSection.IsNullOrEmpty())
            {
                var sidBarActions = await appMgr.LoadIDESideBarActions(details.EnterpriseAPIKey, state.SideBarEditActivity, state.EditSection);

                state.SectionActions = sidBarActions.Model;
            }
            else
                state.SectionActions = new List<IDESideBarAction>();

            return state;
        }

        public virtual async Task<IdeSettingsState> LoadSideBarSections()
        {
            var sections = await appMgr.LoadIDESideBarSections(details.EnterpriseAPIKey, state.SideBarEditActivity);

            state.SideBarSections = sections.Model;

            return state;
        }

        public virtual async Task<IdeSettingsState> SaveActivity(IDEActivity activity)
        {
            if (!activity.Title.IsNullOrEmpty() && !activity.Lookup.IsNullOrEmpty() && !activity.Icon.IsNullOrEmpty())
            {
                var actResp = await appDev.SaveActivity(activity, details.EnterpriseAPIKey);

                activity = actResp.Model;

                await WhenAll(
                    LoadActivities(),
                    ToggleAddNew(AddNewTypes.None)
                );

                state.EditActivity = activity.Lookup;
            }

            return state;
        }

        public virtual async Task<IdeSettingsState> SaveLCU(LowCodeUnitSetupConfig lcu)
        {
            if (!lcu.Lookup.IsNullOrEmpty() && !lcu.NPMPackage.IsNullOrEmpty() && !lcu.PackageVersion.IsNullOrEmpty())
            {
                var ensured = await appDev.EnsureLowCodeUnitView(lcu, details.EnterpriseAPIKey, details.Host);

                return await WhenAll(
                    LoadLCUs(),
                    ToggleAddNew(AddNewTypes.None)
                );
            }

            return state;
        }

        public virtual async Task<IdeSettingsState> SaveLCUCapabilities(string lcuLookup, LowCodeUnitConfiguration lcuConfig)
        {
            if (!lcuLookup.IsNullOrEmpty())
            {
                var status = await appMgr.SaveLCUCapabilities(lcuConfig, details.EnterpriseAPIKey, lcuLookup);

                return await WhenAll(
                    LoadLCUs(),
                    LoadLCUConfig(lcuLookup)
                );
            }

            return state;
        }

        public virtual async Task<IdeSettingsState> SaveSectionAction(IDESideBarAction action)
        {
            if (!action.Action.IsNullOrEmpty() && !action.Title.IsNullOrEmpty())
            {
                action.Section = state.EditSection;

                var secAct = await appDev.SaveSectionAction(action, details.EnterpriseAPIKey, state.SideBarEditActivity);

                return await WhenAll(
                    LoadSecionActions(),
                    ToggleAddNew(AddNewTypes.None)
                );
            }

            return state;
        }

        public virtual async Task<IdeSettingsState> SetConfigLCU(string lcuLookup)
        {
            logger.LogInformation("Starting to set config LCU");

            await ClearConfig();

            state.Config.CurrentLCUConfig = lcuLookup;

            if (!state.Config.CurrentLCUConfig.IsNullOrEmpty())
                return await WhenAll(
                    DeconstructLCUConfig(state.Config.CurrentLCUConfig),
                    LoadLCUConfig(state.Config.CurrentLCUConfig)
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