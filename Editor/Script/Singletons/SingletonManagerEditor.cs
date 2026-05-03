#nullable enable

using System;
using UnityEditor;

namespace Ayla
{
    [CustomEditor(typeof(SingletonManager))]
    internal class SingletonManagerEditor : Editor
    {
        private SerializedProperty m_Singletons = null!;
        private Editor[]? m_CachedEditors;
        private string[]? m_NicifyNames;

        private void OnEnable()
        {
            m_Singletons = serializedObject.FindProperty("m_Singletons");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            int arraySize = m_Singletons.arraySize;
            if (m_CachedEditors == null || m_CachedEditors.Length != arraySize)
            {
                m_CachedEditors = new Editor[arraySize];
            }

            if (m_NicifyNames == null || m_NicifyNames.Length != arraySize)
            {
                m_NicifyNames = new string[arraySize];
            }

            for (int i = 0; i < arraySize; ++i)
            {
                var element = m_Singletons.GetArrayElementAtIndex(i);
                ref string nicifyName = ref m_NicifyNames[i];
                if (string.IsNullOrEmpty(nicifyName))
                {
                    nicifyName = ObjectNames.NicifyVariableName(element.boxedValue.GetType().Name);
                }

                bool isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(element.isExpanded, nicifyName);
                try
                {
                    element.isExpanded = isExpanded;
                    if (isExpanded)
                    {
                        ref var ce = ref m_CachedEditors[i];

                        if (ce == null)
                        {
                            CreateCachedEditor(element.objectReferenceValue, null, ref ce);
                        }

                        if (ce == null)
                        {
                            throw new InvalidOperationException($"Failed to create editor for {nicifyName}.");
                        }

                        ce.OnInspectorGUI();
                    }
                }
                finally
                {
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
