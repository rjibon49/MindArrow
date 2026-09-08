using System;
using System.Collections.Generic;
using UnityEngine;

namespace MindArrow.Board
{
    [Serializable]
    public sealed class ArrowData
    {
        [SerializeField]
        private int id;

        [SerializeField]
        private Color color = Color.cyan;

        [SerializeField]
        private List<GridPosition> path = new();

        public int Id => id;

        public Color Color => color;

        public IReadOnlyList<GridPosition> Path => path;

        public ArrowDirection ExitDirection =>
            CalculateExitDirection();

        public ArrowData()
        {
        }

        public ArrowData(
            int id,
            Color color,
            IEnumerable<GridPosition> path)
        {
            this.id = id;
            this.color = color;

            this.path =
                new List<GridPosition>(path);
        }

        private ArrowDirection CalculateExitDirection()
        {
            if (path == null ||
                path.Count < 2)
            {
                Debug.LogError(
                    $"Arrow {id} does not have enough path points."
                );

                return ArrowDirection.Right;
            }

            // Search backwards for the last valid segment.
            for (int i = path.Count - 1;
                 i > 0;
                 i--)
            {
                GridPosition end =
                    path[i];

                GridPosition previous =
                    path[i - 1];

                if (end == previous)
                {
                    continue;
                }

                // Vertical final segment.
                if (end.X == previous.X)
                {
                    return end.Y > previous.Y
                        ? ArrowDirection.Up
                        : ArrowDirection.Down;
                }

                // Horizontal final segment.
                if (end.Y == previous.Y)
                {
                    return end.X > previous.X
                        ? ArrowDirection.Right
                        : ArrowDirection.Left;
                }

                Debug.LogError(
                    $"Arrow {id} has a diagonal final segment: " +
                    $"{previous} -> {end}"
                );

                return ArrowDirection.Right;
            }

            Debug.LogError(
                $"Arrow {id} has no valid final segment."
            );

            return ArrowDirection.Right;
        }
    }
}