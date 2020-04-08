using BeatSaberMarkupLanguage.Settings;
using Discord;
using DiscordCore.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DiscordCore
{
    public class DiscordManager : PersistentSingleton<DiscordManager>
    {
        public static bool active = true;

        internal List<DiscordInstance> _activeInstances = new List<DiscordInstance>();
        private float lastUpdateTime;

        public static string deactivationReason;
        private static string lastCheckDeactivationReason;
        private static float lastCheckTime;

        protected void Awake()
        {
            Plugin.log.Debug($"{nameof(DiscordManager)} Awake");
            DiscordClient.OnActivityInvite += DiscordClient_OnActivityInvite;
            DiscordClient.OnActivityJoin += DiscordClient_OnActivityJoin;
            DiscordClient.OnActivityJoinRequest += DiscordClient_OnActivityJoinRequest;
            DiscordClient.OnActivitySpectate += DiscordClient_OnActivitySpectate;
        }

        public static void SetDeactivationReasonFromException(Exception e)
        {
            deactivationReason = e.Message;
        }

        public DiscordInstance CreateInstance(DiscordSettings settings)
        {
            DiscordInstance instance = new DiscordInstance(settings);

            if (Config.Instance.ModStates.TryGetValue(settings.modId, out var state))
            {
                while (_activeInstances.Any(x => x.Priority == state.Priority))
                    state.Priority++;

                instance.Priority = state.Priority;
                instance.activityEnabled = state.Active;
                Config.Instance.ModStates[settings.modId] = state;
            }
            else
            {
                instance.Priority = _activeInstances.Count == 0 ? 0 : _activeInstances.Max(x => x.Priority) + 1;
                instance.activityEnabled = true;

                Config.Instance.ModStates.Add(instance.settings.modId, new ModState() { Active = true, Priority = instance.Priority });
            }

            Config.Instance.Save();

            _activeInstances.Add(instance);

            Settings.instance.UpdateModsList();

            return instance;
        }

        public void DestroyInstance(DiscordInstance instance)
        {
            if (_activeInstances.Contains(instance))
            {
                _activeInstances.Remove(instance);
                Settings.instance.UpdateModsList();
            }
        }
        public void Update()
        {
            if (!active && deactivationReason == DiscordClient.DisabledReason) return;

            if (!active && UnityEngine.Time.time - lastCheckTime >= 10f)
            {
                lastCheckTime = UnityEngine.Time.time;
                try
                {
                    DiscordClient.Enable();
                    Plugin.log.Debug($"Discord reactivated.");
                    DiscordManager.active = true;
                    DiscordManager.deactivationReason = string.Empty;
                    lastCheckDeactivationReason = string.Empty;
                }
                catch (ResultException e)
                {
                    ProcessResultException(e, "Error starting DiscordClient: ");
                }
                catch (Exception e)
                {
                    Plugin.log.Debug(e);
                    DiscordManager.active = false;
                    DiscordManager.SetDeactivationReasonFromException(e);
                    lastCheckDeactivationReason = deactivationReason;
                }
            }
            if (active && Time.time - lastUpdateTime >= 5f)
            {
                lastUpdateTime = Time.time;
                lastCheckDeactivationReason = null;
                try
                {
                    UpdateCurrentActivity();
                    DiscordClient.RunCallbacks();
                }
                catch (Discord.ResultException e)
                {
                    ProcessResultException(e, "Error in RunCallbacks: ");
                }
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            active = true;
            try
            {
                DiscordClient.Enable();
                BSMLSettings.instance.RemoveSettingsMenu(Settings.instance);
                BSMLSettings.instance.AddSettingsMenu("Discord Core", "DiscordCore.UI.SettingsViewController.bsml", Settings.instance);
            }
            catch (ResultException e)
            {
                ProcessResultException(e, "Error starting DiscordClient: ");
            }
            catch (Exception e)
            {
                Plugin.log.Debug(e);
                DiscordManager.active = false;
                DiscordManager.SetDeactivationReasonFromException(e);
                lastCheckDeactivationReason = deactivationReason;
            }
        }

        public void OnDisable()
        {
            active = false;
            DiscordClient.Disable();
        }

        private void UpdateCurrentActivity()
        {
            bool activityFound = false;
            int activityPriority = int.MaxValue;
            Activity topPriorityActivity = default;
            long appId = -1;

            foreach (var instance in _activeInstances)
            {
                if (instance.activityValid && instance.activityEnabled && instance.Priority < activityPriority)
                {
                    activityFound = true;
                    activityPriority = instance.Priority;
                    topPriorityActivity = instance.activity;
                    appId = instance.settings.appId;
                }
            }

            if (activityFound)
            {
                DiscordClient.ChangeAppID(appId);
                DiscordClient.GetActivityManager().UpdateActivity(topPriorityActivity, (results) => { });
            }
            else
            {
                DiscordClient.ChangeAppID(-1);
                DiscordClient.GetActivityManager().ClearActivity((result) => { });
            }
        }

        private void ProcessResultException(ResultException e, string messagePrefix)
        {

            DiscordManager.active = false;
            DiscordManager.SetDeactivationReasonFromException(e);
            if (lastCheckDeactivationReason == deactivationReason) return; // Already messaged this.
            lastCheckDeactivationReason = deactivationReason;
            if (e.Result != Result.NotRunning && e.Result != Result.InternalError)
            {
                Plugin.log.Error(messagePrefix + e.Message);
                Plugin.log.Debug(e);
            }
            else
            {
                Plugin.log.Info(messagePrefix + "Discord is not running.");
#if DEBUG
                Plugin.log.Debug(e);
#endif
            }
        }

        #region Event Handlers

        private DiscordInstance FindActivityEventHandler()
        {
            DiscordInstance handlerInstance = null;

            foreach (var instance in _activeInstances)
            {
                if (instance.activityValid && instance.activityEnabled && instance.settings.handleInvites && instance.settings.appId == DiscordClient.CurrentAppID && (handlerInstance == null || instance.Priority < handlerInstance.Priority))
                {
                    handlerInstance = instance;
                }
            }

            return handlerInstance;
        }

        private void DiscordClient_OnActivitySpectate(string secret)
        {
            FindActivityEventHandler()?.CallActivitySpectate(secret);
        }

        private void DiscordClient_OnActivityJoinRequest(ref User user)
        {
            FindActivityEventHandler()?.CallActivityJoinRequest(ref user);
        }

        private void DiscordClient_OnActivityJoin(string secret)
        {
            FindActivityEventHandler()?.CallActivityJoin(secret);
        }

        private void DiscordClient_OnActivityInvite(ActivityActionType type, ref User user, ref Activity activity)
        {
            FindActivityEventHandler()?.CallActivityInvite(type, ref user, ref activity);
        }

        #endregion
    }
}
