using System;
using UnityEngine;

namespace MindArrow.Board
{
    [Serializable]
    public struct GridPosition : IEquatable<GridPosition>
    {
        [SerializeField]
        private int x;

        [SerializeField]
        private int y;

        public int X => x;
        public int Y => y;

        public GridPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public GridPosition Translate(int deltaX, int deltaY)
        {
            return new GridPosition(
                x + deltaX,
                y + deltaY
            );
        }

        public bool Equals(GridPosition other)
        {
            return x == other.x &&
                   y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPosition other &&
                   Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public static bool operator ==(
            GridPosition left,
            GridPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            GridPosition left,
            GridPosition right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}