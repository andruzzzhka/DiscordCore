using BeatSaberMarkupLanguage.Settings;
using BS_Utils.Utilities;
using IPA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiscordCore
{
    public class Plugin : IBeatSaberPlugin
    {
        internal static IPA.Logging.Logger log;

        public static bool active = true;
        public static string deactivationReason;

        private static float lastCheckTime;

        public void Init(IPA.Logging.Logger log)
        {
            Plugin.log = log;
        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {

        }

        public void OnApplicationQuit()
        {

        }

        public void OnApplicationStart()
        {
            BSEvents.menuSceneLoadedFresh += BSEvents_menuSceneLoadedFresh;
        }

        private void BSEvents_menuSceneLoadedFresh()
        {
            BSMLSettings.instance.AddSettingsMenu("DiscordCore", "DiscordCore.UI.SettingsViewController.bsml", Settings.instance);
        }

        public void OnFixedUpdate()
        {

        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {

        }

        public void OnSceneUnloaded(Scene scene)
        {

        }

        public void OnUpdate()
        {
            if (UnityEngine.Time.time - lastCheckTime >= 10f)
            {
                if (!active)
                {
                    lastCheckTime = UnityEngine.Time.time;
                    log.Debug("DiscordCore is not active! Reason: " + deactivationReason);
                }
            }

            if (active)
            {
                DiscordManager.Instance.Update();
                DiscordClient.RunCallbacks();
            }
        }
    }
}
