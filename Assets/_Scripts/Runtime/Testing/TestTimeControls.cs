using UnityEngine;

namespace CatsVsPillows.Testing
{
    /// <summary>
    /// Editor / development-build helper to slow the game down (or speed it up) while testing.
    /// Change the speed from the inspector, the on-screen buttons, or the keyboard:
    /// <c>-</c> slower, <c>=</c> faster, <c>0</c> back to normal. The chosen speed survives level restarts.
    /// Release builds remove the component and always run at normal speed.
    /// </summary>
    public sealed class TestTimeControls : MonoBehaviour
    {
        private static readonly float[] Presets = { 0.1f, 0.25f, 0.5f, 0.75f, 1f, 1.5f, 2f };

        [Tooltip("Game speed while testing. 1 = normal.")]
        [SerializeField, Range(0.05f, 2f)] private float timeScale = 1f;
        [SerializeField] private bool showOverlay = true;
        [SerializeField] private KeyCode slowerKey = KeyCode.Minus;
        [SerializeField] private KeyCode fasterKey = KeyCode.Equals;
        [SerializeField] private KeyCode resetKey = KeyCode.Alpha0;

        // Remembers the speed across scene reloads within one play session.
        private static float? s_sessionTimeScale;

        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;
        private bool _isInitialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() => s_sessionTimeScale = null;

        private void Awake()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (s_sessionTimeScale.HasValue)
                timeScale = s_sessionTimeScale.Value;
            Apply(timeScale);
            _isInitialized = true;
#else
            Destroy(this);
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Unity also calls OnValidate when the scene loads (before Awake); only react to inspector edits after that.
        private void OnValidate()
        {
            if (Application.isPlaying && _isInitialized)
                Apply(timeScale);
        }

        private void Update()
        {
            if (Input.GetKeyDown(slowerKey))
                Step(-1);
            else if (Input.GetKeyDown(fasterKey))
                Step(1);
            else if (Input.GetKeyDown(resetKey))
                Apply(1f);
        }

        private void OnGUI()
        {
            if (!showOverlay)
                return;

            EnsureStyles();
            float scale = Screen.height / 1080f;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            var area = new Rect(20f, 1080f - 90f, 520f, 70f);
            GUI.Box(area, GUIContent.none);
            GUILayout.BeginArea(new Rect(area.x + 10f, area.y + 10f, area.width - 20f, area.height - 20f));
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Test speed {timeScale:0.##}x", _labelStyle, GUILayout.Width(230f));
            if (GUILayout.Button("-", _buttonStyle)) Step(-1);
            if (GUILayout.Button("+", _buttonStyle)) Step(1);
            if (GUILayout.Button("1x", _buttonStyle)) Apply(1f);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void Step(int direction)
        {
            int index = ClosestPresetIndex(timeScale) + direction;
            Apply(Presets[Mathf.Clamp(index, 0, Presets.Length - 1)]);
        }

        private void Apply(float value)
        {
            timeScale = Mathf.Clamp(value, 0.05f, 2f);
            s_sessionTimeScale = timeScale;
            Time.timeScale = timeScale;
        }

        private static int ClosestPresetIndex(float value)
        {
            int best = 0;
            for (int i = 1; i < Presets.Length; i++)
            {
                if (Mathf.Abs(Presets[i] - value) < Mathf.Abs(Presets[best] - value))
                    best = i;
            }
            return best;
        }

        private void EnsureStyles()
        {
            if (_labelStyle != null)
                return;

            _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, alignment = TextAnchor.MiddleLeft };
            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 28 };
        }
#endif
    }
}
