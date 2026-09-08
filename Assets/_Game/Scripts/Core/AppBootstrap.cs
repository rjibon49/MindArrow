using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MindArrow.Core
{
    public sealed class AppBootstrap : MonoBehaviour
    {
        private static AppBootstrap _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private IEnumerator Start()
        {
            yield return SceneManager.LoadSceneAsync(
                "MainMenu",
                LoadSceneMode.Single
            );
        }
    }
}