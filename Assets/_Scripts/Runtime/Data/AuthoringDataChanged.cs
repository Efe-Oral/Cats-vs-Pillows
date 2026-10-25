using System;
using System.Diagnostics;

namespace CatsVsPillows.Data
{
    /// <summary>
    /// Editor-only signal raised from OnValidate when level authoring data changes, so editor tools
    /// (such as the Scene view level preview) can refresh. Calls compile away outside the editor.
    /// </summary>
    public static class AuthoringDataChanged
    {
#if UNITY_EDITOR
        public static event Action Changed;
#endif

        [Conditional("UNITY_EDITOR")]
        public static void Notify()
        {
#if UNITY_EDITOR
            Changed?.Invoke();
#endif
        }
    }
}
