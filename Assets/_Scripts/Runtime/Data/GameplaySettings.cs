using CatsVsPillows.Gameplay;
using UnityEngine;

namespace CatsVsPillows.Data
{
    /// <summary>Global tuning and prefab references shared by every level.</summary>
    [CreateAssetMenu(fileName = "GameplaySettings", menuName = "Cats vs Pillows/Gameplay Settings")]
    public sealed class GameplaySettings : ScriptableObject
    {
        [Header("Grid")]
        [Tooltip("Number of pillow slots stacked in each gap.")]
        [SerializeField, Min(1)] private int slotsPerGap = 4;
        [Tooltip("Horizontal width of a gap (and of a pillow).")]
        [SerializeField, Min(0.1f)] private float gapWidth = 1.2f;
        [Tooltip("Height of one pillow slot. Furniture tops sit at slotsPerGap * slotHeight.")]
        [SerializeField, Min(0.05f)] private float slotHeight = 0.45f;
        [Tooltip("How many not-yet-crossed gaps the player may place pillows into (what the camera shows).")]
        [SerializeField, Min(1)] private int placeableGapCount = 2;

        [Header("Prefabs")]
        [SerializeField] private Pillow pillowPrefab;
        [SerializeField] private ParticleSystem pillowPopVfx;
        [SerializeField] private GameObject slotMarkerPrefab;
        [SerializeField] private CatController catPrefab;

        [Header("Room")]
        [SerializeField] private VisualStyle visualStyle;
        [Tooltip("Wallpaper/floor used by levels that don't pick their own Room Theme.")]
        [SerializeField] private RoomTheme defaultRoomTheme;
        [SerializeField, Min(1f)] private float wallHeight = 7f;
        [SerializeField] private float wallZ = 4f;
        [Tooltip("Extra floor/wall length added before the first and after the last platform.")]
        [SerializeField, Min(0f)] private float roomPadding = 12f;

        [Header("Cat")]
        [Tooltip("Distance from the start of the first platform where the cat spawns.")]
        [SerializeField, Min(0f)] private float catStartInset = 1f;
        [Tooltip("Distance before the end of the last platform where the cat stops and wins.")]
        [SerializeField, Min(0f)] private float goalInset = 1.2f;
        [Tooltip("Seconds before the cat starts walking.")]
        [SerializeField, Min(0f)] private float startDelay = 1.5f;

        [Header("Input")]
        [Tooltip("World-space distance within which a dragged pillow snaps to a slot.")]
        [SerializeField, Min(0.1f)] private float slotSnapRadius = 1.1f;

        [Header("Flow")]
        [Tooltip("Seconds between the cat falling and the level restarting.")]
        [SerializeField, Min(0f)] private float restartDelay = 2f;

        public int SlotsPerGap => slotsPerGap;
        public float GapWidth => gapWidth;
        public float SlotHeight => slotHeight;
        public float PlatformTopY => slotsPerGap * slotHeight;
        public int PlaceableGapCount => placeableGapCount;

        public Pillow PillowPrefab => pillowPrefab;
        public ParticleSystem PillowPopVfx => pillowPopVfx;
        public GameObject SlotMarkerPrefab => slotMarkerPrefab;
        public CatController CatPrefab => catPrefab;

        public VisualStyle Style => visualStyle;
        public RoomTheme DefaultRoomTheme => defaultRoomTheme;
        public float WallHeight => wallHeight;
        public float WallZ => wallZ;
        public float RoomPadding => roomPadding;

        public float CatStartInset => catStartInset;
        public float GoalInset => goalInset;
        public float StartDelay => startDelay;

        public float SlotSnapRadius => slotSnapRadius;
        public float RestartDelay => restartDelay;

        private void OnValidate() => AuthoringDataChanged.Notify();
    }
}
