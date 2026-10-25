using UnityEngine;

namespace CatsVsPillows.Visuals
{
    /// <summary>Gently wobbles a light's intensity (and an optional glow sprite) like a warm bulb or candle.</summary>
    [RequireComponent(typeof(Light))]
    public sealed class LightFlicker : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.3f)] private float amount = 0.04f;
        [SerializeField, Min(0f)] private float speed = 2.2f;
        [SerializeField] private SpriteRenderer glow;

        private Light _light;
        private float _baseIntensity;
        private Color _baseGlowColor;
        private float _seed;

        public void Configure(float flickerAmount, SpriteRenderer glowRenderer)
        {
            amount = flickerAmount;
            glow = glowRenderer;
        }

        private void Awake()
        {
            _light = GetComponent<Light>();
            _baseIntensity = _light.intensity;
            _seed = Random.value * 100f;
            if (glow != null)
                _baseGlowColor = glow.color;
        }

        private void Update()
        {
            float wobble = 1f + (Mathf.PerlinNoise(_seed, Time.time * speed) * 2f - 1f) * amount;
            _light.intensity = _baseIntensity * wobble;
            if (glow != null)
                glow.color = _baseGlowColor * wobble;
        }
    }
}
