using MindArrow.Board;
using UnityEngine;

namespace MindArrow.Gameplay
{
    public sealed class BoardPrototypeController
        : MonoBehaviour
    {
        [SerializeField]
        private BoardView boardView;

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

            Canvas.ForceUpdateCanvases();

            ArrowData testArrow =
                new ArrowData(
                    1,
                    new Color(
                        0.15f,
                        0.80f,
                        0.95f,
                        1f
                    ),
                    ArrowDirection.Right,
                    new[]
                    {
                        new GridPosition(1, 2),
                        new GridPosition(1, 7),
                        new GridPosition(4, 7),
                        new GridPosition(4, 11),
                        new GridPosition(8, 11)
                    }
                );

            boardView.RenderArrow(testArrow);
        }
    }
}