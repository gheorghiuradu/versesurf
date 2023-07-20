using Assets.Scripts.Reusable;
using UnityEngine;

namespace Assets.Scripts.Panels
{
    public static class PanelManager
    {
        public static void CloseAllOpenPanels()
        {
            var openPanels = GameObject.FindObjectOfType<Canvas>().GetComponentsInChildren<IClosablePanel>();
            foreach (var panel in openPanels)
            {
                panel.Close();
            }
        }
    }
}