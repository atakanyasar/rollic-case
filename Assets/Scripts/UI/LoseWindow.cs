using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class LoseWindow : MonoBehaviour
    {
        private const string MainScene = "MainScene";
        private const string GameplayScene = "GameplayScene";

        [SerializeField] private Transform Window;
        [SerializeField] private CanvasGroup Group;
        [SerializeField] private float OpenDuration;

        private Sequence _openSequence;
        private Vector3 _windowScale;

        private void Awake()
        {
            _windowScale = Window.localScale;
        }

        private void OnEnable()
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

        public void Retry()
        {
            LoadScene(GameplayScene);
        }

        public void Close()
        {
            LoadScene(MainScene);
        }

        private void LoadScene(string sceneName)
        {
            Group.interactable = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }

        private void OnDisable()
        {
            _openSequence?.Kill();
            _openSequence = null;
            Window.localScale = _windowScale;
        }
    }
}
