using System.Collections;
using CatsVsPillows.Core;
using CatsVsPillows.Utilities;
using UnityEngine;

namespace CatsVsPillows.Gameplay
{
    /// <summary>Visual for a single pillow. Animations run on a child so the root stays on its slot.</summary>
    public sealed class Pillow : MonoBehaviour
    {
        [SerializeField] private Renderer pillowRenderer;
        [Tooltip("Child transform that is animated (squash, wobble, pop).")]
        [SerializeField] private Transform visual;

        [Header("Drop In")]
        [SerializeField, Min(0.01f)] private float dropDuration = 0.28f;
        [SerializeField] private float dropHeight = 0.35f;

        [Header("Pop")]
        [SerializeField, Min(0.01f)] private float popDuration = 0.16f;
        [SerializeField, Min(1f)] private float popScale = 1.35f;

        [Header("Clash Warning")]
        [SerializeField] private float wobbleAngle = 9f;
        [SerializeField] private float wobbleFrequency = 22f;

        private Vector3 _restScale;
        private bool _isWarning;
        private Coroutine _dropInRoutine;

        public PillowColor Color { get; private set; }

        private void Awake()
        {
            _restScale = visual.localScale;
        }

        private void Update()
        {
            if (_isWarning)
                visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * wobbleFrequency) * wobbleAngle);
        }

        public void SetColor(PillowColor color, Material material)
        {
            Color = color;
            pillowRenderer.sharedMaterial = material;
        }

        /// <summary>Wobbles the pillow to warn that it would explode with the pillow being dragged.</summary>
        public void SetWarning(bool isWarning)
        {
            _isWarning = isWarning;
            if (!isWarning)
                visual.localRotation = Quaternion.identity;
        }

        public void PlayDropIn()
        {
            StopDropIn();
            _dropInRoutine = StartCoroutine(DropInRoutine());
        }

        /// <summary>Plays the explosion after <paramref name="delay"/> seconds and destroys the pillow.</summary>
        public void Pop(ParticleSystem vfxPrefab, Color tint, float delay)
        {
            SetWarning(false);
            StartCoroutine(PopRoutine(vfxPrefab, tint, delay));
        }

        private void StopDropIn()
        {
            if (_dropInRoutine != null)
                StopCoroutine(_dropInRoutine);
            _dropInRoutine = null;
            visual.localPosition = Vector3.zero;
        }

        private IEnumerator DropInRoutine()
        {
            for (float time = 0f; time < dropDuration; time += Time.deltaTime)
            {
                float t = time / dropDuration;
                visual.localPosition = Vector3.up * (dropHeight * (1f - Easing.OutQuad(t)));
                float squash = Easing.Punch(t) * 0.25f;
                visual.localScale = Vector3.Scale(_restScale, new Vector3(1f + squash, 1f - squash, 1f + squash));
                yield return null;
            }
            visual.localPosition = Vector3.zero;
            visual.localScale = _restScale;
            _dropInRoutine = null;
        }

        private IEnumerator PopRoutine(ParticleSystem vfxPrefab, Color tint, float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            StopDropIn();
            for (float time = 0f; time < popDuration; time += Time.deltaTime)
            {
                float t = time / popDuration;
                float scale = t < 0.5f
                    ? Mathf.Lerp(1f, popScale, Easing.OutQuad(t * 2f))
                    : Mathf.Lerp(popScale, 0f, Easing.InQuad((t - 0.5f) * 2f));
                visual.localScale = _restScale * scale;
                yield return null;
            }

            SpawnVfx(vfxPrefab, tint);
            Destroy(gameObject);
        }

        private void SpawnVfx(ParticleSystem vfxPrefab, Color tint)
        {
            if (vfxPrefab == null)
                return;

            ParticleSystem vfx = Instantiate(vfxPrefab, visual.position, Quaternion.identity);
            ParticleSystem.MainModule main = vfx.main;
            main.startColor = tint;
            vfx.Play();
        }
    }
}
