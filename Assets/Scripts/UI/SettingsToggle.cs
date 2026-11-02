using Data;
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
        [SerializeField] private string PreferenceKey;
        [SerializeField] private bool DefaultValue;
        [SerializeField] private float AnimationDuration;

        private CachedInt _setting;
        private Tween _slideTween;
        private float _halfWidth;

        public bool IsOn => _setting.Value != 0;

        private void OnDisable()
        {
            _slideTween?.Kill();
            _slideTween = null;
        }

        private void OnEnable()
        {
            _setting = CachedInt.Get(PreferenceKey, DefaultValue ? 1 : 0);
            _halfWidth = (Indicator.anchorMax.x - Indicator.anchorMin.x) * 0.5f;
            IndicatorImage.sprite = IsOn ? OnSprite : OffSprite;
            SetPosition(IsOn ? OnPosition : OffPosition);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _setting.Value = IsOn ? 0 : 1;
            IndicatorImage.sprite = IsOn ? OnSprite : OffSprite;
            _slideTween?.Kill();
            _slideTween = DOTween.To(
                () => (Indicator.anchorMin.x + Indicator.anchorMax.x) * 0.5f,
                SetPosition,
                IsOn ? OnPosition : OffPosition,
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
