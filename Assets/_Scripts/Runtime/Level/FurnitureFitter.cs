using UnityEngine;

namespace CatsVsPillows.Level
{
    /// <summary>Scales and positions arbitrary furniture models so they fill a platform's footprint.</summary>
    public static class FurnitureFitter
    {
        /// <summary>
        /// Scales <paramref name="instance"/> so its height matches <paramref name="height"/> (uniformly in Y/Z)
        /// and its X extent matches <paramref name="length"/>, then rests it on the floor centered at <paramref name="centerX"/>.
        /// </summary>
        public static void Fit(GameObject instance, float centerX, float length, float height)
        {
            Transform t = instance.transform;
            t.localScale = Vector3.one;
            t.position = Vector3.zero;

            if (!TryGetBounds(instance, out Bounds bounds) || bounds.size.y <= 0f || bounds.size.x <= 0f)
            {
                Debug.LogWarning($"Furniture '{instance.name}' has no renderers to fit.", instance);
                return;
            }

            float uniform = height / bounds.size.y;
            t.localScale = new Vector3(length / bounds.size.x, uniform, uniform);

            TryGetBounds(instance, out bounds);
            t.position += new Vector3(centerX - bounds.center.x, -bounds.min.y, -bounds.center.z);
        }

        private static bool TryGetBounds(GameObject instance, out Bounds bounds)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            bounds = default;
            if (renderers.Length == 0)
                return false;

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return true;
        }
    }
}
