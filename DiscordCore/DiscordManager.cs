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
    public class DiscordManager
    {
        public static DiscordManager Instance 
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = new DiscordManager();
                }
                return _instance; 
            }
        }
        private static DiscordManager _instance;

        internal List<DiscordInstance> _activeInstances = new List<DiscordInstance>();
        private float lastUpdateTime;

        public DiscordManager()
        {
            DiscordClient.OnActivityInvite += DiscordClient_OnActivityInvite;
            DiscordClient.OnActivityJoin += DiscordClient_OnActivityJoin;
            DiscordClient.OnActivityJoinRequest += DiscordClient_OnActivityJoinRequest;
            DiscordClient.OnActivitySpectate += DiscordClient_OnActivitySpectate;
        }

        public DiscordInstance CreateInstance(DiscordSettings settings)
        {
            DiscordInstance instance = new DiscordInstance(settings);

            Plugin.log.Info($"Trying to register new mod: {settings.modId}");

            if (Config.Instance.ModStates.TryGetValue(settings.modId, out var state))
            {
                Plugin.log.Info($"Found mod state: {state.Active}, {state.Priority}");

                while (_activeInstances.Any(x => x.Priority == state.Priority))
                    state.Priority++;

                Plugin.log.Info($"Modified mod state: {state.Active}, {state.Priority}");

                instance.Priority = state.Priority;
                instance.activityEnabled = state.Active;
                Config.Instance.ModStates[settings.modId] = state;
            }
            else
            {
                instance.Priority = _activeInstances.Count == 0 ? 0 : _activeInstances.Max(x => x.Priority) + 1;
                instance.activityEnabled = true;

                Plugin.log.Info($"Created mod state: true, {instance.Priority}");

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
            if(Time.time - lastUpdateTime >= 5f)
            {
                lastUpdateTime = Time.time;

                UpdateCurrentActivity();

            }
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
                DiscordClient.GetActivityManager().UpdateActivity(topPriorityActivity, (results) => {  });
            }
            else
            {
                DiscordClient.ChangeAppID(-1);
                DiscordClient.GetActivityManager().ClearActivity((result) => { });
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
