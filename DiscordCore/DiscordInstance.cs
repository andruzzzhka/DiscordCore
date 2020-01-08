using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DiscordCore
{
    public class DiscordInstance
    {
        public string modId;
        public string modName;
        public Sprite modIcon;
        public long appId;

        public int priority;

        public bool activityValid;
        public Activity activity;

        public DiscordInstance(string modId, string modName, Sprite modIcon, long appId)
        {
            this.modId = modId;
            this.modName = modName;
            this.modIcon = modIcon;
            this.appId = appId;

            priority = 0;
        }

        public void UpdateActivity(Activity activity)
        {
            activityValid = true;
            this.activity = activity;
        }

        public void ClearActivity()
        {
            activityValid = false;
            activity = default;
        }

        public void DestroyInstance()
        {
            DiscordManager.Instance.DestroyInstance(this);
        }
    }
}
