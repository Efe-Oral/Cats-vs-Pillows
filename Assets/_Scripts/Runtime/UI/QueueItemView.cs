using System;
using System.Collections;
using CatsVsPillows.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CatsVsPillows.UI
{
    /// <summary>
    /// One pillow in the queue. Forwards pointer events and plays its own feedback animations.
    /// Animations use unscaled time so the UI stays responsive when testing at a slower time scale.
    /// </summary>
    public sealed class QueueItemView : MonoBehaviour,
        IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private CanvasGroup canvasGroup;
        [Tooltip("Optional caption under or above the icon (e.g. \"DRAG ME\" / \"UP NEXT\").")]
        [SerializeField] private TMP_Text label;
        [Tooltip("Optional glow that pulses to draw attention (used on the current pillow).")]
        [SerializeField] private Graphic highlight;

        [Header("Look")]
        [Tooltip("Opacity when idle. Previews use a lower value so they read as \"not yet\".")]
        [SerializeField, Range(0f, 1f)] private float restAlpha = 1f;
        [SerializeField, Range(0f, 1f)] private float liftedAlpha = 0.3f;
        [SerializeField] private float pulseSpeed = 3f;

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float slideDuration = 0.25f;
        [SerializeField, Min(0.01f)] private float deniedDuration = 0.35f;
        [SerializeField] private float deniedShakeDistance = 12f;
        [SerializeField] private string deniedMessage = "Not yet!";

        private RectTransform _rectTransform;
        private Vector2 _restPosition;
        private Vector3 _restScale;
        private string _restLabel;
        private bool _isLifted;
        private Coroutine _animation;

        public event Action<QueueItemView> Pressed;
        public event Action<PointerEventData> DragStarted;
        public event Action<PointerEventData> Dragged;
        public event Action<PointerEventData> DragEnded;

        public RectTransform RectTransform => _rectTransform;
        public Vector2 RestPosition => _restPosition;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _restPosition = _rectTransform.anchoredPosition;
            _restScale = _rectTransform.localScale;
            _restLabel = label != null ? label.text : null;
            canvasGroup.alpha = restAlpha;
        }

        private void Update()
        {
            if (highlight == null)
                return;

            Color color = highlight.color;
            color.a = _isLifted ? 0f : 0.35f + 0.35f * Mathf.Sin(Time.unscaledTime * pulseSpeed);
            highlight.color = color;
        }

        public void Show(Color color)
        {
            gameObject.SetActive(true);
            icon.color = color;
        }

        public void Hide() => gameObject.SetActive(false);

        /// <summary>Dims the icon while its pillow is being dragged in the world.</summary>
        public void SetLifted(bool isLifted)
        {
            _isLifted = isLifted;
            canvasGroup.alpha = isLifted ? liftedAlpha : restAlpha;
        }

        /// <summary>Moves the item from another slot's position/size into its own resting slot.</summary>
        public void PlaySlideIn(Vector2 fromPosition, float fromScale, float fromAlpha) =>
            Play(SlideInRoutine(fromPosition, fromScale, fromAlpha));

        /// <summary>Quick bounce used when the pillow is swapped for a different color.</summary>
        public void PlayPunch() => Play(PunchRoutine());

        /// <summary>Shake plus a short message: this pillow can't be used yet.</summary>
        public void PlayDenied() => Play(DeniedRoutine());

        public void OnPointerDown(PointerEventData eventData) => Pressed?.Invoke(this);

        public void OnBeginDrag(PointerEventData eventData) => DragStarted?.Invoke(eventData);

        public void OnDrag(PointerEventData eventData) => Dragged?.Invoke(eventData);

        public void OnEndDrag(PointerEventData eventData) => DragEnded?.Invoke(eventData);

        private void Play(IEnumerator routine)
        {
            if (!isActiveAndEnabled)
                return;

            if (_animation != null)
                StopCoroutine(_animation);
            ResetToRest();
            _animation = StartCoroutine(routine);
        }

        private void ResetToRest()
        {
            _rectTransform.anchoredPosition = _restPosition;
            _rectTransform.localScale = _restScale;
            canvasGroup.alpha = _isLifted ? liftedAlpha : restAlpha;
            if (label != null)
                label.text = _restLabel;
        }

        private IEnumerator SlideInRoutine(Vector2 fromPosition, float fromScale, float fromAlpha)
        {
            for (float time = 0f; time < slideDuration; time += Time.unscaledDeltaTime)
            {
                float t = Easing.OutBack(time / slideDuration);
                _rectTransform.anchoredPosition = Vector2.LerpUnclamped(fromPosition, _restPosition, t);
                _rectTransform.localScale = _restScale * Mathf.LerpUnclamped(fromScale, 1f, t);
                canvasGroup.alpha = Mathf.Lerp(fromAlpha, restAlpha, time / slideDuration);
                yield return null;
            }
            ResetToRest();
            _animation = null;
        }

        private IEnumerator PunchRoutine()
        {
            for (float time = 0f; time < slideDuration; time += Time.unscaledDeltaTime)
            {
                _rectTransform.localScale = _restScale * (1f + 0.25f * Easing.Punch(time / slideDuration));
                yield return null;
            }
            ResetToRest();
            _animation = null;
        }

        private IEnumerator DeniedRoutine()
        {
            if (label != null)
                label.text = deniedMessage;

            for (float time = 0f; time < deniedDuration; time += Time.unscaledDeltaTime)
            {
                float t = time / deniedDuration;
                float offset = Mathf.Sin(t * Mathf.PI * 6f) * deniedShakeDistance * (1f - t);
                _rectTransform.anchoredPosition = _restPosition + new Vector2(offset, 0f);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.4f);
            ResetToRest();
            _animation = null;
        }
    }
}
