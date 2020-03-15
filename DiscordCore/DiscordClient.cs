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

        public static long CurrentAppID { get; private set; }
        public static bool Enabled { get; private set; }
        private static long _appIdWhenDisabled;
        private static Discord.Discord _discordClient;
        private static Dictionary<LogLevel, Logger.Level> _logLevels = new Dictionary<LogLevel, Logger.Level>() { { LogLevel.Debug, Logger.Level.Debug }, { LogLevel.Info, Logger.Level.Info }, { LogLevel.Warn, Logger.Level.Warning }, { LogLevel.Error, Logger.Level.Error } };

        static DiscordClient()
        {
            CurrentAppID = -1;
            ChangeAppID(DefaultAppID);
        }
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

        internal static void Enable()
        {
            if (Enabled)
                return;

            try
            {
                if (_discordClient != null)
                {
                    DisposeClient(_discordClient);
                    _discordClient = null;
                }
                CurrentAppID = _appIdWhenDisabled < 0 ? DefaultAppID : _appIdWhenDisabled;
                _discordClient = CreateClient(CurrentAppID);

                var newActManager = _discordClient.GetActivityManager();
                LinkActivityEvents(newActManager);
                newActManager.RegisterSteam(620980);
                Enabled = true;
                DiscordManager.active = true;
                DiscordManager.deactivationReason = string.Empty;
                Plugin.log.Info($"DiscordClient enabled.");
            }
            catch (Discord.ResultException e)
            {
                if (e.Result != Result.NotRunning && e.Result != Result.InternalError)
                {
                    Plugin.log.Error($"Error in RunCallbacks: {e.Result} - {e.Message}");
                    Plugin.log.Debug(e);
                }
                else
                {
                    Plugin.log.Info("Discord is not running.");
                }
                Disable(true);
                DiscordManager.active = false;
                DiscordManager.SetDeactivationReasonFromException(e);
            }
            catch (Exception e)
            {
                Plugin.log.Debug(e);
                Disable(true);
                DiscordManager.active = false;
                DiscordManager.SetDeactivationReasonFromException(e);
            }
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
                    if (e.Result != Result.NotRunning)
                    {
                        Plugin.log.Error($"Error in RunCallbacks: {e.Result} - {e.Message}");
                        Plugin.log.Debug(e);
                    }
                    else
                    {
                        Plugin.log.Info("Discord is not running.");
                    }
                    Disable(true);
                    DiscordManager.active = false;
                    DiscordManager.SetDeactivationReasonFromException(e);
                }
                catch (Exception e)
                {
                    Plugin.log.Debug(e);
                    Disable(true);
                    DiscordManager.active = false;
                    DiscordManager.SetDeactivationReasonFromException(e);
                }
            }
        }


        private static void LogCallback(LogLevel level, string message)
        {
            Plugin.log.Log(_logLevels[level], $"[DISCORD] {message}");
        }

        public static void RunCallbacks()
        {
            try
            {
                _discordClient?.RunCallbacks();
            }
            catch (Discord.ResultException e)
            {
                if (e.Result == Result.NotRunning)
                {
                    Plugin.log.Info("Discord is no longer running.");
                    Disable(true);
                }
                else
                {
                    Plugin.log.Error($"Error in RunCallbacks: {e.Result} - {e.Message}");
                    Plugin.log.Debug(e);
                }
                DiscordManager.active = false;
                DiscordManager.SetDeactivationReasonFromException(e);
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
