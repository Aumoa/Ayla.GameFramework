using UnityEditor;
using UnityEngine;

namespace Ayla
{
    [CustomEditor(typeof(TimerMixer))]
    public class TimerMixerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button(TimerMixerText.OpenEditor))
            {
                TimerMixerEditorWindow.OpenWindow();
            }
        }
    }
}
