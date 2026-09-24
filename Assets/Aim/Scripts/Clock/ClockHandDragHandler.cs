using UnityEngine;
using UnityEngine.EventSystems;

namespace Aim.Clock
{
    public sealed class ClockHandDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public enum HandType
        {
            Hour,
            Minute
        }

        [SerializeField]
        private ClockView _clockView;

        [SerializeField]
        private HandType _handType;

        [SerializeField]
        private RectTransform _handTransform;

        [SerializeField]
        private RectTransform _dragArea;

        private bool _isDragging;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_clockView == null || !_clockView.IsEditInteractionEnabled)
            {
                return;
            }

            _isDragging = true;
            ApplyDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _clockView == null || !_clockView.IsEditInteractionEnabled)
            {
                return;
            }

            ApplyDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
        }

        private void ApplyDrag(PointerEventData eventData)
        {
            if (_clockView == null || _handTransform == null)
            {
                return;
            }

            var area = _dragArea != null ? _dragArea : _handTransform.parent as RectTransform;
            if (area == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    area,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var localPoint))
            {
                return;
            }

            var angle = Mathf.Atan2(localPoint.x, localPoint.y) * Mathf.Rad2Deg;
            _handTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);

            if (_handType == HandType.Hour)
            {
                _clockView.NotifyHourHandDragged(angle);
            }
            else
            {
                _clockView.NotifyMinuteHandDragged(angle);
            }
        }
    }
}
