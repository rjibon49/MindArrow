using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MindArrow.Board
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ArrowPathGraphic :
        MaskableGraphic,
        ICanvasRaycastFilter
    {
        [SerializeField, Min(4f)]
        private float pathThickness = 18f;

        [SerializeField, Min(8f)]
        private float headLength = 46f;

        [SerializeField, Min(8f)]
        private float headWidth = 42f;

        [SerializeField, Range(6, 20)]
        private int roundSegments = 12;

        [SerializeField, Min(0f)]
        private float hitPadding = 16f;

        private readonly List<Vector2> points =
            new();

        public void SetPath(
            IReadOnlyList<Vector2> sourcePoints,
            Color pathColor,
            float thickness,
            float arrowHeadLength,
            float arrowHeadWidth)
        {
            points.Clear();

            if (sourcePoints != null)
            {
                for (int i = 0;
                     i < sourcePoints.Count;
                     i++)
                {
                    points.Add(
                        sourcePoints[i]
                    );
                }
            }

            color = pathColor;

            pathThickness =
                thickness;

            headLength =
                arrowHeadLength;

            headWidth =
                arrowHeadWidth;

            raycastTarget = true;

            SetVerticesDirty();
            SetMaterialDirty();
        }

        public void UpdatePath(
            IReadOnlyList<Vector2> sourcePoints)
                {
                    points.Clear();

                    if (sourcePoints != null)
                    {
                        for (int i = 0;
                             i < sourcePoints.Count;
                             i++)
                        {
                            points.Add(
                                sourcePoints[i]
                            );
                        }
                    }

                    SetVerticesDirty();
                }

        protected override void OnPopulateMesh(
            VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            if (points.Count < 2)
            {
                return;
            }

            GetHeadGeometry(
                out Vector2 headDirection,
                out Vector2 tip,
                out Vector2 baseCenter,
                out Vector2 baseLeft,
                out Vector2 baseRight
            );

            int lastSegmentIndex =
                points.Count - 2;

            // Draw every straight segment.
            for (int i = 0;
                 i < points.Count - 1;
                 i++)
            {
                Vector2 start =
                    points[i];

                Vector2 end =
                    points[i + 1];

                // Final line stops slightly inside
                // the arrowhead so they visually join.
                if (i == lastSegmentIndex)
                {
                    end =
                        baseCenter +
                        headDirection *
                        (pathThickness * 0.25f);
                }

                AddSegment(
                    vertexHelper,
                    start,
                    end
                );
            }

            // Rounded starting cap.
            AddCircle(
                vertexHelper,
                points[0],
                pathThickness * 0.5f
            );

            // Rounded 90-degree bends.
            for (int i = 1;
                 i < points.Count - 1;
                 i++)
            {
                AddCircle(
                    vertexHelper,
                    points[i],
                    pathThickness * 0.5f
                );
            }

            // Integrated triangular head.
            AddTriangle(
                vertexHelper,
                tip,
                baseLeft,
                baseRight
            );
        }

        private void AddSegment(
            VertexHelper vertexHelper,
            Vector2 start,
            Vector2 end)
        {
            Vector2 delta =
                end - start;

            float length =
                delta.magnitude;

            if (length <= 0.001f)
            {
                return;
            }

            Vector2 direction =
                delta / length;

            Vector2 perpendicular =
                new Vector2(
                    -direction.y,
                    direction.x
                );

            Vector2 halfOffset =
                perpendicular *
                (pathThickness * 0.5f);

            Vector2 a =
                start + halfOffset;

            Vector2 b =
                start - halfOffset;

            Vector2 c =
                end - halfOffset;

            Vector2 d =
                end + halfOffset;

            int index =
                vertexHelper.currentVertCount;

            AddVertex(vertexHelper, a);
            AddVertex(vertexHelper, b);
            AddVertex(vertexHelper, c);
            AddVertex(vertexHelper, d);

            vertexHelper.AddTriangle(
                index,
                index + 1,
                index + 2
            );

            vertexHelper.AddTriangle(
                index,
                index + 2,
                index + 3
            );
        }

        private void AddCircle(
            VertexHelper vertexHelper,
            Vector2 center,
            float radius)
        {
            int centerIndex =
                vertexHelper.currentVertCount;

            AddVertex(
                vertexHelper,
                center
            );

            for (int i = 0;
                 i <= roundSegments;
                 i++)
            {
                float angle =
                    i /
                    (float)roundSegments *
                    Mathf.PI *
                    2f;

                Vector2 point =
                    center +
                    new Vector2(
                        Mathf.Cos(angle),
                        Mathf.Sin(angle)
                    ) *
                    radius;

                AddVertex(
                    vertexHelper,
                    point
                );
            }

            for (int i = 0;
                 i < roundSegments;
                 i++)
            {
                vertexHelper.AddTriangle(
                    centerIndex,
                    centerIndex + i + 1,
                    centerIndex + i + 2
                );
            }
        }

        private void AddTriangle(
            VertexHelper vertexHelper,
            Vector2 tip,
            Vector2 baseLeft,
            Vector2 baseRight)
        {
            int index =
                vertexHelper.currentVertCount;

            AddVertex(
                vertexHelper,
                tip
            );

            AddVertex(
                vertexHelper,
                baseLeft
            );

            AddVertex(
                vertexHelper,
                baseRight
            );

            vertexHelper.AddTriangle(
                index,
                index + 1,
                index + 2
            );
        }

        private void AddVertex(
            VertexHelper vertexHelper,
            Vector2 position)
        {
            UIVertex vertex =
                UIVertex.simpleVert;

            vertex.position =
                position;

            vertex.color =
                color;

            vertex.uv0 =
                Vector2.zero;

            vertexHelper.AddVert(
                vertex
            );
        }

        private void GetHeadGeometry(
            out Vector2 direction,
            out Vector2 tip,
            out Vector2 baseCenter,
            out Vector2 baseLeft,
            out Vector2 baseRight)
        {
            tip =
                points[^1];

            Vector2 previous =
                points[^2];

            Vector2 delta =
                tip - previous;

            float finalSegmentLength =
                delta.magnitude;

            direction =
                finalSegmentLength > 0.001f
                    ? delta.normalized
                    : Vector2.right;

            float actualHeadLength =
                Mathf.Min(
                    headLength,
                    finalSegmentLength * 0.75f
                );

            actualHeadLength =
                Mathf.Max(
                    actualHeadLength,
                    pathThickness * 1.25f
                );

            actualHeadLength =
                Mathf.Min(
                    actualHeadLength,
                    finalSegmentLength * 0.9f
                );

            float actualHalfWidth =
                Mathf.Min(
                    headWidth * 0.5f,
                    actualHeadLength * 0.70f
                );

            baseCenter =
                tip -
                direction *
                actualHeadLength;

            Vector2 perpendicular =
                new Vector2(
                    -direction.y,
                    direction.x
                );

            baseLeft =
                baseCenter +
                perpendicular *
                actualHalfWidth;

            baseRight =
                baseCenter -
                perpendicular *
                actualHalfWidth;
        }

        public bool IsRaycastLocationValid(
            Vector2 screenPoint,
            Camera eventCamera)
        {
            if (points.Count < 2)
            {
                return false;
            }

            if (!RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    screenPoint,
                    eventCamera,
                    out Vector2 localPoint))
            {
                return false;
            }

            float hitRadius =
                pathThickness * 0.5f +
                hitPadding;

            // Test against path segments.
            for (int i = 0;
                 i < points.Count - 1;
                 i++)
            {
                float distance =
                    DistanceToSegment(
                        localPoint,
                        points[i],
                        points[i + 1]
                    );

                if (distance <= hitRadius)
                {
                    return true;
                }
            }

            GetHeadGeometry(
                out _,
                out Vector2 tip,
                out _,
                out Vector2 baseLeft,
                out Vector2 baseRight
            );

            return PointInsideTriangle(
                localPoint,
                tip,
                baseLeft,
                baseRight
            );
        }

        private static float DistanceToSegment(
            Vector2 point,
            Vector2 start,
            Vector2 end)
        {
            Vector2 segment =
                end - start;

            float squaredLength =
                segment.sqrMagnitude;

            if (squaredLength <= 0.0001f)
            {
                return Vector2.Distance(
                    point,
                    start
                );
            }

            float t =
                Vector2.Dot(
                    point - start,
                    segment
                ) /
                squaredLength;

            t =
                Mathf.Clamp01(t);

            Vector2 projection =
                start +
                segment * t;

            return Vector2.Distance(
                point,
                projection
            );
        }

        private static bool PointInsideTriangle(
            Vector2 point,
            Vector2 a,
            Vector2 b,
            Vector2 c)
        {
            float d1 =
                Sign(
                    point,
                    a,
                    b
                );

            float d2 =
                Sign(
                    point,
                    b,
                    c
                );

            float d3 =
                Sign(
                    point,
                    c,
                    a
                );

            bool hasNegative =
                d1 < 0f ||
                d2 < 0f ||
                d3 < 0f;

            bool hasPositive =
                d1 > 0f ||
                d2 > 0f ||
                d3 > 0f;

            return !(
                hasNegative &&
                hasPositive
            );
        }

        private static float Sign(
            Vector2 p1,
            Vector2 p2,
            Vector2 p3)
        {
            return
                (p1.x - p3.x) *
                (p2.y - p3.y) -
                (p2.x - p3.x) *
                (p1.y - p3.y);
        }
    }
}