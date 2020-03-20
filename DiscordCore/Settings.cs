using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiscordCore
{
    public class Settings : PersistentSingleton<Settings>
    {
        [UIValue("enable-plugin")]
        public bool enablePlugin;
        [UIValue("allow-join")]
        public bool allowJoinRequests;
        [UIValue("allow-spectator")]
        public bool allowSpectatorRequests;
        [UIValue("allow-invites")]
        public bool allowInvites;

        [UIComponent("mods-list")]
        public CustomCellListTableData modsList;

        [UIValue("mods")]
        public List<object> modObjectsList = new List<object>();

        [UIAction("#post-parse")]
        public void UpdateModsList()
        {
            if (modsList != null)
            {
                try
                {
                    if (DiscordManager.Instance._activeInstances != null)
                    {
                        var instances = DiscordManager.Instance._activeInstances.OrderBy(y => y.Priority);

                        if (modObjectsList.Count != DiscordManager.Instance._activeInstances.Count)
                        {
                            modObjectsList.Clear();

                            foreach (var instance in instances)
                            {
                                var listObject = new ModListObject(instance);

                                listObject.activeStateChanged += ListObject_activeStateChanged;
                                listObject.increasePriorityPressed += ListObject_increasePriorityPressed;
                                listObject.decreasePriorityPressed += ListObject_decreasePriorityPressed;

                                modObjectsList.Add(listObject);
                            }

                            modsList.tableView.ReloadData();
                        }
                        else
                        {
                            for (int i = 0; i < modObjectsList.Count; i++)
                                (modObjectsList[i] as ModListObject).ReplaceModInstance(instances.ElementAt(i));
                        }
                    }
                }
                catch (Exception e)
                {
                    Plugin.log.Error($"Unable to update mods list in settings! Exception: {e}");
                }
            }
        }

        private void ListObject_increasePriorityPressed(DiscordInstance sender)
        {
            var nextInstance = DiscordManager.Instance._activeInstances.OrderByDescending(x => x.Priority).FirstOrDefault(x => x.Priority < sender.Priority);

            if (nextInstance != null)
            {
                sender.Priority = nextInstance.Priority - 1;
                UpdateModsList();
            }
        }

        private void ListObject_decreasePriorityPressed(DiscordInstance sender)
        {
            var prevInstance = DiscordManager.Instance._activeInstances.OrderBy(x => x.Priority).FirstOrDefault(x => x.Priority > sender.Priority);

            if (prevInstance != null)
            {
                sender.Priority = prevInstance.Priority + 1;
                UpdateModsList();
            }
        }

        private void ListObject_activeStateChanged(DiscordInstance sender, bool newState)
        {

        }

        public class ModListObject
        {
            public event Action<DiscordInstance> increasePriorityPressed;
            public event Action<DiscordInstance> decreasePriorityPressed;
            public event Action<DiscordInstance, bool> activeStateChanged;

            private DiscordInstance modInstance;

            [UIComponent("mod-name")]
            private TextMeshProUGUI modName;

            [UIComponent("mod-icon")]
            private Image modIcon;

            [UIValue("enable-mod")]
            private bool enableMod;

            public ModListObject(DiscordInstance instance)
            {
                modInstance = instance;
            }

            public void ReplaceModInstance(DiscordInstance instance)
            {
                modInstance = instance;

                if (modIcon != null && modName != null)
                    Refresh(false, false);
            }

            [UIAction("refresh-visuals")]
            public void Refresh(bool selected, bool highlighted)
            {
                modName.text = modInstance.settings.modName;
                modIcon.sprite = modInstance.settings.modIcon;
            }

            [UIAction("increase-priority")]
            private void IncreasePriority()
            {
                increasePriorityPressed?.Invoke(modInstance);
            }

            [UIAction("decrease-priority")]
            private void DecreasePriority()
            {
                decreasePriorityPressed?.Invoke(modInstance);
            }

            [UIAction("active-state-changed")]
            private void ActiveStateChanged(bool newState)
            {
                activeStateChanged?.Invoke(modInstance, newState);
            }
        }

    }
}
