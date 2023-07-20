using Assets.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Reusable
{
    public class SimplePausePanel : MonoBehaviour
    {
        private const string PrefabPath = "Prefabs/SimplePausePanel";

        [SerializeField]
        private Button resumeButton;

        private UnityEvent onResume;
        private UnityEvent onExit;

        private void Start()
        {
            this.resumeButton.Select();
        }

        private void LateUpdate()
        {
            if (this.transform.IsLastChild() && EventSystem.current.currentSelectedGameObject == null)
            {
                this.resumeButton.Select();
            }
            //if (this.transform.IsLastChild() && Input.GetKeyDown(KeyCode.Escape))
            //{
            //    this.Resume();
            //}
        }

        public void Resume()
        {
            this.onExit.RemoveAllListeners();
            this.onResume.Invoke();
            this.onResume.RemoveAllListeners();
            GameObject.Destroy(this.gameObject);
        }

        public void Exit()
        {
            this.onResume.RemoveAllListeners();
            this.onExit.Invoke();
            this.onExit.RemoveAllListeners();
            SceneFader.Fade("MainMenu", Color.black, 2);
        }

        public static SimplePausePanel Instantiate(
            Transform parent = null,
            bool pauseTime = true,
            UnityAction onResume = null,
            UnityAction onExit = null)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);
            if (parent == null)
            {
                parent = GameObject.FindObjectOfType<Canvas>().transform;
            }
            var script = GameObject.Instantiate(prefab, parent).GetComponent<SimplePausePanel>();
            script.onResume = new UnityEvent();
            script.onExit = new UnityEvent();

            if (!(onResume is null))
            {
                script.onResume.AddListener(onResume);
            }
            if (!(onExit is null))
            {
                script.onExit.AddListener(onExit);
            }

            if (pauseTime)
            {
                Time.timeScale = 0;
                void resumeTime() => Time.timeScale = 1;
                script.onResume.AddListener(resumeTime);
                script.onExit.AddListener(resumeTime);
            }

            return script;
        }
    }
}