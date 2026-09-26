using UnityEngine;

using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace Watch.Common
{
    public class PanelAnimationView : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Vector2 _startPosition;
        [SerializeField] private Vector2 _finishPosition;
        [SerializeField] private float _durationMoving;
        [Header("References")]
        [SerializeField] private RectTransform _rectTransform;

        public TweenerCore<Vector2, Vector2, VectorOptions> Show()
            => _rectTransform.DOAnchorPos(_finishPosition, _durationMoving);

        public TweenerCore<Vector2, Vector2, VectorOptions> Hide()
            => _rectTransform.DOAnchorPos(_startPosition, _durationMoving);

        private void Start()
        {
            _rectTransform.anchoredPosition = _startPosition;
        }

        private void OnValidate()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
        }
    }
}
