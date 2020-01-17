using IPA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (active)
            {
                DiscordManager.Instance.Update();
                DiscordClient.RunCallbacks();
            }
            else
            {
                if (UnityEngine.Time.time - lastCheckTime >= 10f)
                {
                    lastCheckTime = UnityEngine.Time.time;
                    log.Error("DiscordCore is not active! Reason: " + deactivationReason);
                }
            }
        }
    }
}
