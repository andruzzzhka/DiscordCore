using IPA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace DiscordCore
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static IPA.Logging.Logger log;

        [Init]
        public void Init(IPA.Logging.Logger log)
        {
            Plugin.log = log;
            DiscordManager manager = DiscordManager.instance;
        }
    }
}
