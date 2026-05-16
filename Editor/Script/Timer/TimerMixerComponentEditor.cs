#nullable enable

using System;
using UnityEditor;

namespace Ayla
{
    [CustomEditor(typeof(TimerMixerComponent), true)]
    public class TimerMixerComponentEditor : Editor
    {
        private SerializedProperty m_Channel = null!;

        protected virtual void OnEnable()
        {
            m_Channel = serializedObject.FindProperty("m_Channel")
                ?? throw new InvalidOperationException("Failed to find serialized property 'm_Channel'.");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var scriptProp = serializedObject.FindProperty("m_Script");
            if (scriptProp != null)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(scriptProp);
                }
            }

            EditorGUILayout.PropertyField(m_Channel);

            DrawPropertiesExcluding(serializedObject, "m_Script", "m_Channel");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
