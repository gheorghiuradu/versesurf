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

        [SerializeField] private Button resumeButton;

        private UnityEvent onResume;
        private UnityEvent onExit;

        private void Start()
        {
            resumeButton.Select();
        }

        private void LateUpdate()
        {
            if (transform.IsLastChild() && EventSystem.current.currentSelectedGameObject == null) resumeButton.Select();
            //if (this.transform.IsLastChild() && Input.GetKeyDown(KeyCode.Escape))
            //{
            //    this.Resume();
            //}
        }

        public void Resume()
        {
            onExit.RemoveAllListeners();
            onResume.Invoke();
            onResume.RemoveAllListeners();
            Destroy(gameObject);
        }

        public void Exit()
        {
            onResume.RemoveAllListeners();
            onExit.Invoke();
            onExit.RemoveAllListeners();
            SceneFader.Fade("MainMenu", Color.black, 2);
        }

        public static SimplePausePanel Instantiate(
            Transform parent = null,
            bool pauseTime = true,
            UnityAction onResume = null,
            UnityAction onExit = null)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);
            if (parent == null) parent = FindObjectOfType<Canvas>().transform;
            var script = GameObject.Instantiate(prefab, parent).GetComponent<SimplePausePanel>();
            script.onResume = new UnityEvent();
            script.onExit = new UnityEvent();

            if (!(onResume is null)) script.onResume.AddListener(onResume);
            if (!(onExit is null)) script.onExit.AddListener(onExit);

            if (pauseTime)
            {
                Time.timeScale = 0;

                void resumeTime()
                {
                    Time.timeScale = 1;
                }

                script.onResume.AddListener(resumeTime);
                script.onExit.AddListener(resumeTime);
            }

            return script;
        }
    }
}