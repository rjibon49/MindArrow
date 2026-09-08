using System;
using UnityEngine;

namespace MindArrow.Board
{
    [Serializable]
    public struct GridPosition
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
    }
}