#nullable enable

using UnityEditor;
using UnityEngine;

namespace Ayla
{
    internal static class TimerMixerStylesheet
    {
        private static GUIStyle? s_RightMiniLabelStyle;
        public static GUIStyle RightMiniLabelStyle => s_RightMiniLabelStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleRight
        };

        private static GUIStyle? s_FinalTsStyle;
        public static GUIStyle FinalTsStyle => s_FinalTsStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleRight,
            normal = { textColor = new Color(0.55f, 0.55f, 0.55f) }
        };

        public const float kContentNameColWidth = 120f;
        public const float kContentFinalColWidth = 70f;
        public const float kContentColGap = 4f;

        public static readonly Color ColumnHeaderColor = new(0.22f, 0.22f, 0.22f);
        public static readonly Color SelectedBackgroundColor = new(0.17f, 0.36f, 0.53f, 0.3f);
    }
}
