using System;
using TMPro;
using UnityEngine;

namespace MindArrow.Gameplay
{
    public sealed class LevelTimer : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text timerText;

        public float RemainingSeconds { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsExpired { get; private set; }

        public event Action TimedOut;

        public void Initialize(int seconds)
        {
            RemainingSeconds = Mathf.Max(0, seconds);
            IsExpired = RemainingSeconds <= 0f;
            IsRunning = false;
            RefreshLabel();
        }

        public void StartTimer()
        {
            if (IsExpired || RemainingSeconds <= 0f)
            {
                IsRunning = false;
                return;
            }

            IsRunning = true;
        }

        public void StopTimer()
        {
            IsRunning = false;
        }

        public void AddSeconds(int seconds)
        {
            if (seconds <= 0 || IsExpired)
            {
                return;
            }

            RemainingSeconds += seconds;
            RefreshLabel();
        }

        private void Update()
        {
            if (!IsRunning)
            {
                return;
            }

            RemainingSeconds -= Time.deltaTime;

            if (RemainingSeconds <= 0f)
            {
                RemainingSeconds = 0f;
                IsRunning = false;
                IsExpired = true;
                RefreshLabel();
                TimedOut?.Invoke();
                return;
            }

            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (timerText == null)
            {
                return;
            }

            int total = Mathf.CeilToInt(RemainingSeconds);
            int minutes = total / 60;
            int seconds = total % 60;
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
