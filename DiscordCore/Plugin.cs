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
        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
            if (nextScene.name.Contains("Menu"))
            {

            }

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
            DiscordManager.Instance.Update();
        }
    }
}
