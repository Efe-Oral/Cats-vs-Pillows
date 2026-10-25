using UnityEngine;

namespace CatsVsPillows.Utilities
{
    public static class ObjectUtility
    {
        /// <summary>Destroys an object in play mode and immediately in edit mode (e.g. when building previews).</summary>
        public static void DestroySafe(Object target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(target);
            else
                Object.DestroyImmediate(target);
        }
    }
}
