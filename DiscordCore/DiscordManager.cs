using Discord;
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

        private List<DiscordInstance> _activeInstances = new List<DiscordInstance>();
        private float lastUpdateTime;

        public DiscordInstance CreateInstance(string modId, string modName, Sprite modIcon, long appId = -1)
        {
            DiscordInstance instance = new DiscordInstance(modId, modName, modIcon, appId);

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
            if(Time.time - lastUpdateTime >= 5f)
            {
                lastUpdateTime = Time.time;

                UpdateCurrentActivity();

            }
        }

        private void UpdateCurrentActivity()
        {
            bool activityFound = false;
            int activityPriority = int.MinValue;
            Activity topPriorityActivity = default;
            long appId = -1;

            foreach (var instance in _activeInstances)
            {
                if (instance.activityValid && instance.priority > activityPriority)
                {
                    activityFound = true;
                    activityPriority = instance.priority;
                    topPriorityActivity = instance.activity;
                    appId = instance.appId;
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
    }
}
