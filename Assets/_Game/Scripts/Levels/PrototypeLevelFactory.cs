using MindArrow.Board;
using UnityEngine;

namespace MindArrow.Levels
{
    public static class PrototypeLevelFactory
    {
        public const int DefaultLevelNumber = 1;
        public const int DefaultColumns = 10;
        public const int DefaultRows = 16;
        public const int DefaultTimeLimitSeconds = 180;

        public static ArrowData[] CreatePrototypeArrows()
        {
            return new[]
            {
                CreateCyan(),
                CreatePink(),
                CreateBlue()
            };
        }

        private static ArrowData CreateCyan()
        {
            return new ArrowData(
                1,
                new Color(0.15f, 0.80f, 0.95f, 1f),
                new[]
                {
                    new GridPosition(1, 2),
                    new GridPosition(1, 5),
                    new GridPosition(2, 5),
                    new GridPosition(2, 6)
                });
        }

        private static ArrowData CreatePink()
        {
            return new ArrowData(
                2,
                new Color(1f, 0.35f, 0.65f, 1f),
                new[]
                {
                    new GridPosition(2, 8),
                    new GridPosition(2, 9),
                    new GridPosition(6, 9)
                });
        }

        private static ArrowData CreateBlue()
        {
            return new ArrowData(
                3,
                new Color(0.35f, 0.45f, 1f, 1f),
                new[]
                {
                    new GridPosition(9, 13),
                    new GridPosition(8, 13),
                    new GridPosition(8, 9)
                });
        }
    }
}
