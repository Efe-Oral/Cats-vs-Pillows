using UnityEngine;

namespace CatsVsPillows.Data
{
    /// <summary>Wallpaper and floor of the room. Levels pick a theme; several levels can share one.</summary>
    [CreateAssetMenu(fileName = "RoomTheme", menuName = "Cats vs Pillows/Room Theme")]
    public sealed class RoomTheme : ScriptableObject
    {
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material floorMaterial;
        [Tooltip("Used for platforms without a furniture prefab.")]
        [SerializeField] private Material fallbackPlatformMaterial;

        public Material WallMaterial => wallMaterial;
        public Material FloorMaterial => floorMaterial;
        public Material FallbackPlatformMaterial => fallbackPlatformMaterial;

        private void OnValidate() => AuthoringDataChanged.Notify();
    }
}
