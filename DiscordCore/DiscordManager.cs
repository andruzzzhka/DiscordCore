using Discord;
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

        private List<DiscordInstance> _activeInstances = new List<DiscordInstance>();
        private float lastUpdateTime;

        public static string deactivationReason;
        private static string lastCheckDeactivationReason;
        private static float lastCheckTime;

        protected void Awake()
        {
            DiscordClient.OnActivityInvite += DiscordClient_OnActivityInvite;
            DiscordClient.OnActivityJoin += DiscordClient_OnActivityJoin;
            DiscordClient.OnActivityJoinRequest += DiscordClient_OnActivityJoinRequest;
            DiscordClient.OnActivitySpectate += DiscordClient_OnActivitySpectate;
            Plugin.log.Debug($"{nameof(DiscordManager)} Awake");
        }

        public static void SetDeactivationReasonFromException(Exception e)
        {
            deactivationReason = e.Message;
        }

        public DiscordInstance CreateInstance(DiscordSettings settings)
        {
            DiscordInstance instance = new DiscordInstance(settings);

            instance.Priority = _activeInstances.Count;

            _activeInstances.Add(instance);

            return instance;
        }

        public void DestroyInstance(DiscordInstance instance)
        {
            if (_activeInstances.Contains(instance))
                _activeInstances.Remove(instance);
        }

        public void Update()
        {
            if (!active && deactivationReason == DiscordClient.DisabledReason) return;
            if(active && Time.time - lastUpdateTime >= 5f)
            {
                lastUpdateTime = Time.time;
                UpdateCurrentActivity();
                DiscordClient.RunCallbacks();
            }
            else if(!active)
            {
                if (UnityEngine.Time.time - lastCheckTime >= 10f)
                {
                    lastCheckTime = UnityEngine.Time.time;
                    if (lastCheckDeactivationReason != deactivationReason)
                    {
                        Plugin.log.Error("DiscordCore is not active! Reason: " + deactivationReason);
                        lastCheckDeactivationReason = deactivationReason;
                    }
#if DEBUG
                    else
                    {
                        Plugin.log.Debug("DiscordCore is still not active: " + deactivationReason);
                    }
#endif
                    DiscordClient.Enable();
                }
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            active = true;
            DiscordClient.Enable();
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
                if (instance.activityValid && instance.Priority < activityPriority)
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
                if (instance.activityValid && instance.settings.handleInvites && instance.settings.appId == DiscordClient.CurrentAppID && (handlerInstance == null || instance.Priority < handlerInstance.Priority))
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
