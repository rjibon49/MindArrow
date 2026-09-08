using System;
using MindArrow.Board;
using MindArrow.Core;
using MindArrow.Levels;
using MindArrow.Save;
using MindArrow.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MindArrow.Gameplay
{
    public sealed class BoardPrototypeController : MonoBehaviour
    {
        [Header("Board")]
        [SerializeField]
        private BoardView boardView;

        [Header("Level")]
        [SerializeField]
        private LevelCatalog levelCatalog;

        [Tooltip("Optional. Used only if the requested catalog level is missing/invalid.")]
        [SerializeField]
        private LevelData fallbackLevel;

        [Header("Timer")]
        [SerializeField]
        private LevelTimer levelTimer;

        [Header("UI")]
        [SerializeField]
        private TMP_Text levelText;

        [SerializeField]
        private LevelResultPanel resultPanel;

        [SerializeField]
        private string mainMenuSceneName = "MainMenu";

        private BoardModel boardModel;
        private LevelData activeLevel;
        private int activeLevelNumber;
        private bool isResolvingMove;
        private bool isLevelOver;

        public BoardModel Model => boardModel;
        public bool IsLevelOver => isLevelOver;
        public bool IsResolvingMove => isResolvingMove;
        public int ActiveLevelNumber => activeLevelNumber;

        public event Action<BoardModel> ModelChanged;

        private void Start()
        {
            if (boardView == null)
            {
                Debug.LogError("BoardView is not assigned.", this);
                return;
            }

            if (resultPanel != null)
            {
                resultPanel.RetryClicked += RestartLevel;
                resultPanel.NextClicked += LoadNextLevel;
                resultPanel.HomeClicked += GoHome;
            }

            if (levelTimer != null)
            {
                levelTimer.TimedOut += HandleTimeOut;
            }

            LoadSelectedLevel();
        }

        private void OnDestroy()
        {
            if (resultPanel != null)
            {
                resultPanel.RetryClicked -= RestartLevel;
                resultPanel.NextClicked -= LoadNextLevel;
                resultPanel.HomeClicked -= GoHome;
            }

            if (levelTimer != null)
            {
                levelTimer.TimedOut -= HandleTimeOut;
            }
        }

        private void LoadSelectedLevel()
        {
            activeLevel = ResolveLevel(GameSession.SelectedLevelNumber);

            bool usePrototype = activeLevel == null;

            int columns = usePrototype
                ? PrototypeLevelFactory.DefaultColumns
                : activeLevel.Columns;

            int rows = usePrototype
                ? PrototypeLevelFactory.DefaultRows
                : activeLevel.Rows;

            int seconds = usePrototype
                ? PrototypeLevelFactory.DefaultTimeLimitSeconds
                : activeLevel.TimeLimitSeconds;

            activeLevelNumber = usePrototype
                ? PrototypeLevelFactory.DefaultLevelNumber
                : activeLevel.LevelNumber;

            GameSession.SelectLevel(activeLevelNumber);

            if (levelText != null)
            {
                levelText.text = $"LEVEL {activeLevelNumber}";
            }

            if (levelTimer != null)
            {
                levelTimer.StopTimer();
            }

            if (resultPanel != null)
            {
                resultPanel.Hide();
            }

            boardView.Clear();
            boardView.ConfigureGrid(columns, rows);

            // Make sure the BoardArea RectTransform has its final dimensions
            // before GridPosition -> local UI conversion happens.
            Canvas.ForceUpdateCanvases();

            boardModel = new BoardModel(columns, rows);
            isResolvingMove = false;
            isLevelOver = false;

            if (usePrototype)
            {
                Debug.LogWarning(
                    "No valid LevelData could be resolved. Using the built-in prototype level.",
                    this);

                SpawnArrows(PrototypeLevelFactory.CreatePrototypeArrows());
            }
            else
            {
                SpawnArrows(activeLevel.Arrows);
            }

            ModelChanged?.Invoke(boardModel);

            if (levelTimer != null)
            {
                levelTimer.Initialize(seconds);
                levelTimer.StartTimer();
            }

            Debug.Log($"Level {activeLevelNumber} loaded.");
        }

        private LevelData ResolveLevel(int requestedNumber)
        {
            if (levelCatalog != null &&
                levelCatalog.TryGetByNumber(requestedNumber, out LevelData requested))
            {
                return requested;
            }

            if (fallbackLevel != null)
            {
                if (fallbackLevel.IsValid(out string fallbackError))
                {
                    return fallbackLevel;
                }

                Debug.LogWarning(
                    $"Fallback LevelData '{fallbackLevel.name}' is invalid: {fallbackError}",
                    fallbackLevel);
            }

            if (levelCatalog != null)
            {
                LevelData first = levelCatalog.GetFirst();

                if (first != null)
                {
                    return first;
                }
            }

            return null;
        }

        private void SpawnArrows(System.Collections.Generic.IReadOnlyList<ArrowData> arrows)
        {
            if (arrows == null)
            {
                return;
            }

            for (int i = 0; i < arrows.Count; i++)
            {
                AddArrow(arrows[i]);
            }
        }

        private void AddArrow(ArrowData arrow)
        {
            if (arrow == null)
            {
                return;
            }

            boardModel.AddArrow(arrow);
            boardView.RenderArrow(arrow, HandleArrowClicked);
        }

        private void HandleArrowClicked(int arrowId)
        {
            if (isLevelOver || isResolvingMove)
            {
                return;
            }

            if (!boardModel.TryGetArrow(arrowId, out ArrowData arrow))
            {
                Debug.LogWarning($"Arrow {arrowId} was not found.", this);
                return;
            }

            if (!boardModel.CanEscape(arrowId))
            {
                Debug.Log($"Arrow {arrowId} is BLOCKED.");
                boardView.PlayBlockedFeedback(arrowId);
                return;
            }

            Debug.Log($"Arrow {arrowId} is escaping.");
            isResolvingMove = true;

            boardView.AnimateEscape(
                arrowId,
                arrow.ExitDirection,
                HandleEscapeCompleted);
        }

        private void HandleEscapeCompleted(int arrowId)
        {
            if (boardModel != null)
            {
                boardModel.RemoveArrow(arrowId);
            }

            isResolvingMove = false;
            Debug.Log($"Arrow {arrowId} escaped.");

            if (!isLevelOver)
            {
                CheckWin();
            }
        }

        private void CheckWin()
        {
            if (isLevelOver ||
                boardModel == null ||
                boardModel.ArrowCount > 0)
            {
                return;
            }

            isLevelOver = true;
            levelTimer?.StopTimer();

            bool hasNext = TryGetNextLevel(out LevelData nextLevel);

            // IMPORTANT: unlock the actual next level number, not current + 1.
            // This also works for sparse catalogs such as 1, 5, 10.
            if (hasNext)
            {
                SaveService.UnlockUpTo(nextLevel.LevelNumber);
            }

            Debug.Log("LEVEL COMPLETE!");
            resultPanel?.ShowWin(hasNext);
        }

        private void HandleTimeOut()
        {
            if (isLevelOver)
            {
                return;
            }

            isLevelOver = true;
            Debug.Log("TIME UP!");
            resultPanel?.ShowTimeUp();
        }

        private void RestartLevel()
        {
            LoadSelectedLevel();
        }

        private void LoadNextLevel()
        {
            if (TryGetNextLevel(out LevelData nextLevel))
            {
                GameSession.SelectLevel(nextLevel.LevelNumber);
                LoadSelectedLevel();
                return;
            }

            GoHome();
        }

        private bool TryGetNextLevel(out LevelData nextLevel)
        {
            nextLevel = null;

            if (activeLevel == null || levelCatalog == null)
            {
                return false;
            }

            return levelCatalog.TryGetNext(
                activeLevel.LevelNumber,
                out nextLevel);
        }

        private void GoHome()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
