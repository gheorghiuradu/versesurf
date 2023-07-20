using Assets.Scripts.Services;
using UnityEngine;

namespace Assets.Scripts.Panels
{
    public class ReconnectPanelScript : MonoBehaviour
    {
        public bool IsCancelled { get; private set; }

        public static GameObject Instantiate()
        {
            var prefab = Resources.Load<GameObject>(Constants.ReconnectingPanelPrefabPath);
            var reconnectPanel = GameObject.Instantiate(prefab, GameObject.FindObjectOfType<Canvas>().transform);

            return reconnectPanel;
        }

        public static void Destroy()
        {
            var @object = GameObject.FindObjectOfType<ReconnectPanelScript>();
            if (@object)
            {
                GameObject.Destroy(@object.gameObject);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                this.IsCancelled = true;
            }
        }
    }
}