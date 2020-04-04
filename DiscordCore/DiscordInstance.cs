using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Discord.ActivityManager;

namespace DiscordCore
{
    public struct DiscordSettings
    {
        public string modId;
        public string modName;
        public Sprite modIcon;
        public long appId;

        public bool handleInvites;
    }

    public class DiscordInstance
    {
        public event ActivityJoinHandler OnActivityJoin;
        public event ActivityJoinRequestHandler OnActivityJoinRequest;
        public event ActivityInviteHandler OnActivityInvite;
        public event ActivitySpectateHandler OnActivitySpectate;

        public DiscordSettings settings;

        public int Priority { get; internal set; }

        internal bool activityValid;
        internal bool activityEnabled;
        internal Activity activity;

        public DiscordInstance(DiscordSettings settings)
        {
            this.settings = settings;
            activityEnabled = true;

            Priority = 0;
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
            DiscordManager.instance.DestroyInstance(this);
        }

        internal void CallActivityJoin(string secret) { OnActivityJoin?.Invoke(secret); }
        internal void CallActivityJoinRequest(ref User user) { OnActivityJoinRequest?.Invoke(ref user); }
        internal void CallActivityInvite(ActivityActionType actionType, ref User user, ref Activity activity) { OnActivityInvite?.Invoke(actionType, ref user, ref activity); }
        internal void CallActivitySpectate(string secret) { OnActivitySpectate?.Invoke(secret); }

    }
}
