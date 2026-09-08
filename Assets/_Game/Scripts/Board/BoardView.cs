using System;
using System.Collections.Generic;
using MindArrow.Gameplay;
using UnityEngine;
using UnityEngine.UI;

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

        [Header("Prototype Rendering")]
        [SerializeField, Min(2f)]
        private float pathThickness = 18f;

        [SerializeField, Min(4f)]
        private float headSize = 38f;

        private RectTransform boardRect;

        private readonly Dictionary<int, GameObject>
            arrowRoots = new();

        private void Awake()
        {
            boardRect =
                GetComponent<RectTransform>();
        }

        public void RenderArrow(
            ArrowData arrow,
            Action<int> clickedCallback)
        {
            if (arrow == null ||
                arrow.Path == null ||
                arrow.Path.Count < 2)
            {
                Debug.LogError(
                    "Invalid ArrowData.",
                    this
                );

                return;
            }

            GameObject arrowRoot =
                new(
                    $"Arrow_{arrow.Id}",
                    typeof(RectTransform),
                    typeof(ArrowView)
                );

            arrowRoot.transform.SetParent(
                transform,
                false
            );

            ArrowView arrowView =
                arrowRoot.GetComponent<ArrowView>();

            arrowView.Initialize(
                arrow.Id,
                clickedCallback
            );

            arrowRoots[arrow.Id] =
                arrowRoot;

            for (int i = 0;
                 i < arrow.Path.Count - 1;
                 i++)
            {
                CreateSegment(
                    arrowRoot.transform,
                    arrow.Path[i],
                    arrow.Path[i + 1],
                    arrow.Color,
                    i
                );
            }

            CreateHead(
                arrowRoot.transform,
                arrow.Path[^1],
                arrow.Color
            );
        }

        public void RemoveArrow(int arrowId)
        {
            if (!arrowRoots.TryGetValue(
                    arrowId,
                    out GameObject arrowRoot))
            {
                return;
            }

            arrowRoots.Remove(arrowId);

            Destroy(arrowRoot);
        }

        public void Clear()
        {
            foreach (GameObject arrowRoot
                     in arrowRoots.Values)
            {
                if (arrowRoot != null)
                {
                    Destroy(arrowRoot);
                }
            }

            arrowRoots.Clear();
        }

        private void CreateSegment(
            Transform parent,
            GridPosition start,
            GridPosition end,
            Color color,
            int segmentIndex)
        {
            bool horizontal =
                start.Y == end.Y;

            bool vertical =
                start.X == end.X;

            if (!horizontal &&
                !vertical)
            {
                return;
            }

            Vector2 startPosition =
                GridToLocal(start);

            Vector2 endPosition =
                GridToLocal(end);

            Vector2 midpoint =
                (startPosition + endPosition)
                * 0.5f;

            float distance =
                Vector2.Distance(
                    startPosition,
                    endPosition
                );

            Image image =
                CreateImage(
                    parent,
                    $"Segment_{segmentIndex}",
                    color
                );

            RectTransform rect =
                image.rectTransform;

            rect.anchoredPosition =
                midpoint;

            rect.sizeDelta =
                horizontal
                    ? new Vector2(
                        distance + pathThickness,
                        pathThickness
                    )
                    : new Vector2(
                        pathThickness,
                        distance + pathThickness
                    );
        }

        private void CreateHead(
            Transform parent,
            GridPosition position,
            Color color)
        {
            Image image =
                CreateImage(
                    parent,
                    "Head",
                    color
                );

            RectTransform rect =
                image.rectTransform;

            rect.anchoredPosition =
                GridToLocal(position);

            rect.sizeDelta =
                new Vector2(
                    headSize,
                    headSize
                );
        }

        private static Image CreateImage(
            Transform parent,
            string objectName,
            Color color)
        {
            GameObject gameObject =
                new(
                    objectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
                );

            gameObject.transform.SetParent(
                parent,
                false
            );

            Image image =
                gameObject.GetComponent<Image>();

            image.color = color;

            // IMPORTANT:
            // Leave raycast enabled for clicking.
            image.raycastTarget = true;

            return image;
        }

        private Vector2 GridToLocal(
            GridPosition position)
        {
            float cellSize =
                Mathf.Min(
                    boardRect.rect.width
                    / columns,
                    boardRect.rect.height
                    / rows
                );

            float width =
                columns * cellSize;

            float height =
                rows * cellSize;

            float originX =
                -width * 0.5f +
                cellSize * 0.5f;

            float originY =
                -height * 0.5f +
                cellSize * 0.5f;

            return new Vector2(
                originX +
                position.X * cellSize,

                originY +
                position.Y * cellSize
            );
        }
    }
}