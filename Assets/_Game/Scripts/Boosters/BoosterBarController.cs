using System.Collections.Generic;
using MindArrow.Board;
using MindArrow.Gameplay;
using MindArrow.Solver;
using UnityEngine;
using UnityEngine.UI;

namespace MindArrow.Boosters
{
    public sealed class BoosterBarController : MonoBehaviour
    {
        [SerializeField]
        private Button hintButton;

        [SerializeField]
        private Button extraTimeButton;

        [SerializeField]
        private BoardPrototypeController gameplayController;

        [SerializeField]
        private LevelTimer levelTimer;

        [SerializeField, Min(1)]
        private int extraTimeSeconds = 30;

        private BoardModel modelProvider;

        private void Awake()
        {
            if (hintButton != null)
            {
                hintButton.onClick.AddListener(OnHint);
            }

            if (extraTimeButton != null)
            {
                extraTimeButton.onClick.AddListener(OnExtraTime);
            }
        }

        private void OnEnable()
        {
            if (gameplayController == null)
            {
                return;
            }

            gameplayController.ModelChanged += BindModel;
            BindModel(gameplayController.Model);
        }

        private void OnDisable()
        {
            if (gameplayController != null)
            {
                gameplayController.ModelChanged -= BindModel;
            }
        }

        private void OnDestroy()
        {
            if (hintButton != null)
            {
                hintButton.onClick.RemoveListener(OnHint);
            }

            if (extraTimeButton != null)
            {
                extraTimeButton.onClick.RemoveListener(OnExtraTime);
            }
        }

        public void BindModel(BoardModel model)
        {
            modelProvider = model;
        }

        private void OnHint()
        {
            if (modelProvider == null ||
                gameplayController == null ||
                gameplayController.IsLevelOver ||
                gameplayController.IsResolvingMove)
            {
                return;
            }

            List<int> escapable =
                BoardSolver.GetEscapableArrows(modelProvider);

            if (escapable.Count == 0)
            {
                Debug.Log("HINT: no free arrow.");
                return;
            }

            // Prototype behavior: console-only hint.
            // Do NOT use blocked shake as hint feedback.
            Debug.Log($"HINT: try arrow {escapable[0]}");
        }

        private void OnExtraTime()
        {
            if (levelTimer == null ||
                gameplayController == null ||
                gameplayController.IsLevelOver ||
                !levelTimer.IsRunning ||
                levelTimer.IsExpired)
            {
                return;
            }

            levelTimer.AddSeconds(extraTimeSeconds);
        }
    }
}
