#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Ayla
{
    [CustomPropertyDrawer(typeof(RequiredInterfaceAttribute))]
    internal class RequiredInterfacePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.HelpBox(position, $"{nameof(RequiredInterfaceAttribute)} requires an Object reference field.", MessageType.Error);
                return;
            }

            var attr = (RequiredInterfaceAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);

            using (GUIScope.Changed())
            {
                var prev = property.objectReferenceValue;
                var next = EditorGUI.ObjectField(position, label, prev, attr.InterfaceType, true);
                if (GUI.changed)
                {
                    property.objectReferenceValue = next;
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif
