using MindArrow.Board;
using UnityEngine;

namespace MindArrow.Gameplay
{
    public sealed class BoardPrototypeController
        : MonoBehaviour
    {
        [SerializeField]
        private BoardView boardView;

        private BoardModel boardModel;

        private void Start()
        {
            if (boardView == null)
            {
                Debug.LogError(
                    "BoardView is not assigned.",
                    this
                );

                return;
            }

            boardModel =
                new BoardModel(
                    10,
                    16
                );

            Canvas.ForceUpdateCanvases();

            CreatePrototypeLevel();
        }

        private void CreatePrototypeLevel()
        {
            // CYAN:
            // Blocked by Arrow 2.
            ArrowData arrow1 =
                new(
                    1,
                    new Color(
                        0.15f,
                        0.80f,
                        0.95f
                    ),
                    ArrowDirection.Right,
                    new[]
                    {
                new GridPosition(1, 3),
                new GridPosition(1, 7),
                new GridPosition(4, 7),
                new GridPosition(4, 9)
                    }
                );

            // PINK:
            // Blocked by Arrow 3.
            ArrowData arrow2 =
                new(
                    2,
                    new Color(
                        1f,
                        0.35f,
                        0.65f
                    ),
                    ArrowDirection.Right,
                    new[]
                    {
                new GridPosition(3, 11),
                new GridPosition(6, 11),
                new GridPosition(6, 8)
                    }
                );

            // BLUE:
            // Free at the beginning.
            ArrowData arrow3 =
                new(
                    3,
                    new Color(
                        0.35f,
                        0.45f,
                        1f
                    ),
                    ArrowDirection.Right,
                    new[]
                    {
                new GridPosition(8, 10),
                new GridPosition(8, 13)
                    }
                );

            AddArrow(arrow1);
            AddArrow(arrow2);
            AddArrow(arrow3);
        }

        private void AddArrow(
            ArrowData arrow)
        {
            boardModel.AddArrow(arrow);

            boardView.RenderArrow(
                arrow,
                HandleArrowClicked
            );
        }

        private void HandleArrowClicked(
            int arrowId)
        {
            bool canEscape =
                boardModel.CanEscape(arrowId);

            if (!canEscape)
            {
                Debug.Log(
                    $"Arrow {arrowId} is BLOCKED."
                );

                return;
            }

            Debug.Log(
                $"Arrow {arrowId} escaped."
            );

            boardModel.RemoveArrow(arrowId);

            boardView.RemoveArrow(arrowId);

            CheckWin();
        }

        private void CheckWin()
        {
            if (boardModel.ArrowCount > 0)
            {
                return;
            }

            Debug.Log(
                "LEVEL COMPLETE!"
            );
        }
    }
}