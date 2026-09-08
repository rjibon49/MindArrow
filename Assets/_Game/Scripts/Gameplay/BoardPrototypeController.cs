using MindArrow.Board;
using UnityEngine;

namespace MindArrow.Gameplay
{
    public sealed class BoardPrototypeController : MonoBehaviour
    {
        [SerializeField]
        private BoardView boardView;

        private BoardModel boardModel;

        private bool isResolvingMove;

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

            boardModel = new BoardModel(
                10,
                16
            );

            Canvas.ForceUpdateCanvases();

            CreatePrototypeLevel();
        }

        private void CreatePrototypeLevel()
        {
            // CYAN
            // Direction is automatically UP.
            // Initially blocked by Pink.
            ArrowData arrow1 =
                new(
                    1,
                    new Color(
                        0.15f,
                        0.80f,
                        0.95f,
                        1f
                    ),
                    new[]
                    {
                new GridPosition(1, 2),
                new GridPosition(1, 5),
                new GridPosition(2, 5),
                new GridPosition(2, 6)
                    }
                );

            // PINK
            // Direction is automatically RIGHT.
            // Initially blocked by Blue.
            ArrowData arrow2 =
                new(
                    2,
                    new Color(
                        1f,
                        0.35f,
                        0.65f,
                        1f
                    ),
                    new[]
                    {
                new GridPosition(2, 8),
                new GridPosition(2, 9),
                new GridPosition(6, 9)
                    }
                );

            // BLUE
            // Direction is automatically DOWN.
            // Free at the beginning.
            ArrowData arrow3 =
                new(
                    3,
                    new Color(
                        0.35f,
                        0.45f,
                        1f,
                        1f
                    ),
                    new[]
                    {
                new GridPosition(9, 13),
                new GridPosition(8, 13),
                new GridPosition(8, 9)
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
            // Prevent multiple arrows from being resolved
            // while one arrow is escaping.
            if (isResolvingMove)
            {
                return;
            }

            if (!boardModel.TryGetArrow(
                    arrowId,
                    out ArrowData arrow))
            {
                Debug.LogWarning(
                    $"Arrow {arrowId} was not found."
                );

                return;
            }

            bool canEscape =
                boardModel.CanEscape(arrowId);

            if (!canEscape)
            {
                Debug.Log(
                    $"Arrow {arrowId} is BLOCKED."
                );

                boardView.PlayBlockedFeedback(
                    arrowId
                );

                return;
            }

            Debug.Log(
                $"Arrow {arrowId} is escaping."
            );

            isResolvingMove = true;

            boardView.AnimateEscape(
                arrowId,
                arrow.ExitDirection,
                HandleEscapeCompleted
            );
        }

        private void HandleEscapeCompleted(
            int arrowId)
        {
            boardModel.RemoveArrow(
                arrowId
            );

            isResolvingMove = false;

            Debug.Log(
                $"Arrow {arrowId} escaped."
            );

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