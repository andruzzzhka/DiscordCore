using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Discord.ActivityManager;

namespace DiscordCore
{
    public static class DiscordClient
    {
        public const long DefaultAppID = 658039028825718827;

        public static event ActivityJoinHandler OnActivityJoin;
        public static event ActivityJoinRequestHandler OnActivityJoinRequest;
        public static event ActivityInviteHandler OnActivityInvite;
        public static event ActivitySpectateHandler OnActivitySpectate;

        private static long _currentAppId;

        private static Discord.Discord _discordClient;

        static DiscordClient()
        {
            _currentAppId = -1;
            ChangeAppID(DefaultAppID);
        }

        public static void ChangeAppID(long newAppId)
        {
            if((newAppId < 0 ? DefaultAppID : newAppId) != _currentAppId)
            {
                if(_discordClient != null)
                {
                    var oldActManager = _discordClient.GetActivityManager();
                    oldActManager.OnActivityInvite -= OnActivityInvite;
                    oldActManager.OnActivityJoin -= OnActivityJoin;
                    oldActManager.OnActivityJoinRequest -= OnActivityJoinRequest;
                    oldActManager.OnActivitySpectate -= OnActivitySpectate;

                    _discordClient.Dispose();
                }

                _discordClient = new Discord.Discord(newAppId, (UInt64)Discord.CreateFlags.NoRequireDiscord);
                _currentAppId = newAppId;

                var newActManager = _discordClient.GetActivityManager();
                newActManager.OnActivityInvite += OnActivityInvite;
                newActManager.OnActivityJoin += OnActivityJoin;
                newActManager.OnActivityJoinRequest += OnActivityJoinRequest;
                newActManager.OnActivitySpectate += OnActivitySpectate;
            }
        }

        public static ActivityManager GetActivityManager()
        {
            return _discordClient.GetActivityManager();
        }

    }
}
