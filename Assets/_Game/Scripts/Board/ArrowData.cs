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
        private ArrowDirection exitDirection;

        [SerializeField]
        private List<GridPosition> path = new();

        public int Id => id;

        public Color Color => color;

        public ArrowDirection ExitDirection => exitDirection;

        public IReadOnlyList<GridPosition> Path => path;

        public ArrowData()
        {
        }

        public ArrowData(
            int id,
            Color color,
            ArrowDirection exitDirection,
            IEnumerable<GridPosition> path)
        {
            this.id = id;
            this.color = color;
            this.exitDirection = exitDirection;

            this.path = new List<GridPosition>(path);
        }
    }
}