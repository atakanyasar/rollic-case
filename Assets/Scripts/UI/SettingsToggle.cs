using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class SettingsToggle : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private RectTransform Indicator;
        [SerializeField] private Image IndicatorImage;
        [SerializeField] private Sprite OnSprite;
        [SerializeField] private Sprite OffSprite;
        [SerializeField] private float OnPosition;
        [SerializeField] private float OffPosition;
        [SerializeField] private bool Value;
        [SerializeField] private float AnimationDuration;

        private Tween _slideTween;
        private float _halfWidth;

        public bool IsOn => Value;

        private void OnDisable()
        {
            _slideTween?.Kill();
            _slideTween = null;
        }

        private void OnEnable()
        {
            _halfWidth = (Indicator.anchorMax.x - Indicator.anchorMin.x) * 0.5f;
            IndicatorImage.sprite = Value ? OnSprite : OffSprite;
            SetPosition(Value ? OnPosition : OffPosition);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            Value = !Value;
            IndicatorImage.sprite = Value ? OnSprite : OffSprite;
            _slideTween?.Kill();
            _slideTween = DOTween.To(
                () => (Indicator.anchorMin.x + Indicator.anchorMax.x) * 0.5f,
                SetPosition,
                Value ? OnPosition : OffPosition,
                AnimationDuration).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        private void SetPosition(float position)
        {
            Indicator.anchorMin = new Vector2(position - _halfWidth, Indicator.anchorMin.y);
            Indicator.anchorMax = new Vector2(position + _halfWidth, Indicator.anchorMax.y);
            var offset = Indicator.anchoredPosition;
            offset.x = 0f;
            Indicator.anchoredPosition = offset;
        }
    }
}
