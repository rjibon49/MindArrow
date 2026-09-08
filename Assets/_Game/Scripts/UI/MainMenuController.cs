using MindArrow.Core;
using MindArrow.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MindArrow.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField]
        private Button playButton;

        [SerializeField]
        private string gameplaySceneName = "Gameplay";

        [Tooltip("If enabled, PLAY opens the highest unlocked level. Disable to keep the last selected level.")]
        [SerializeField]
        private bool playHighestUnlockedLevel;

        private void Awake()
        {
            if (playButton == null)
            {
                Debug.LogError("PlayButton is not assigned.", this);
                return;
            }

            playButton.onClick.AddListener(OnPlayClicked);
        }

        private void OnDestroy()
        {
            if (playButton != null)
            {
                playButton.onClick.RemoveListener(OnPlayClicked);
            }
        }

        private void OnPlayClicked()
        {
            if (playHighestUnlockedLevel)
            {
                GameSession.SelectLevel(SaveService.GetUnlockedLevel());
            }

            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}
