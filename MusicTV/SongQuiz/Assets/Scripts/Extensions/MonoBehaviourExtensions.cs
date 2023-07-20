using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Extensions
{
    public static class MonoBehaviourExtensions
    {
        public static void DisableAllButtons(this MonoBehaviour monoBehaviour)
        {
            foreach (var button in monoBehaviour.GetComponentsInChildren<Button>())
            {
                button.interactable = false;
            }
        }

        public static void EnableAllButtons(this MonoBehaviour monoBehaviour)
        {
            foreach (var button in monoBehaviour.GetComponentsInChildren<Button>())
            {
                button.interactable = true;
            }
        }

        public static void SelectFirstEnabledButton(this MonoBehaviour monoBehaviour, bool excludeXButton = true)
        {
            var enabledButtons = System.Array.FindAll(monoBehaviour.GetComponentsInChildren<Button>(), b => b.interactable);
            if (!(enabledButtons is null))
            {
                foreach (var button in enabledButtons)
                {
                    if (excludeXButton && button.name.Equals("XButton"))
                    {
                        continue;
                    }

                    button.Select();
                    return;
                }
            }
        }
    }
}