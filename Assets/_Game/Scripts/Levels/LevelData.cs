using System.Collections.Generic;
using MindArrow.Board;
using UnityEngine;

namespace MindArrow.Levels
{
    [CreateAssetMenu(
        fileName = "Level_001",
        menuName = "MindArrow/Level Data",
        order = 0)]
    public sealed class LevelData : ScriptableObject
    {
        [SerializeField, Min(1)]
        private int levelNumber = 1;

        [SerializeField]
        private string displayName = "Level 1";

        [SerializeField, Min(2)]
        private int columns = 10;

        [SerializeField, Min(2)]
        private int rows = 16;

        [SerializeField, Min(1)]
        private int timeLimitSeconds = 180;

        [SerializeField]
        private List<ArrowData> arrows = new();

        public int LevelNumber => levelNumber;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName)
                ? $"Level {levelNumber}"
                : displayName;

        public int Columns => columns;
        public int Rows => rows;
        public int TimeLimitSeconds => timeLimitSeconds;
        public IReadOnlyList<ArrowData> Arrows => arrows;

        public bool IsValid(out string error)
        {
            if (levelNumber < 1)
            {
                error = "Level number must be 1 or higher.";
                return false;
            }

            if (columns < 2 || rows < 2)
            {
                error = "Grid must be at least 2x2.";
                return false;
            }

            if (timeLimitSeconds < 1)
            {
                error = "Time limit must be at least 1 second.";
                return false;
            }

            if (arrows == null || arrows.Count == 0)
            {
                error = "Level must contain at least one arrow.";
                return false;
            }

            HashSet<int> ids = new();
            HashSet<GridPosition> globallyOccupied = new();

            for (int i = 0; i < arrows.Count; i++)
            {
                ArrowData arrow = arrows[i];

                if (arrow == null)
                {
                    error = $"Arrow at index {i} is null.";
                    return false;
                }

                if (arrow.Id < 1)
                {
                    error = $"Arrow at index {i} has invalid id {arrow.Id}.";
                    return false;
                }

                if (!ids.Add(arrow.Id))
                {
                    error = $"Duplicate arrow id: {arrow.Id}";
                    return false;
                }

                if (arrow.Path == null || arrow.Path.Count < 2)
                {
                    error = $"Arrow {arrow.Id} needs at least 2 path points.";
                    return false;
                }

                if (!TryExpandAndValidateArrow(
                        arrow,
                        out HashSet<GridPosition> occupied,
                        out error))
                {
                    return false;
                }

                foreach (GridPosition cell in occupied)
                {
                    if (!globallyOccupied.Add(cell))
                    {
                        error =
                            $"Arrow {arrow.Id} overlaps another arrow at {cell}.";
                        return false;
                    }
                }
            }

            error = string.Empty;
            return true;
        }

        private bool TryExpandAndValidateArrow(
            ArrowData arrow,
            out HashSet<GridPosition> occupied,
            out string error)
        {
            occupied = new HashSet<GridPosition>();

            GridPosition first = arrow.Path[0];

            if (!IsInside(first))
            {
                error = $"Arrow {arrow.Id} starts outside the grid at {first}.";
                return false;
            }

            occupied.Add(first);

            for (int i = 0; i < arrow.Path.Count - 1; i++)
            {
                GridPosition start = arrow.Path[i];
                GridPosition end = arrow.Path[i + 1];

                if (!IsInside(start) || !IsInside(end))
                {
                    error =
                        $"Arrow {arrow.Id} contains an out-of-grid path point: " +
                        $"{start} -> {end}.";
                    return false;
                }

                if (start == end)
                {
                    error =
                        $"Arrow {arrow.Id} contains a zero-length segment at {start}.";
                    return false;
                }

                bool horizontal = start.Y == end.Y;
                bool vertical = start.X == end.X;

                if (!horizontal && !vertical)
                {
                    error =
                        $"Arrow {arrow.Id} contains a diagonal segment: " +
                        $"{start} -> {end}.";
                    return false;
                }

                int dx = System.Math.Sign(end.X - start.X);
                int dy = System.Math.Sign(end.Y - start.Y);
                GridPosition current = start;

                while (current != end)
                {
                    current = current.Translate(dx, dy);

                    if (!occupied.Add(current))
                    {
                        error =
                            $"Arrow {arrow.Id} crosses/reuses its own path at {current}.";
                        return false;
                    }
                }
            }

            error = string.Empty;
            return true;
        }

        private bool IsInside(GridPosition position)
        {
            return
                position.X >= 0 &&
                position.X < columns &&
                position.Y >= 0 &&
                position.Y < rows;
        }
    }
}
