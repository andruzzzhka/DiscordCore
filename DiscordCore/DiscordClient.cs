using Discord;
using IPA.Logging;
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
        public const string DisabledReason = "Disabled";
        internal static event ActivityJoinHandler OnActivityJoin;
        internal static event ActivityJoinRequestHandler OnActivityJoinRequest;
        internal static event ActivityInviteHandler OnActivityInvite;
        internal static event ActivitySpectateHandler OnActivitySpectate;

        public static long CurrentAppID { get; private set; } = -1;
        public static bool Enabled { get; private set; }
        private static long _appIdWhenDisabled;
        private static Discord.Discord _discordClient;
        private static Dictionary<LogLevel, Logger.Level> _logLevels = new Dictionary<LogLevel, Logger.Level>() { { LogLevel.Debug, Logger.Level.Debug }, { LogLevel.Info, Logger.Level.Info }, { LogLevel.Warn, Logger.Level.Warning }, { LogLevel.Error, Logger.Level.Error } };

        private static void LinkActivityEvents(ActivityManager activityManager)
        {
            activityManager.OnActivityInvite += OnActivityInvite;
            activityManager.OnActivityJoin += OnActivityJoin;
            activityManager.OnActivityJoinRequest += OnActivityJoinRequest;
            activityManager.OnActivitySpectate += OnActivitySpectate;
        }

        private static void UnlinkActivityEvents(ActivityManager activityManager)
        {
            activityManager.OnActivityInvite -= OnActivityInvite;
            activityManager.OnActivityJoin -= OnActivityJoin;
            activityManager.OnActivityJoinRequest -= OnActivityJoinRequest;
            activityManager.OnActivitySpectate -= OnActivitySpectate;
        }

        private static void DisposeClient(Discord.Discord client)
        {
            UnlinkActivityEvents(client.GetActivityManager());
            client.Dispose();
        }

        internal static void Disable(bool hasError = false)
        {
            _appIdWhenDisabled = CurrentAppID;
            CurrentAppID = -1;
            if (_discordClient != null)
            {
                DisposeClient(_discordClient);
                _discordClient = null;
            }
            if (Enabled)
            {
                Enabled = false;
                if (!hasError)
                {
                    DiscordManager.deactivationReason = DisabledReason;
                    Plugin.log.Info($"DiscordClient disabled.");
                }
                else
                {
                    Plugin.log.Info($"DiscordClient disabled by error.");
                }
            }
        }

        /// <summary>
        /// If not already enabled, creates a new DiscordClient. Disposes of the old one if it exists.
        /// </summary>
        /// <exception cref="ResultException"></exception>
        internal static void Enable()
        {
            if (Enabled)
                return;
            if (_discordClient != null)
            {
                DisposeClient(_discordClient);
                _discordClient = null;
            }
            CurrentAppID = _appIdWhenDisabled < 0 ? DefaultAppID : _appIdWhenDisabled;
            try
            {
                _discordClient = CreateClient(CurrentAppID);

                var newActManager = _discordClient.GetActivityManager();
                LinkActivityEvents(newActManager);
                newActManager.RegisterSteam(620980);
            }
            catch
            {
                Disable(true);
                throw;
            }
            Enabled = true;
            DiscordManager.active = true;
            DiscordManager.deactivationReason = string.Empty;
            Plugin.log.Info($"DiscordClient enabled.");
        }

        private static Discord.Discord CreateClient(long appId)
        {
            Discord.Discord client = new Discord.Discord(appId, (ulong)CreateFlags.NoRequireDiscord);
            client.SetLogHook(LogLevel.Debug, LogCallback);
            return client;
        }

        public static void ChangeAppID(long newAppId)
        {
            if ((newAppId < 0 ? DefaultAppID : newAppId) != CurrentAppID)
            {
                try
                {
                    if (_discordClient != null)
                    {
                        DisposeClient(_discordClient);
                        _discordClient = null;
                    }

                    _discordClient = CreateClient(newAppId);
                    CurrentAppID = newAppId;

                    var newActManager = _discordClient.GetActivityManager();
                    LinkActivityEvents(newActManager);
                    newActManager.RegisterSteam(620980);
                    Enabled = true;
                    DiscordManager.active = true;
                }
                catch (Discord.ResultException e)
                {
                    Disable(true);
                    throw;
                }
            }
        }


        private static void LogCallback(LogLevel level, string message)
        {
            Plugin.log.Log(_logLevels[level], $"[DISCORD] {message}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ResultException"></exception>
        public static void RunCallbacks()
        {
            try
            {
                _discordClient?.RunCallbacks();
            }
            catch
            {
                Disable(true);
                throw;
            }
        }

        public static AchievementManager GetAchievementManager() { return _discordClient?.GetAchievementManager(); }
        public static ActivityManager GetActivityManager() { return _discordClient?.GetActivityManager(); }
        public static ApplicationManager GetApplicationManager() { return _discordClient?.GetApplicationManager(); }
        public static ImageManager GetImageManager() { return _discordClient?.GetImageManager(); }
        public static LobbyManager GetLobbyManager() { return _discordClient?.GetLobbyManager(); }
        public static NetworkManager GetNetworkManager() { return _discordClient?.GetNetworkManager(); }
        public static OverlayManager GetOverlayManager() { return _discordClient?.GetOverlayManager(); }
        public static RelationshipManager GetRelationshipManager() { return _discordClient?.GetRelationshipManager(); }
        public static StorageManager GetStorageManager() { return _discordClient?.GetStorageManager(); }
        public static StoreManager GetStoreManager() { return _discordClient?.GetStoreManager(); }
        public static UserManager GetUserManager() { return _discordClient?.GetUserManager(); }
        public static VoiceManager GetVoiceManager() { return _discordClient?.GetVoiceManager(); }

    }
}
