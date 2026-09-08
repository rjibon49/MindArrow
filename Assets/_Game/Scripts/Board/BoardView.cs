using System;
using System.Collections.Generic;
using MindArrow.Gameplay;
using UnityEngine;

namespace MindArrow.Board
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class BoardView : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField, Min(2)]
        private int columns = 10;

        [SerializeField, Min(2)]
        private int rows = 16;

        [Header("Arrow Visual")]
        [SerializeField, Min(4f)]
        private float pathThickness = 16f;

        [SerializeField, Min(8f)]
        private float headLength = 44f;

        [SerializeField, Min(8f)]
        private float headWidth = 38f;

        [Header("Snake Escape")]
        [SerializeField, Min(0f)]
        private float outsidePadding = 100f;

        private RectTransform boardRect;

        private readonly Dictionary<int, GameObject> arrowRoots = new();

        private void Awake()
        {
            boardRect = GetComponent<RectTransform>();
        }

        public void ConfigureGrid(int newColumns, int newRows)
        {
            columns = Mathf.Max(2, newColumns);
            rows = Mathf.Max(2, newRows);
        }

        public void RenderArrow(
            ArrowData arrow,
            Action<int> clickedCallback)
        {
            if (arrow == null ||
                arrow.Path == null ||
                arrow.Path.Count < 2)
            {
                Debug.LogError("Invalid ArrowData.", this);
                return;
            }

            GameObject arrowRoot = new(
                $"Arrow_{arrow.Id}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(ArrowPathGraphic),
                typeof(ArrowView));

            arrowRoot.transform.SetParent(transform, false);

            RectTransform rootRect = arrowRoot.GetComponent<RectTransform>();

            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;

            List<Vector2> localPoints = new();

            for (int i = 0; i < arrow.Path.Count; i++)
            {
                localPoints.Add(GridToLocal(arrow.Path[i]));
            }

            ArrowPathGraphic graphic = arrowRoot.GetComponent<ArrowPathGraphic>();

            graphic.SetPath(
                localPoints,
                arrow.Color,
                pathThickness,
                headLength,
                headWidth);

            ArrowView arrowView = arrowRoot.GetComponent<ArrowView>();

            arrowView.Initialize(
                arrow.Id,
                clickedCallback,
                graphic,
                localPoints);

            arrowRoots[arrow.Id] = arrowRoot;
        }

        public void PlayBlockedFeedback(int arrowId)
        {
            if (!arrowRoots.TryGetValue(arrowId, out GameObject arrowRoot))
            {
                return;
            }

            ArrowView arrowView = arrowRoot.GetComponent<ArrowView>();
            arrowView.PlayBlockedFeedback();
        }

        public void AnimateEscape(
            int arrowId,
            ArrowDirection direction,
            Action<int> completedCallback)
        {
            if (!arrowRoots.TryGetValue(arrowId, out GameObject arrowRoot))
            {
                return;
            }

            ArrowView arrowView = arrowRoot.GetComponent<ArrowView>();

            Vector2 exitDirection = GetDirectionVector(direction);

            float headExitDistance = CalculateHeadExitDistance(
                arrowView.HeadLocalPosition,
                direction);

            arrowView.PlaySnakeEscape(
                exitDirection,
                headExitDistance,
                completedArrowId =>
                {
                    arrowRoots.Remove(completedArrowId);

                    if (arrowRoot != null)
                    {
                        Destroy(arrowRoot);
                    }

                    completedCallback?.Invoke(completedArrowId);
                });
        }

        public void Clear()
        {
            foreach (GameObject arrowRoot in arrowRoots.Values)
            {
                if (arrowRoot == null)
                {
                    continue;
                }

                // Prevent one-frame overlap when reloading the same level.
                arrowRoot.SetActive(false);
                Destroy(arrowRoot);
            }

            arrowRoots.Clear();
        }

        private float CalculateHeadExitDistance(
            Vector2 headPosition,
            ArrowDirection direction)
        {
            Rect rect = boardRect.rect;

            return direction switch
            {
                ArrowDirection.Up =>
                    Mathf.Max(0f, rect.yMax - headPosition.y) + outsidePadding,

                ArrowDirection.Right =>
                    Mathf.Max(0f, rect.xMax - headPosition.x) + outsidePadding,

                ArrowDirection.Down =>
                    Mathf.Max(0f, headPosition.y - rect.yMin) + outsidePadding,

                ArrowDirection.Left =>
                    Mathf.Max(0f, headPosition.x - rect.xMin) + outsidePadding,

                _ => outsidePadding
            };
        }

        private static Vector2 GetDirectionVector(ArrowDirection direction)
        {
            return direction switch
            {
                ArrowDirection.Up => Vector2.up,
                ArrowDirection.Right => Vector2.right,
                ArrowDirection.Down => Vector2.down,
                ArrowDirection.Left => Vector2.left,
                _ => Vector2.right
            };
        }

        private Vector2 GridToLocal(GridPosition position)
        {
            float cellSize = Mathf.Min(
                boardRect.rect.width / columns,
                boardRect.rect.height / rows);

            float boardWidth = columns * cellSize;
            float boardHeight = rows * cellSize;

            float originX = -boardWidth * 0.5f + cellSize * 0.5f;
            float originY = -boardHeight * 0.5f + cellSize * 0.5f;

            return new Vector2(
                originX + position.X * cellSize,
                originY + position.Y * cellSize);
        }
    }
}
