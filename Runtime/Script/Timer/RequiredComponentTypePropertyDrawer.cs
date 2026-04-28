#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ayla
{
    [CustomPropertyDrawer(typeof(RequiredComponentTypeAttribute))]
    internal class RequiredComponentTypePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.HelpBox(position, $"{nameof(RequiredComponentTypeAttribute)} requires an Object reference field.", MessageType.Error);
                return;
            }

            var attr = (RequiredComponentTypeAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);

            using (GUIScope.Changed())
            {
                var prev = property.objectReferenceValue;
                var next = EditorGUI.ObjectField(position, label, prev, typeof(Component), true);
                if (GUI.changed)
                {
                    if (next == null || attr.ComponentTypes.Contains(next.GetType()))
                    {
                        property.objectReferenceValue = next;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Type Mismatch", $"The assigned component must be one of the following types:\n{string.Join("\n", attr.ComponentTypes.Select(t => t.Name))}", "OK");
                    }
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif
