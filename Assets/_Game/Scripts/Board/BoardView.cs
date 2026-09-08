using System.Collections.Generic;
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
        private float pathThickness = 14f;

        [SerializeField, Min(4f)]
        private float headSize = 30f;

        private RectTransform boardRect;

        private readonly List<GameObject> renderedObjects = new();

        private void Awake()
        {
            boardRect = GetComponent<RectTransform>();
        }

        public void RenderArrow(ArrowData arrow)
        {
            if (arrow == null)
            {
                Debug.LogError("ArrowData is null.", this);
                return;
            }

            if (arrow.Path == null || arrow.Path.Count < 2)
            {
                Debug.LogError(
                    "Arrow path needs at least two grid positions.",
                    this
                );

                return;
            }

            for (int i = 0; i < arrow.Path.Count - 1; i++)
            {
                CreateSegment(
                    arrow.Path[i],
                    arrow.Path[i + 1],
                    arrow.Color,
                    arrow.Id,
                    i
                );
            }

            CreateHead(
                arrow.Path[^1],
                arrow.Color,
                arrow.Id
            );
        }

        public void ClearRenderedArrows()
        {
            foreach (GameObject renderedObject in renderedObjects)
            {
                if (renderedObject != null)
                {
                    Destroy(renderedObject);
                }
            }

            renderedObjects.Clear();
        }

        private void CreateSegment(
            GridPosition start,
            GridPosition end,
            Color color,
            int arrowId,
            int segmentIndex)
        {
            bool horizontal = start.Y == end.Y;
            bool vertical = start.X == end.X;

            if (!horizontal && !vertical)
            {
                Debug.LogError(
                    $"Arrow {arrowId} contains a diagonal segment.",
                    this
                );

                return;
            }

            Vector2 startPosition = GridToLocal(start);
            Vector2 endPosition = GridToLocal(end);

            Vector2 midpoint =
                (startPosition + endPosition) * 0.5f;

            float distance =
                Vector2.Distance(startPosition, endPosition);

            Image image = CreateImage(
                $"Arrow_{arrowId}_Segment_{segmentIndex}",
                color
            );

            RectTransform rect =
                image.rectTransform;

            rect.anchoredPosition = midpoint;

            if (horizontal)
            {
                rect.sizeDelta =
                    new Vector2(
                        distance + pathThickness,
                        pathThickness
                    );
            }
            else
            {
                rect.sizeDelta =
                    new Vector2(
                        pathThickness,
                        distance + pathThickness
                    );
            }
        }

        private void CreateHead(
            GridPosition position,
            Color color,
            int arrowId)
        {
            Image image = CreateImage(
                $"Arrow_{arrowId}_Head",
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

        private Image CreateImage(
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
                transform,
                false
            );

            Image image =
                gameObject.GetComponent<Image>();

            image.color = color;
            image.raycastTarget = false;

            renderedObjects.Add(gameObject);

            return image;
        }

        private Vector2 GridToLocal(
            GridPosition position)
        {
            float cellSize =
                Mathf.Min(
                    boardRect.rect.width / columns,
                    boardRect.rect.height / rows
                );

            float boardWidth =
                columns * cellSize;

            float boardHeight =
                rows * cellSize;

            float originX =
                -boardWidth * 0.5f
                + cellSize * 0.5f;

            float originY =
                -boardHeight * 0.5f
                + cellSize * 0.5f;

            return new Vector2(
                originX + position.X * cellSize,
                originY + position.Y * cellSize
            );
        }
    }
}