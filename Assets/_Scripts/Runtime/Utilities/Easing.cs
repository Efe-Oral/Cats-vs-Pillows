using UnityEngine;

namespace CatsVsPillows.Utilities
{
    /// <summary>Easing curves for code-driven tweens. Input and output are normalized (0..1).</summary>
    public static class Easing
    {
        public static float OutQuad(float t) => 1f - (1f - t) * (1f - t);

        public static float InQuad(float t) => t * t;

        public static float OutBack(float t)
        {
            const float overshoot = 1.70158f;
            const float c3 = overshoot + 1f;
            float p = t - 1f;
            return 1f + c3 * p * p * p + overshoot * p * p;
        }

        /// <summary>Damped bounce from 0 back to 0, peaking early. Useful for squash/punch effects.</summary>
        public static float Punch(float t) => Mathf.Sin(t * Mathf.PI * 2f) * (1f - t);
    }
}
