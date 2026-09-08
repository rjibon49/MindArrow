using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MindArrow.UI
{
    public sealed class LevelResultPanel : MonoBehaviour
    {
        [Header("Root")]
        [Tooltip("Recommended: assign a CHILD panel root. Keep this script's GameObject active.")]
        [SerializeField]
        private GameObject panelRoot;

        [Header("Labels")]
        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text messageText;

        [Header("Buttons")]
        [SerializeField]
        private Button retryButton;

        [SerializeField]
        private Button nextButton;

        [SerializeField]
        private Button homeButton;

        public event Action RetryClicked;
        public event Action NextClicked;
        public event Action HomeClicked;

        public bool IsVisible => GetRoot().activeSelf;

        private void Awake()
        {
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(HandleRetryClicked);
            }

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(HandleNextClicked);
            }

            if (homeButton != null)
            {
                homeButton.onClick.AddListener(HandleHomeClicked);
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(HandleRetryClicked);
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(HandleNextClicked);
            }

            if (homeButton != null)
            {
                homeButton.onClick.RemoveListener(HandleHomeClicked);
            }
        }

        public void ShowWin(bool hasNextLevel)
        {
            Show(
                "LEVEL COMPLETE",
                "All arrows escaped.",
                showNext: hasNextLevel);
        }

        public void ShowTimeUp()
        {
            Show(
                "TIME UP",
                "The timer reached 00:00.",
                showNext: false);
        }

        public void Hide()
        {
            GetRoot().SetActive(false);
        }

        private void Show(string title, string message, bool showNext)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (messageText != null)
            {
                messageText.text = message;
            }

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(showNext);
            }

            GetRoot().SetActive(true);
        }

        private GameObject GetRoot()
        {
            return panelRoot != null ? panelRoot : gameObject;
        }

        private void HandleRetryClicked()
        {
            RetryClicked?.Invoke();
        }

        private void HandleNextClicked()
        {
            NextClicked?.Invoke();
        }

        private void HandleHomeClicked()
        {
            HomeClicked?.Invoke();
        }
    }
}
