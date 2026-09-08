using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MindArrow.Gameplay
{
    public sealed class ArrowView
        : MonoBehaviour,
          IPointerClickHandler
    {
        private int arrowId;

        private Action<int> clickedCallback;

        public int ArrowId => arrowId;

        public void Initialize(
            int id,
            Action<int> callback)
        {
            arrowId = id;
            clickedCallback = callback;
        }

        public void OnPointerClick(
            PointerEventData eventData)
        {
            clickedCallback?.Invoke(arrowId);
        }
    }
}