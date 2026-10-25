using CatsVsPillows.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace CatsVsPillows.Visuals
{
    /// <summary>Applies a <see cref="LevelAtmosphere"/> (time of day) to the scene's sun, ambient light, camera and post-processing.</summary>
    public sealed class AtmosphereController : MonoBehaviour
    {
        [SerializeField] private Light sun;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Volume volume;

        public void Apply(LevelAtmosphere atmosphere)
        {
            if (atmosphere == null)
                return;

            if (sun != null)
            {
                sun.color = atmosphere.SunColor;
                sun.intensity = atmosphere.SunIntensity;
                sun.shadowStrength = atmosphere.ShadowStrength;
                sun.transform.rotation = atmosphere.SunRotation;
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = atmosphere.AmbientSky;
            RenderSettings.ambientEquatorColor = atmosphere.AmbientEquator;
            RenderSettings.ambientGroundColor = atmosphere.AmbientGround;

            if (targetCamera != null)
                targetCamera.backgroundColor = atmosphere.BackgroundColor;

            if (volume != null && atmosphere.PostProcessing != null)
                volume.sharedProfile = atmosphere.PostProcessing;
        }
    }
}
