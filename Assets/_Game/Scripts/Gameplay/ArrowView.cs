using System;
using System.Collections;
using System.Collections.Generic;
using MindArrow.Board;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MindArrow.Gameplay
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class ArrowView :
        MonoBehaviour,
        IPointerClickHandler
    {
        [Header("Snake Escape")]
        [SerializeField, Min(100f)]
        private float escapeSpeed = 1700f;

        [SerializeField, Min(0.05f)]
        private float minimumEscapeDuration = 0.28f;

        [SerializeField, Min(0.1f)]
        private float maximumEscapeDuration = 0.85f;

        [Header("Blocked Feedback")]
        [SerializeField, Min(0.05f)]
        private float blockedDuration = 0.16f;

        [SerializeField, Min(1f)]
        private float blockedShakeDistance = 14f;

        [SerializeField, Min(1f)]
        private float blockedShakeCycles = 3f;

        private int arrowId;

        private Action<int> clickedCallback;

        private RectTransform rectTransform;

        private ArrowPathGraphic pathGraphic;

        private Coroutine activeRoutine;

        private readonly List<Vector2>
            originalPath = new();

        private readonly List<Vector2>
            visiblePath = new();

        public int ArrowId => arrowId;

        public ArrowState State { get; private set; }
            = ArrowState.Idle;

        public Vector2 HeadLocalPosition
        {
            get
            {
                if (originalPath.Count == 0)
                {
                    return Vector2.zero;
                }

                return originalPath[^1];
            }
        }

        private void Awake()
        {
            rectTransform =
                GetComponent<RectTransform>();
        }

        public void Initialize(
            int id,
            Action<int> callback,
            ArrowPathGraphic graphic,
            IReadOnlyList<Vector2> sourcePath)
        {
            arrowId = id;

            clickedCallback =
                callback;

            pathGraphic =
                graphic;

            originalPath.Clear();

            if (sourcePath != null)
            {
                for (int i = 0;
                     i < sourcePath.Count;
                     i++)
                {
                    originalPath.Add(
                        sourcePath[i]
                    );
                }
            }

            State =
                ArrowState.Idle;
        }

        public void OnPointerClick(
            PointerEventData eventData)
        {
            if (State != ArrowState.Idle)
            {
                return;
            }

            clickedCallback?.Invoke(
                arrowId
            );
        }

        public void PlayBlockedFeedback()
        {
            if (State != ArrowState.Idle)
            {
                return;
            }

            StartManagedCoroutine(
                BlockedRoutine()
            );
        }

        public void PlaySnakeEscape(
            Vector2 exitDirection,
            float headExitDistance,
            Action<int> completedCallback)
        {
            if (State != ArrowState.Idle)
            {
                return;
            }

            if (pathGraphic == null ||
                originalPath.Count < 2)
            {
                Debug.LogError(
                    $"Arrow {arrowId} has no valid visual path.",
                    this
                );

                return;
            }

            StartManagedCoroutine(
                SnakeEscapeRoutine(
                    exitDirection,
                    headExitDistance,
                    completedCallback
                )
            );
        }

        private void StartManagedCoroutine(
            IEnumerator routine)
        {
            if (activeRoutine != null)
            {
                StopCoroutine(
                    activeRoutine
                );
            }

            activeRoutine =
                StartCoroutine(
                    routine
                );
        }

        private IEnumerator BlockedRoutine()
        {
            State =
                ArrowState.BlockedFeedback;

            Vector2 startPosition =
                rectTransform.anchoredPosition;

            Vector3 startScale =
                rectTransform.localScale;

            float elapsed = 0f;

            while (elapsed < blockedDuration)
            {
                elapsed +=
                    Time.deltaTime;

                float progress =
                    Mathf.Clamp01(
                        elapsed /
                        blockedDuration
                    );

                float shake =
                    Mathf.Sin(
                        progress *
                        Mathf.PI *
                        2f *
                        blockedShakeCycles
                    );

                float fade =
                    1f - progress;

                rectTransform.anchoredPosition =
                    startPosition +
                    new Vector2(
                        shake *
                        blockedShakeDistance *
                        fade,
                        0f
                    );

                float pulse =
                    1f +
                    Mathf.Sin(
                        progress *
                        Mathf.PI
                    ) *
                    0.035f;

                rectTransform.localScale =
                    startScale *
                    pulse;

                yield return null;
            }

            rectTransform.anchoredPosition =
                startPosition;

            rectTransform.localScale =
                startScale;

            State =
                ArrowState.Idle;

            activeRoutine =
                null;
        }

        private IEnumerator SnakeEscapeRoutine(
            Vector2 exitDirection,
            float headExitDistance,
            Action<int> completedCallback)
        {
            State =
                ArrowState.Escaping;

            exitDirection =
                exitDirection.normalized;

            float arrowLength =
                CalculatePolylineLength(
                    originalPath
                );

            if (arrowLength <= 0.001f)
            {
                State =
                    ArrowState.Removed;

                completedCallback?.Invoke(
                    arrowId
                );

                yield break;
            }

            /*
             * Route:
             *
             * tail ---- bends ---- head
             *                        |
             *                        | exit extension
             *                        v
             *
             * The visible arrow is a moving
             * "window" over this route.
             */

            List<Vector2> route =
                new(originalPath);

            Vector2 originalHead =
                originalPath[^1];

            /*
             * We need enough extension for:
             *
             * 1. head to leave the board
             * 2. full original arrow length
             *    to follow it outside
             */
            float extensionLength =
                headExitDistance +
                arrowLength +
                100f;

            route.Add(
                originalHead +
                exitDirection *
                extensionLength
            );

            float[] cumulative =
                BuildCumulativeLengths(
                    route
                );

            /*
             * Tail must travel:
             *
             * original arrow length
             * +
             * head-to-outside distance
             */
            float travelDistance =
                arrowLength +
                headExitDistance;

            float duration =
                Mathf.Clamp(
                    travelDistance /
                    escapeSpeed,
                    minimumEscapeDuration,
                    maximumEscapeDuration
                );

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed +=
                    Time.deltaTime;

                float normalized =
                    Mathf.Clamp01(
                        elapsed /
                        duration
                    );

                /*
                 * Smooth acceleration/deceleration.
                 * Not a rigid position tween.
                 */
                float eased =
                    SmoothStep01(
                        normalized
                    );

                float tailDistance =
                    eased *
                    travelDistance;

                float headDistance =
                    tailDistance +
                    arrowLength;

                BuildVisiblePath(
                    route,
                    cumulative,
                    tailDistance,
                    headDistance,
                    visiblePath
                );

                pathGraphic.UpdatePath(
                    visiblePath
                );

                yield return null;
            }

            BuildVisiblePath(
                route,
                cumulative,
                travelDistance,
                travelDistance +
                arrowLength,
                visiblePath
            );

            pathGraphic.UpdatePath(
                visiblePath
            );

            State =
                ArrowState.Removed;

            activeRoutine =
                null;

            completedCallback?.Invoke(
                arrowId
            );
        }

        private static float
            CalculatePolylineLength(
                IReadOnlyList<Vector2> path)
        {
            float total = 0f;

            for (int i = 0;
                 i < path.Count - 1;
                 i++)
            {
                total +=
                    Vector2.Distance(
                        path[i],
                        path[i + 1]
                    );
            }

            return total;
        }

        private static float[]
            BuildCumulativeLengths(
                IReadOnlyList<Vector2> path)
        {
            float[] cumulative =
                new float[
                    path.Count
                ];

            cumulative[0] = 0f;

            for (int i = 1;
                 i < path.Count;
                 i++)
            {
                cumulative[i] =
                    cumulative[i - 1] +
                    Vector2.Distance(
                        path[i - 1],
                        path[i]
                    );
            }

            return cumulative;
        }

        private static void BuildVisiblePath(
            IReadOnlyList<Vector2> route,
            IReadOnlyList<float> cumulative,
            float fromDistance,
            float toDistance,
            List<Vector2> result)
        {
            result.Clear();

            float totalLength =
                cumulative[
                    cumulative.Count - 1
                ];

            fromDistance =
                Mathf.Clamp(
                    fromDistance,
                    0f,
                    totalLength
                );

            toDistance =
                Mathf.Clamp(
                    toDistance,
                    fromDistance,
                    totalLength
                );

            Vector2 startPoint =
                PointAtDistance(
                    route,
                    cumulative,
                    fromDistance
                );

            AddDistinct(
                result,
                startPoint
            );

            for (int i = 1;
                 i < route.Count - 1;
                 i++)
            {
                float vertexDistance =
                    cumulative[i];

                if (vertexDistance >
                        fromDistance + 0.01f &&
                    vertexDistance <
                        toDistance - 0.01f)
                {
                    AddDistinct(
                        result,
                        route[i]
                    );
                }
            }

            Vector2 endPoint =
                PointAtDistance(
                    route,
                    cumulative,
                    toDistance
                );

            AddDistinct(
                result,
                endPoint
            );
        }

        private static Vector2 PointAtDistance(
            IReadOnlyList<Vector2> route,
            IReadOnlyList<float> cumulative,
            float distance)
        {
            if (distance <= 0f)
            {
                return route[0];
            }

            float totalLength =
                cumulative[
                    cumulative.Count - 1
                ];

            if (distance >= totalLength)
            {
                return route[^1];
            }

            for (int i = 0;
                 i < route.Count - 1;
                 i++)
            {
                float segmentStart =
                    cumulative[i];

                float segmentEnd =
                    cumulative[i + 1];

                if (distance >
                    segmentEnd)
                {
                    continue;
                }

                float segmentLength =
                    segmentEnd -
                    segmentStart;

                if (segmentLength <=
                    0.0001f)
                {
                    return route[i + 1];
                }

                float t =
                    (
                        distance -
                        segmentStart
                    ) /
                    segmentLength;

                return Vector2.Lerp(
                    route[i],
                    route[i + 1],
                    t
                );
            }

            return route[^1];
        }

        private static void AddDistinct(
            List<Vector2> result,
            Vector2 point)
        {
            if (result.Count == 0)
            {
                result.Add(point);
                return;
            }

            if (Vector2.Distance(
                    result[^1],
                    point) >
                0.01f)
            {
                result.Add(point);
            }
        }

        private static float SmoothStep01(
            float value)
        {
            value =
                Mathf.Clamp01(value);

            return
                value *
                value *
                (
                    3f -
                    2f *
                    value
                );
        }
    }
}