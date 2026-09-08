using System.Collections.Generic;
using UnityEngine;

namespace MindArrow.Board
{
    public sealed class BoardModel
    {
        private readonly int columns;
        private readonly int rows;

        private readonly Dictionary<int, ArrowData>
            arrows = new();

        public int ArrowCount => arrows.Count;

        public BoardModel(int columns, int rows)
        {
            this.columns = columns;
            this.rows = rows;
        }

        public void AddArrow(ArrowData arrow)
        {
            if (arrow == null)
            {
                Debug.LogError("Cannot add null arrow.");
                return;
            }

            arrows[arrow.Id] = arrow;
        }

        public bool ContainsArrow(int arrowId)
        {
            return arrows.ContainsKey(arrowId);
        }

        public void RemoveArrow(int arrowId)
        {
            arrows.Remove(arrowId);
        }

        public bool CanEscape(int arrowId)
        {
            if (!arrows.TryGetValue(
                    arrowId,
                    out ArrowData arrow))
            {
                return false;
            }

            HashSet<GridPosition> movingCells =
                ExpandPath(arrow);

            HashSet<GridPosition> otherCells =
                BuildOtherOccupancy(arrowId);

            GetDirectionDelta(
                arrow.ExitDirection,
                out int deltaX,
                out int deltaY
            );

            int maxSteps =
                columns + rows + 4;

            for (int step = 1;
                 step <= maxSteps;
                 step++)
            {
                bool allOutside = true;

                foreach (GridPosition originalCell
                         in movingCells)
                {
                    GridPosition translated =
                        originalCell.Translate(
                            deltaX * step,
                            deltaY * step
                        );

                    if (!IsInside(translated))
                    {
                        continue;
                    }

                    allOutside = false;

                    if (otherCells.Contains(translated))
                    {
                        return false;
                    }
                }

                if (allOutside)
                {
                    return true;
                }
            }

            return false;
        }

        private HashSet<GridPosition>
            BuildOtherOccupancy(int excludedArrowId)
        {
            HashSet<GridPosition> occupied =
                new();

            foreach (KeyValuePair<int, ArrowData> pair
                     in arrows)
            {
                if (pair.Key == excludedArrowId)
                {
                    continue;
                }

                occupied.UnionWith(
                    ExpandPath(pair.Value)
                );
            }

            return occupied;
        }

        private static HashSet<GridPosition>
            ExpandPath(ArrowData arrow)
        {
            HashSet<GridPosition> cells =
                new();

            IReadOnlyList<GridPosition> path =
                arrow.Path;

            if (path == null ||
                path.Count == 0)
            {
                return cells;
            }

            cells.Add(path[0]);

            for (int i = 0;
                 i < path.Count - 1;
                 i++)
            {
                GridPosition start =
                    path[i];

                GridPosition end =
                    path[i + 1];

                int dx =
                    System.Math.Sign(
                        end.X - start.X
                    );

                int dy =
                    System.Math.Sign(
                        end.Y - start.Y
                    );

                bool horizontal =
                    start.Y == end.Y;

                bool vertical =
                    start.X == end.X;

                if (!horizontal &&
                    !vertical)
                {
                    Debug.LogError(
                        $"Arrow {arrow.Id} has " +
                        "a diagonal path."
                    );

                    continue;
                }

                GridPosition current =
                    start;

                while (current != end)
                {
                    current =
                        current.Translate(dx, dy);

                    cells.Add(current);
                }
            }

            return cells;
        }

        private bool IsInside(
            GridPosition position)
        {
            return
                position.X >= 0 &&
                position.X < columns &&
                position.Y >= 0 &&
                position.Y < rows;
        }

        private static void GetDirectionDelta(
            ArrowDirection direction,
            out int deltaX,
            out int deltaY)
        {
            deltaX = 0;
            deltaY = 0;

            switch (direction)
            {
                case ArrowDirection.Up:
                    deltaY = 1;
                    break;

                case ArrowDirection.Right:
                    deltaX = 1;
                    break;

                case ArrowDirection.Down:
                    deltaY = -1;
                    break;

                case ArrowDirection.Left:
                    deltaX = -1;
                    break;
            }
        }
    }
}