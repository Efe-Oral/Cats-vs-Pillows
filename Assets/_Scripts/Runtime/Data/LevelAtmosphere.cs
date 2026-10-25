using UnityEngine;
using UnityEngine.Rendering;

namespace CatsVsPillows.Data
{
    /// <summary>Time-of-day mood for a level: sunlight, ambient light, post-processing and whether the lamps are on.</summary>
    [CreateAssetMenu(fileName = "Atmosphere", menuName = "Cats vs Pillows/Level Atmosphere")]
    public sealed class LevelAtmosphere : ScriptableObject
    {
        [Header("Sun / Moon")]
        [SerializeField] private Color sunColor = new Color(1f, 0.95f, 0.86f);
        [SerializeField, Min(0f)] private float sunIntensity = 1.1f;
        [SerializeField] private Vector3 sunRotation = new Vector3(42f, -38f, 0f);
        [SerializeField, Range(0f, 1f)] private float shadowStrength = 1f;

        [Header("Ambient")]
        [SerializeField] private Color ambientSky = new Color(0.95f, 0.9f, 1f);
        [SerializeField] private Color ambientEquator = new Color(0.9f, 0.8f, 0.85f);
        [SerializeField] private Color ambientGround = new Color(0.6f, 0.5f, 0.55f);

        [Header("Camera")]
        [SerializeField] private Color backgroundColor = new Color(1f, 0.9f, 0.8f);
        [Tooltip("Post-processing profile (bloom, color grading, vignette) for this mood.")]
        [SerializeField] private VolumeProfile postProcessing;

        [Header("Cozy Lights")]
        [Tooltip("Multiplies every lamp's light and glow. 0 = lamps off (daytime).")]
        [SerializeField, Range(0f, 2f)] private float lampIntensity = 1f;
        [SerializeField] private bool fairyLights = true;

        public Color SunColor => sunColor;
        public float SunIntensity => sunIntensity;
        public Quaternion SunRotation => Quaternion.Euler(sunRotation);
        public float ShadowStrength => shadowStrength;
        public Color AmbientSky => ambientSky;
        public Color AmbientEquator => ambientEquator;
        public Color AmbientGround => ambientGround;
        public Color BackgroundColor => backgroundColor;
        public VolumeProfile PostProcessing => postProcessing;
        public float LampIntensity => lampIntensity;
        public bool FairyLights => fairyLights;

        private void OnValidate() => AuthoringDataChanged.Notify();
    }
}
