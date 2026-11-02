using DG.Tweening;
using UnityEngine;
namespace UI.Windows
{
    public class WindowView : MonoBehaviour
    {
        [SerializeField] private Transform Window;
        [SerializeField] private CanvasGroup Group;
        [SerializeField] private float OpenDuration;

        private Sequence _openSequence;
        private Vector3 _windowScale;

        protected virtual void Awake()
        {
            _windowScale = Window.localScale;
        }

        protected virtual void OnEnable()
        {
            transform.SetAsLastSibling();
            Group.alpha = 0f;
            Group.interactable = false;
            Group.blocksRaycasts = true;
            Window.localScale = Vector3.zero;
            _openSequence = DOTween.Sequence();
            _openSequence.Append(DOTween.To(() => Group.alpha, value => Group.alpha = value, 1f, OpenDuration));
            _openSequence.Join(Window.DOScale(_windowScale, OpenDuration).SetEase(Ease.OutBack));
            _openSequence.SetUpdate(true);
            _openSequence.OnComplete(() => Group.interactable = true);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        protected void DisableInteraction()
        {
            Group.interactable = false;
        }

        protected virtual void OnDisable()
        {
            _openSequence?.Kill();
            _openSequence = null;
            Window.localScale = _windowScale;
        }
    }
}
